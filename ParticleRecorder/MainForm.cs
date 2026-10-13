using ParticleRecorder.Effects;

namespace ParticleRecorder;

public sealed partial class MainForm : Form
{
    private readonly ParticleSettings _settings = new();

    public MainForm()
    {
        InitializeComponent();

        // Runtime data the designer can't hold: the settings object and the shared image filter.
        propertyGrid.SelectedObject = _settings;
        openTextureDialog.Filter = ImageFileNameEditor.ImageFilter;
    }

    private void propertyGrid_PropertyValueChanged(object? sender, PropertyValueChangedEventArgs e) =>
        SendSetting(e.ChangedItem?.PropertyDescriptor?.Name);

    private void viewport_MessageReceived(object? sender, string message) => HandleGameMessage(message);

    private void restartButton_Click(object? sender, EventArgs e) => viewport.Post("restart");

    private void loadTextureButton_Click(object? sender, EventArgs e) => ChooseTexture();

    private void clearTextureButton_Click(object? sender, EventArgs e) => SetTexture(string.Empty);

    private void ChooseTexture()
    {
        if (!string.IsNullOrEmpty(_settings.Texture))
            openTextureDialog.InitialDirectory = Path.GetDirectoryName(_settings.Texture);

        if (openTextureDialog.ShowDialog(this) == DialogResult.OK) SetTexture(openTextureDialog.FileName);
    }

    private void SetTexture(string path)
    {
        _settings.Texture = path;
        propertyGrid.Refresh();
        statusLabel.Text = string.IsNullOrEmpty(path) ? "Drawing circles" : $"Texture: {Path.GetFileName(path)}";
        SendSetting(nameof(ParticleSettings.Texture));
    }

    private void SendSetting(string? propertyName)
    {
        if (propertyName == null) return;

        var message = _settings.ToMessage(propertyName);
        if (message != null) viewport.Post(message);
    }

    private void HandleGameMessage(string message)
    {
        var separator = message.IndexOf(' ');
        var name = separator < 0 ? message : message[..separator];
        var argument = separator < 0 ? string.Empty : message[(separator + 1)..];

        switch (name)
        {
            case "ready":
                foreach (var setting in _settings.ToMessages()) viewport.Post(setting);
                statusLabel.Text = "Preview running";
                break;
            case "stats":
                particleCountLabel.Text = $"Particles: {argument}";
                break;
            case "error":
                statusLabel.Text = argument;
                break;
        }
    }
}
