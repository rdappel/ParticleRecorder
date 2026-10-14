using System.Globalization;
using System.Text.Json;
using ParticleRecorder.Effects;

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

    // The reply the export flow is waiting for ("measured"/"exported", or an error).
    private TaskCompletionSource<string>? _pendingReply;

    public MainForm()
    {
        InitializeComponent();

        // Runtime data the designer can't hold: the settings object and the shared image filter.
        propertyGrid.SelectedObject = _settings;
        openTextureDialog.Filter = ImageFileNameEditor.ImageFilter;
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

    private async void exportMenuItem_Click(object? sender, EventArgs e) => await ExportAnimationAsync();

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
        MarkDirty();
    }

    // ---- Menu: View ----

    private void restartPreviewMenuItem_Click(object? sender, EventArgs e) => viewport.Post("restart");

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
        viewport.Post("restart");
    }

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
            case "measured" or "exported":
                _pendingReply?.TrySetResult(message);
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
        _settings = particles;
        _recording = recording;
        _effectPath = path;
        _isDirty = false;

        propertyGrid.SelectedObject = _settings;
        SendAllSettings();
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
        if (_isDirty) return;

        _isDirty = true;
        UpdateTitle();
    }

    private void UpdateTitle() => Text = $"{EffectName}{(_isDirty ? "*" : string.Empty)} - {AppName}";

    // ---- Recording ----

    private async Task ExportAnimationAsync()
    {
        if (!viewport.IsRunning)
        {
            statusLabel.Text = "The preview isn't running, so nothing can be recorded.";
            return;
        }

        exportDialog.FileName = EffectName + ".png";
        if (exportDialog.ShowDialog(this) != DialogResult.OK) return;

        var pngPath = exportDialog.FileName;
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
                    "Fewer frames, a lower frame rate or a smaller effect will reduce it.\n\nExport anyway?",
                    AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (answer != DialogResult.Yes)
                {
                    statusLabel.Text = "Export cancelled";
                    return;
                }
            }

            statusLabel.Text = "Recording...";
            var exported = RecordingResult.Parse(await RequestAsync($"export {options} path={pngPath}"));
            statusLabel.Text = $"Exported {Path.GetFileName(pngPath)} + {Path.GetFileNameWithoutExtension(pngPath)}.anim: " +
                $"{exported.Frames} frames of {exported.FrameWidth}×{exported.FrameHeight} ({exported.SheetWidth}×{exported.SheetHeight})";
        }
        catch (RecordingException ex)
        {
            statusLabel.Text = $"Export failed: {ex.Message}";
            MessageBox.Show(this, $"Could not export the animation:\n{ex.Message}", AppName,
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
        menuStrip.Enabled = propertyGrid.Enabled = !busy;
        if (statusText != null) statusLabel.Text = statusText;
    }

    private sealed class RecordingException(string message) : Exception(message);

    /// <summary>A "measured"/"exported" reply: frames, frame size and sheet size.</summary>
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
