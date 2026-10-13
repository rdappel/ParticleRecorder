using KatanaEngine.WinForms;
using ParticleRecorder.Effects;

namespace ParticleRecorder;

public sealed class MainForm : Form
{
    private const string NativeGameLibrary = "ParticleRecorder.Native.dll";
    private const int PropertyPanelWidth = 340;

    private readonly ParticleSettings _settings = new();
    private readonly PropertyGrid _propertyGrid;
    private readonly KatanaViewport _viewport;
    private readonly ToolStripStatusLabel _statusLabel;
    private readonly ToolStripStatusLabel _particleCountLabel;

    public MainForm()
    {
        Text = "Particle Recorder";
        ClientSize = new Size(1400, 850);
        StartPosition = FormStartPosition.CenterScreen;

        _propertyGrid = new PropertyGrid
        {
            Dock = DockStyle.Left,
            Width = PropertyPanelWidth,
            SelectedObject = _settings,
            ToolbarVisible = false,
        };
        _propertyGrid.PropertyValueChanged += (_, e) => SendSetting(e.ChangedItem?.PropertyDescriptor?.Name);

        _viewport = new KatanaViewport { Dock = DockStyle.Fill };
        _viewport.MessageReceived += (_, message) => HandleGameMessage(message);

        var splitter = new Splitter { Dock = DockStyle.Left };

        var restartButton = new ToolStripButton("Restart Effect");
        restartButton.Click += (_, _) => _viewport.Post("restart");
        var toolStrip = new ToolStrip(restartButton) { GripStyle = ToolStripGripStyle.Hidden };

        _statusLabel = new ToolStripStatusLabel("Starting preview...") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
        _particleCountLabel = new ToolStripStatusLabel("Particles: 0");
        var statusStrip = new StatusStrip();
        statusStrip.Items.AddRange([_statusLabel, _particleCountLabel]);

        // Docking is applied in reverse order of addition: fill first, then the left-docked pair.
        Controls.Add(_viewport);
        Controls.Add(splitter);
        Controls.Add(_propertyGrid);
        Controls.Add(toolStrip);
        Controls.Add(statusStrip);

        _viewport.GameLibraryPath = NativeGameLibrary;
    }

    private void SendSetting(string? propertyName)
    {
        if (propertyName == null) return;

        var message = _settings.ToMessage(propertyName);
        if (message != null) _viewport.Post(message);
    }

    private void HandleGameMessage(string message)
    {
        var separator = message.IndexOf(' ');
        var name = separator < 0 ? message : message[..separator];
        var argument = separator < 0 ? string.Empty : message[(separator + 1)..];

        switch (name)
        {
            case "ready":
                foreach (var setting in _settings.ToMessages()) _viewport.Post(setting);
                _statusLabel.Text = "Preview running";
                break;
            case "stats":
                _particleCountLabel.Text = $"Particles: {argument}";
                break;
            case "error":
                _statusLabel.Text = argument;
                break;
        }
    }
}
