using System.Globalization;
using System.Numerics;
using System.Text.Json;
using ParticleRecorder.Effects;
using ParticleRecorder.Paths;

namespace ParticleRecorder;

public sealed partial class MainForm : Form
{
    private const string AppName = "Particle Recorder";

    /// <summary>Sheets wider or taller than this may not load on some graphics cards.</summary>
    private const int SafeTextureSize = 4096;

    private ParticleSettings _settings = new();
    private RecordingSettings _recording = new();
    private string? _effectPath;
    private bool _isDirty;

    // The reply a recording request is waiting for ("measured", "recorded" or "saved", or an error).
    private TaskCompletionSource<string>? _pendingReply;

    // Edits _settings.Path from the viewport's mouse messages while path editing is on.
    private readonly PenTool _penTool = new();
    private bool _isSyncingPathControls;

    public MainForm()
    {
        InitializeComponent();

        // Runtime data the designer can't hold: the settings object and the shared image filter.
        propertyGrid.SelectedObject = _settings;
        openTextureDialog.Filter = ImageFileNameEditor.ImageFilter;
        backgroundComboBox.SelectedIndex = 0;
        zoomComboBox.SelectedIndex = 0;
        _penTool.SetPath(_settings.Path);
        UpdateTitle();
    }

    // ---- Menu: File ----

    private void newMenuItem_Click(object? sender, EventArgs e)
    {
        if (!ConfirmDiscardChanges()) return;

        LoadEffect(new ParticleSettings(), new RecordingSettings(), path: null);
        statusLabel.Text = "New effect";
    }

    private void openMenuItem_Click(object? sender, EventArgs e)
    {
        if (!ConfirmDiscardChanges()) return;
        if (openEffectDialog.ShowDialog(this) != DialogResult.OK) return;

        var path = openEffectDialog.FileName;
        try
        {
            var (particles, recording) = EffectDocument.Load(path);
            LoadEffect(particles, recording, path);
            statusLabel.Text = $"Opened {Path.GetFileName(path)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        {
            MessageBox.Show(this, $"Could not open {Path.GetFileName(path)}:\n{ex.Message}", AppName,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void saveMenuItem_Click(object? sender, EventArgs e) => SaveEffect(saveAs: false);

    private void saveAsMenuItem_Click(object? sender, EventArgs e) => SaveEffect(saveAs: true);

    private async void recordMenuItem_Click(object? sender, EventArgs e) => await RecordAnimationAsync();

    private async void saveAnimationMenuItem_Click(object? sender, EventArgs e) => await SaveAnimationAsync();

    private void exitMenuItem_Click(object? sender, EventArgs e) => Close();

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_pendingReply != null || !ConfirmDiscardChanges()) e.Cancel = true;
    }

    // ---- Menu: Edit ----

    private void loadTextureMenuItem_Click(object? sender, EventArgs e) => ChooseTexture();

    private void clearTextureMenuItem_Click(object? sender, EventArgs e) => SetTexture(string.Empty);

    private void recordingSettingsMenuItem_Click(object? sender, EventArgs e)
    {
        using var dialog = new RecordingSettingsDialog { Settings = _recording.Clone() };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        _recording = dialog.Settings;
        SendPathDuration();
        MarkDirty();
    }

    private void emitterPathMenuItem_Click(object? sender, EventArgs e) => SetPathEditing(!IsEditingPath);

    // ---- Menu: View ----

    private void restartPreviewMenuItem_Click(object? sender, EventArgs e)
    {
        if (!IsPreviewingAnimation)
        {
            viewport.Post("restart");
            return;
        }

        viewport.Post("preview frame 0");
        viewport.Post("preview play");
    }

    private void settingsPanelMenuItem_CheckedChanged(object? sender, EventArgs e) =>
        propertyGrid.Visible = splitter.Visible = settingsPanelMenuItem.Checked;

    // ---- Settings and preview ----

    private void propertyGrid_PropertyValueChanged(object? sender, PropertyValueChangedEventArgs e)
    {
        SendSetting(e.ChangedItem?.PropertyDescriptor?.Name);
        MarkDirty();
    }

    private void viewport_MessageReceived(object? sender, string message) => HandleGameMessage(message);

    private void ChooseTexture()
    {
        if (!string.IsNullOrEmpty(_settings.Texture))
            openTextureDialog.InitialDirectory = Path.GetDirectoryName(_settings.Texture);

        if (openTextureDialog.ShowDialog(this) == DialogResult.OK) SetTexture(openTextureDialog.FileName);
    }

    private void SetTexture(string path)
    {
        if (_settings.Texture == path) return;

        _settings.Texture = path;
        propertyGrid.Refresh();
        statusLabel.Text = string.IsNullOrEmpty(path) ? "Drawing circles" : $"Texture: {Path.GetFileName(path)}";
        SendSetting(nameof(ParticleSettings.Texture));
        MarkDirty();
    }

    private void SendSetting(string? propertyName)
    {
        if (propertyName == null) return;

        var message = _settings.ToMessage(propertyName);
        if (message != null) viewport.Post(message);
    }

    private void SendAllSettings()
    {
        foreach (var setting in _settings.ToMessages()) viewport.Post(setting);
        SendPathDuration();
        viewport.Post("restart");
    }

    private void SendPathDuration() =>
        viewport.Post(string.Create(CultureInfo.InvariantCulture, $"duration {_recording.Duration}"));

    private void HandleGameMessage(string message)
    {
        var separator = message.IndexOf(' ');
        var name = separator < 0 ? message : message[..separator];
        var argument = separator < 0 ? string.Empty : message[(separator + 1)..];

        switch (name)
        {
            case "ready":
                SendAllSettings();
                statusLabel.Text = "Preview running";
                break;
            case "stats":
                particleCountLabel.Text = $"Particles: {argument}";
                break;
            case "measured" or "recorded" or "saved":
                _pendingReply?.TrySetResult(message);
                break;
            case "frame":
                ShowPlaybackPosition(argument);
                break;
            case "mouse":
                HandlePenMouse(argument);
                break;
            case "error" when _pendingReply != null:
                _pendingReply.TrySetException(new RecordingException(argument));
                break;
            case "error":
                statusLabel.Text = argument;
                break;
        }
    }

    // ---- Effect files ----

    private void LoadEffect(ParticleSettings particles, RecordingSettings recording, string? path)
    {
        ClosePreview();

        _settings = particles;
        _recording = recording;
        _effectPath = path;
        _isDirty = false;

        propertyGrid.SelectedObject = _settings;
        _penTool.SetPath(_settings.Path);
        SendAllSettings();
        if (IsEditingPath) viewport.Post("pen select -1");
        SyncPathControls();
        UpdateTitle();
    }

    /// <summary>Returns false if the user cancelled or the save failed.</summary>
    private bool SaveEffect(bool saveAs)
    {
        var path = _effectPath;
        if (saveAs || path == null)
        {
            saveEffectDialog.FileName = Path.GetFileName(path) ?? "effect" + EffectDocument.Extension;
            if (saveEffectDialog.ShowDialog(this) != DialogResult.OK) return false;
            path = saveEffectDialog.FileName;
        }

        try
        {
            EffectDocument.Save(path, _settings, _recording);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"Could not save {Path.GetFileName(path)}:\n{ex.Message}", AppName,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        _effectPath = path;
        _isDirty = false;
        UpdateTitle();
        statusLabel.Text = $"Saved {Path.GetFileName(path)}";
        return true;
    }

    /// <summary>Asks to save unsaved changes. Returns false if the user cancelled.</summary>
    private bool ConfirmDiscardChanges()
    {
        if (!_isDirty) return true;

        var answer = MessageBox.Show(this, $"Save changes to {EffectName}?", AppName,
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
        return answer switch
        {
            DialogResult.Yes => SaveEffect(saveAs: false),
            DialogResult.No => true,
            _ => false,
        };
    }

    private string EffectName => _effectPath == null ? "Untitled" : Path.GetFileNameWithoutExtension(_effectPath);

    private void MarkDirty()
    {
        if (IsPreviewingAnimation) statusLabel.Text = "The effect changed since this recording: Record Again to update the preview.";
        if (_isDirty) return;

        _isDirty = true;
        UpdateTitle();
    }

    private void UpdateTitle() => Text = $"{EffectName}{(_isDirty ? "*" : string.Empty)} - {AppName}";

    // ---- Recording ----

    // True while the animation preview (and its bar) is shown instead of the live effect.
    private bool IsPreviewingAnimation { get; set; }

    /// <summary>Records the effect into memory and shows it in the animation preview, ready to save.</summary>
    private async Task RecordAnimationAsync()
    {
        if (!viewport.IsRunning)
        {
            statusLabel.Text = "The preview isn't running, so nothing can be recorded.";
            return;
        }

        SetPathEditing(false);
        var options = _recording.ToOptions(seed: (uint)Random.Shared.Next(1, int.MaxValue));

        SetBusy(true, "Measuring the animation...");
        try
        {
            var measured = RecordingResult.Parse(await RequestAsync($"measure {options}"));
            if (measured.SheetWidth > SafeTextureSize || measured.SheetHeight > SafeTextureSize)
            {
                var answer = MessageBox.Show(this,
                    $"The sprite sheet will be {measured.SheetWidth}×{measured.SheetHeight} pixels " +
                    $"({measured.Frames} frames of {measured.FrameWidth}×{measured.FrameHeight}).\n\n" +
                    $"Some graphics cards can't load textures larger than {SafeTextureSize}×{SafeTextureSize}. " +
                    "Fewer frames, a lower frame rate or a smaller effect will reduce it.\n\nRecord anyway?",
                    AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (answer != DialogResult.Yes)
                {
                    statusLabel.Text = "Recording cancelled";
                    return;
                }
            }

            statusLabel.Text = "Recording...";
            var recorded = RecordingResult.Parse(await RequestAsync($"record {options}"));
            ShowPreview(recorded);
        }
        catch (RecordingException ex)
        {
            statusLabel.Text = $"Recording failed: {ex.Message}";
            MessageBox.Show(this, $"Could not record the animation:\n{ex.Message}", AppName,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _pendingReply = null;
            SetBusy(false, statusText: null);
        }
    }

    /// <summary>Writes the previewed recording as a .png sprite sheet plus a matching .anim.</summary>
    private async Task SaveAnimationAsync()
    {
        if (!IsPreviewingAnimation) return;

        saveAnimationDialog.FileName = EffectName + ".png";
        if (saveAnimationDialog.ShowDialog(this) != DialogResult.OK) return;

        var pngPath = saveAnimationDialog.FileName;
        SetBusy(true, "Saving...");
        try
        {
            await RequestAsync($"save {pngPath}");
            statusLabel.Text = $"Saved {Path.GetFileName(pngPath)} + {Path.GetFileNameWithoutExtension(pngPath)}.anim";
        }
        catch (RecordingException ex)
        {
            statusLabel.Text = $"Save failed: {ex.Message}";
            MessageBox.Show(this, $"Could not save the animation:\n{ex.Message}", AppName,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _pendingReply = null;
            SetBusy(false, statusText: null);
        }
    }

    private Task<string> RequestAsync(string message)
    {
        _pendingReply = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        viewport.Post(message);
        return _pendingReply.Task;
    }

    private void SetBusy(bool busy, string? statusText)
    {
        UseWaitCursor = busy;
        menuStrip.Enabled = propertyGrid.Enabled = previewPanel.Enabled = !busy;
        if (statusText != null) statusLabel.Text = statusText;
    }

    // ---- Emitter path ----

    // True while the path bar is shown and the viewport's mouse edits the path.
    private bool IsEditingPath { get; set; }

    private void SetPathEditing(bool editing)
    {
        if (editing == IsEditingPath) return;

        if (editing) ClosePreview();
        IsEditingPath = editing;
        pathPanel.Visible = emitterPathMenuItem.Checked = editing;
        viewport.Post(editing ? "pen on" : "pen off");
        viewport.Post($"pen select {_penTool.SelectedIndex}");
        SyncPathControls();
        statusLabel.Text = editing ? "Editing the emitter path" : "Editing";
    }

    private void doneButton_Click(object? sender, EventArgs e) => SetPathEditing(false);

    private void closedPathCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        if (_isSyncingPathControls) return;

        _settings.Path.Closed = closedPathCheckBox.Checked;
        PathChanged();
    }

    private void clearPathButton_Click(object? sender, EventArgs e)
    {
        if (_penTool.Clear()) PathChanged();
    }

    private void deletePointButton_Click(object? sender, EventArgs e)
    {
        if (_penTool.DeleteSelected()) PathChanged();
    }

    private void pointRateInput_ValueChanged(object? sender, EventArgs e)
    {
        if (_isSyncingPathControls || _penTool.SelectedPoint is not { } point) return;

        point.Rate = (float)pointRateInput.Value;
        PathChanged();
    }

    /// <summary>Handles "mouse down|up x y left|right" and "mouse move x y" from the viewport.</summary>
    private void HandlePenMouse(string argument)
    {
        if (!IsEditingPath) return;

        var parts = argument.Split(' ');
        if (parts.Length < 3
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) return;

        var position = new Vector2(x, y);
        var button = parts.Length > 3 && parts[3] == "right" ? MouseButtons.Right : MouseButtons.Left;
        var selected = _penTool.SelectedIndex;

        var changed = parts[0] switch
        {
            "down" => _penTool.MouseDown(position, button, Environment.TickCount64),
            "move" => _penTool.MouseMove(position),
            _ => EndDrag(),
        };

        if (changed) PathChanged();
        else if (_penTool.SelectedIndex != selected) SyncPathSelection();
    }

    private bool EndDrag()
    {
        _penTool.MouseUp();
        return false;
    }

    private void PathChanged()
    {
        SendSetting(nameof(ParticleSettings.Path));
        SyncPathSelection();
        MarkDirty();
    }

    private void SyncPathSelection()
    {
        viewport.Post($"pen select {_penTool.SelectedIndex}");
        SyncPathControls();
    }

    private void SyncPathControls()
    {
        _isSyncingPathControls = true;
        closedPathCheckBox.Checked = _settings.Path.Closed;
        clearPathButton.Enabled = !_settings.Path.IsEmpty;

        var point = _penTool.SelectedPoint;
        pointRateInput.Enabled = deletePointButton.Enabled = point != null;
        if (point != null) pointRateInput.Value = Math.Clamp((decimal)point.Rate, pointRateInput.Minimum, pointRateInput.Maximum);
        _isSyncingPathControls = false;
    }

    // ---- Animation preview ----

    private void ShowPreview(RecordingResult recorded)
    {
        frameTrackBar.Maximum = recorded.Frames - 1;
        ShowPlaybackPosition(frame: 0, isPlaying: true);
        previewPanel.Visible = IsPreviewingAnimation = true;
        saveAnimationMenuItem.Enabled = true;
        particleCountLabel.Visible = false;

        // A new recording starts with the native defaults; keep the viewer's choices.
        SendPreviewLooping();
        SendPreviewBackground();
        SendPreviewZoom();

        statusLabel.Text = $"Recorded {recorded.Frames} frames of {recorded.FrameWidth}×{recorded.FrameHeight} " +
            $"(sheet {recorded.SheetWidth}×{recorded.SheetHeight}). Save it, record again, or go back to the editor.";
    }

    private void ClosePreview()
    {
        if (!IsPreviewingAnimation) return;

        viewport.Post("close");
        previewPanel.Visible = IsPreviewingAnimation = false;
        saveAnimationMenuItem.Enabled = false;
        particleCountLabel.Visible = true;
        statusLabel.Text = "Editing";
    }

    private void backButton_Click(object? sender, EventArgs e) => ClosePreview();

    private void playButton_Click(object? sender, EventArgs e) =>
        viewport.Post(playButton.Text == "Play" ? "preview play" : "preview pause");

    private void previousFrameButton_Click(object? sender, EventArgs e) => StepFrame(-1);

    private void nextFrameButton_Click(object? sender, EventArgs e) => StepFrame(1);

    private void frameTrackBar_Scroll(object? sender, EventArgs e) => viewport.Post($"preview frame {frameTrackBar.Value}");

    private void loopPreviewCheckBox_CheckedChanged(object? sender, EventArgs e) => SendPreviewLooping();

    private void backgroundComboBox_SelectedIndexChanged(object? sender, EventArgs e) => SendPreviewBackground();

    private void zoomComboBox_SelectedIndexChanged(object? sender, EventArgs e) => SendPreviewZoom();

    private void StepFrame(int offset)
    {
        var frame = Math.Clamp(frameTrackBar.Value + offset, 0, frameTrackBar.Maximum);
        viewport.Post("preview pause");
        viewport.Post($"preview frame {frame}");
    }

    private void SendPreviewLooping()
    {
        if (!IsPreviewingAnimation) return;

        viewport.Post($"preview loop {(loopPreviewCheckBox.Checked ? 1 : 0)}");
    }

    private void SendPreviewBackground()
    {
        if (!IsPreviewingAnimation) return;

        string[] backgrounds = ["dark", "light", "checker"];
        viewport.Post($"preview background {backgrounds[Math.Max(0, backgroundComboBox.SelectedIndex)]}");
    }

    private void SendPreviewZoom()
    {
        if (!IsPreviewingAnimation) return;

        int[] zooms = [0, 1, 2, 4];   // 0 = fit
        viewport.Post($"preview zoom {zooms[Math.Max(0, zoomComboBox.SelectedIndex)]}");
    }

    /// <summary>Handles a "frame &lt;index&gt; &lt;playing&gt;" message from the preview.</summary>
    private void ShowPlaybackPosition(string argument)
    {
        var parts = argument.Split(' ');
        if (parts.Length == 2 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var frame))
            ShowPlaybackPosition(frame, isPlaying: parts[1] == "1");
    }

    private void ShowPlaybackPosition(int frame, bool isPlaying)
    {
        frameTrackBar.Value = Math.Clamp(frame, 0, frameTrackBar.Maximum);
        frameLabel.Text = $"Frame {frameTrackBar.Value + 1} / {frameTrackBar.Maximum + 1}";
        playButton.Text = isPlaying ? "Pause" : "Play";
    }

    private sealed class RecordingException(string message) : Exception(message);

    /// <summary>A "measured"/"recorded" reply: frames, frame size and sheet size.</summary>
    private readonly record struct RecordingResult(int Frames, int FrameWidth, int FrameHeight, int SheetWidth, int SheetHeight)
    {
        public static RecordingResult Parse(string reply)
        {
            var parts = reply.Split(' ');
            var values = new int[5];
            if (parts.Length != values.Length + 1) throw new RecordingException($"unexpected reply: {reply}");

            for (var i = 0; i < values.Length; i++)
            {
                if (!int.TryParse(parts[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out values[i]))
                    throw new RecordingException($"unexpected reply: {reply}");
            }

            return new RecordingResult(values[0], values[1], values[2], values[3], values[4]);
        }
    }
}
