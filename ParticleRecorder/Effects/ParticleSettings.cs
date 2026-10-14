using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Text.Json.Serialization;
using DrawingColor = System.Drawing.Color;

namespace ParticleRecorder.Effects;

/// <summary>How particles combine with what is behind them.</summary>
public enum ParticleBlend
{
    /// <summary>Normal transparency: particles cover what is behind them.</summary>
    Alpha,

    /// <summary>Particles add light: overlapping particles get brighter (fire, sparks, glow).</summary>
    Additive,
}

/// <summary>
/// The effect being edited, including how it is timed and recorded. C# owns this data; the
/// native preview receives every change as a "set &lt;Name&gt; &lt;value&gt;" message (the
/// Sprite Sheet settings only travel with "measure"/"record", see <see cref="ToRecordingOptions"/>).
/// Must stay in sync with ParticleRecorder.Native/Source/Particles/ParticleSettings.h.
/// </summary>
public sealed class ParticleSettings
{
    [Category("Emission"), Description("Maximum number of live particles.")]
    public int MaxParticles { get; set; } = 500;

    [Category("Emission"), Description("Particles emitted per second.")]
    public float EmissionRate { get; set; } = 60f;

    [Category("Lifetime"), Description("Shortest particle lifetime, in seconds.")]
    public float LifetimeMin { get; set; } = 0.6f;

    [Category("Lifetime"), Description("Longest particle lifetime, in seconds.")]
    public float LifetimeMax { get; set; } = 1.4f;

    [Category("Motion"), Description("Slowest initial speed, in pixels per second.")]
    public float SpeedMin { get; set; } = 60f;

    [Category("Motion"), Description("Fastest initial speed, in pixels per second.")]
    public float SpeedMax { get; set; } = 160f;

    [Category("Motion"), Description("Emission direction in degrees (0 = right, -90 = up).")]
    public float Direction { get; set; } = -90f;

    [Category("Motion"), Description("Random spread either side of Direction, in degrees.")]
    public float Spread { get; set; } = 40f;

    [Category("Motion"), Description("Downward acceleration, in pixels per second squared.")]
    public float Gravity { get; set; } = 120f;

    [Category("Rotation"), Description("Smallest initial rotation, in degrees.")]
    public float RotationMin { get; set; }

    [Category("Rotation"), Description("Largest initial rotation, in degrees.")]
    public float RotationMax { get; set; }

    [Category("Rotation"), Description("Slowest spin, in degrees per second (negative = counter-clockwise).")]
    public float SpinMin { get; set; }

    [Category("Rotation"), Description("Fastest spin, in degrees per second.")]
    public float SpinMax { get; set; }

    [Category("Appearance"), Description("Image drawn for each particle. Leave empty to draw circles.")]
    [Editor(typeof(ImageFileNameEditor), typeof(UITypeEditor))]
    public string Texture { get; set; } = string.Empty;

    [Category("Appearance"), Description("Alpha: normal transparency. Additive: particles add light (fire, sparks, glow).")]
    public ParticleBlend Blend { get; set; } = ParticleBlend.Alpha;

    [Category("Appearance"), Description("Size when a particle is born, in pixels (for textures: the longer side).")]
    public float StartSize { get; set; } = 12f;

    [Category("Appearance"), Description("Size when a particle dies, in pixels.")]
    public float EndSize { get; set; } = 2f;

    [Category("Appearance"), Description("Color when a particle is born (multiplies the texture).")]
    public DrawingColor StartColor { get; set; } = DrawingColor.FromArgb(255, 191, 51);

    [Category("Appearance"), Description("Opacity when a particle is born (0-255).")]
    public int StartAlpha { get; set; } = 255;

    [Category("Appearance"), Description("Color when a particle dies (multiplies the texture).")]
    public DrawingColor EndColor { get; set; } = DrawingColor.FromArgb(230, 26, 13);

    [Category("Appearance"), Description("Opacity when a particle dies (0-255).")]
    public int EndAlpha { get; set; } = 0;

    private float _duration = 1f;
    private float _prewarmSeconds = 1f;
    private int _fps = 30;
    private int _padding = 2;
    private int _framesPerRow;

    [Category("Timing"), Description("Length of the animation in seconds: how long the effect emits, and how long the emitter takes to cover its path.")]
    [RefreshProperties(RefreshProperties.All)]
    public float Duration { get => _duration; set => _duration = Math.Clamp(value, 0.05f, 600f); }

    [Category("Timing"), Description("Start \"full\": simulate the effect before the first frame. Turn off for effects that should start empty (bursts, explosions).")]
    public bool Prewarm { get; set; } = true;

    [Category("Timing"), Description("How long to simulate before the first frame, in seconds (when Prewarm is on).")]
    public float PrewarmSeconds { get => _prewarmSeconds; set => _prewarmSeconds = Math.Clamp(value, 0f, 60f); }

    [Category("Timing"), Description("After Duration, stop emitting and keep going until every particle has died, so the animation ends empty instead of cutting off. Adds frames.")]
    [RefreshProperties(RefreshProperties.All)]
    public bool DieOut { get; set; }

    [Category("Timing"), Description("Written to the .anim file: whether a game loops the animation. The live view always repeats.")]
    public bool Loop { get; set; } = true;

    [Category("Timing"), Description("Frames the recording will have (die-out adds frames until the last particle has died).")]
    [JsonIgnore]
    public string Frames => FrameCount + (DieOut ? " + die-out" : string.Empty);

    [Category("Sprite Sheet"), Description("Animation frames per second; the recording advances exactly 1/Fps per frame.")]
    [RefreshProperties(RefreshProperties.All)]
    public int Fps { get => _fps; set => _fps = Math.Clamp(value, 1, 240); }

    [Category("Sprite Sheet"), Description("Transparent pixels around the effect in each frame. The frame size itself is fitted automatically to every particle.")]
    public int Padding { get => _padding; set => _padding = Math.Clamp(value, 0, 256); }

    [Category("Sprite Sheet"), Description("Frames per sprite-sheet row; 0 chooses the most square sheet.")]
    public int FramesPerRow { get => _framesPerRow; set => _framesPerRow = Math.Clamp(value, 0, 10000); }

    /// <summary>Frames recorded while emitting (rounded like the native side); die-out adds more.</summary>
    [Browsable(false), JsonIgnore]
    public int FrameCount => Math.Max(1, (int)Math.Round(Duration * Fps, MidpointRounding.AwayFromZero));

    /// <summary>The path the emitter follows (edited with the pen tool, not in the grid). Empty = it stays at the origin.</summary>
    [Browsable(false)]
    public EmitterPath Path { get; set; } = new();

    public ParticleSettings Clone()
    {
        var clone = (ParticleSettings)MemberwiseClone();
        clone.Path = Path.Clone();
        return clone;
    }

    /// <summary>Every setting as a "set" message, e.g. to initialise the native preview.</summary>
    public IEnumerable<string> ToMessages() =>
        TypeDescriptor.GetProperties(this)
            .Cast<PropertyDescriptor>()
            .Select(property => ToMessage(property.Name))
            .OfType<string>()
            .Append($"set Path {Path.ToMessageValue()}")
            .Distinct();

    /// <summary>The "set" message for a changed property, or null if it has no native counterpart.</summary>
    public string? ToMessage(string propertyName) => propertyName switch
    {
        nameof(StartColor) or nameof(StartAlpha) => $"set StartColor {FormatColor(StartColor, StartAlpha)}",
        nameof(EndColor) or nameof(EndAlpha) => $"set EndColor {FormatColor(EndColor, EndAlpha)}",
        nameof(MaxParticles) => $"set MaxParticles {MaxParticles}",
        nameof(Texture) => $"set Texture {Texture}",
        nameof(Blend) => $"set Blend {Blend}",
        nameof(Path) => $"set Path {Path.ToMessageValue()}",
        nameof(Prewarm) => $"set Prewarm {(Prewarm ? 1 : 0)}",
        nameof(DieOut) => $"set DieOut {(DieOut ? 1 : 0)}",
        _ => FormatFloatSetting(propertyName),
    };

    /// <summary>The key=value options of a "measure"/"record" message (see Recorder.h in the native project).</summary>
    public string ToRecordingOptions(uint seed) => string.Create(CultureInfo.InvariantCulture,
        $"seed={seed} fps={Fps} duration={Duration} prewarm={(Prewarm ? PrewarmSeconds : 0f)} padding={Padding} columns={FramesPerRow} loop={(Loop ? 1 : 0)} dieout={(DieOut ? 1 : 0)}");

    private string? FormatFloatSetting(string propertyName)
    {
        var property = GetType().GetProperty(propertyName);
        return property?.GetValue(this) is float value
            ? $"set {propertyName} {value.ToString(CultureInfo.InvariantCulture)}"
            : null;
    }

    private static string FormatColor(DrawingColor color, int alpha) =>
        $"{color.R},{color.G},{color.B},{Math.Clamp(alpha, 0, 255)}";
}
