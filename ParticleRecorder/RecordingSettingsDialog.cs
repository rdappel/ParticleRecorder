using System.ComponentModel;
using ParticleRecorder.Effects;

namespace ParticleRecorder;

/// <summary>Edits a copy of the recording settings; read <see cref="Settings"/> after OK.</summary>
public sealed partial class RecordingSettingsDialog : Form
{
    public RecordingSettingsDialog()
    {
        InitializeComponent();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RecordingSettings Settings
    {
        get => new()
        {
            Fps = (int)fpsInput.Value,
            Duration = (float)durationInput.Value,
            PrewarmEnabled = prewarmCheckBox.Checked,
            PrewarmSeconds = (float)prewarmInput.Value,
            Loop = loopCheckBox.Checked,
            Padding = (int)paddingInput.Value,
            Columns = (int)columnsInput.Value,
        };
        set
        {
            fpsInput.Value = Clamp(value.Fps, fpsInput);
            durationInput.Value = Clamp(Math.Round((decimal)value.Duration, 2), durationInput);
            prewarmCheckBox.Checked = value.PrewarmEnabled;
            prewarmInput.Value = Clamp(Math.Round((decimal)value.PrewarmSeconds, 2), prewarmInput);
            loopCheckBox.Checked = value.Loop;
            paddingInput.Value = Clamp(value.Padding, paddingInput);
            columnsInput.Value = Clamp(value.Columns, columnsInput);
            UpdateDependentControls();
        }
    }

    private void timing_ValueChanged(object? sender, EventArgs e) => UpdateDependentControls();

    private void prewarmCheckBox_CheckedChanged(object? sender, EventArgs e) => UpdateDependentControls();

    private void UpdateDependentControls()
    {
        var frames = Settings.FrameCount;
        frameCountLabel.Text = frames == 1 ? "= 1 frame" : $"= {frames} frames";
        prewarmLabel.Enabled = prewarmInput.Enabled = prewarmCheckBox.Checked;
    }

    private static decimal Clamp(decimal value, NumericUpDown input) => Math.Clamp(value, input.Minimum, input.Maximum);
}
