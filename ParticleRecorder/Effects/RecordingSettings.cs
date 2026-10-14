using System.Globalization;
using System.Text.Json.Serialization;

namespace ParticleRecorder.Effects;

/// <summary>
/// How an effect is recorded into a sprite sheet. Sent to the native recorder as the
/// options of a "measure" or "record" message (see ParticleRecorder.Native/Source/Recording/Recorder.h).
/// The canvas size isn't a setting: the recorder fits it to every visible particle.
/// </summary>
public sealed class RecordingSettings
{
    /// <summary>Animation frames per second; the simulation advances exactly 1/Fps per frame.</summary>
    public int Fps { get; set; } = 30;

    /// <summary>Length of the animation, in seconds.</summary>
    public float Duration { get; set; } = 1f;

    /// <summary>Simulate the effect before the first frame, so it starts already "full".</summary>
    public bool PrewarmEnabled { get; set; } = true;

    /// <summary>How long to simulate before the first frame, in seconds.</summary>
    public float PrewarmSeconds { get; set; } = 1f;

    /// <summary>Written to the .anim file: whether the game loops the animation.</summary>
    public bool Loop { get; set; } = true;

    /// <summary>Transparent pixels around the effect in each frame.</summary>
    public int Padding { get; set; } = 2;

    /// <summary>Frames per sprite-sheet row; 0 chooses the most square sheet.</summary>
    public int Columns { get; set; }

    /// <summary>Number of frames recorded (rounded like the native side).</summary>
    [JsonIgnore]
    public int FrameCount => Math.Max(1, (int)Math.Round(Duration * Fps, MidpointRounding.AwayFromZero));

    public RecordingSettings Clone() => (RecordingSettings)MemberwiseClone();

    /// <summary>The key=value options for a "measure"/"record" message.</summary>
    public string ToOptions(uint seed) => string.Create(CultureInfo.InvariantCulture,
        $"seed={seed} fps={Fps} duration={Duration} prewarm={(PrewarmEnabled ? PrewarmSeconds : 0f)} padding={Padding} columns={Columns} loop={(Loop ? 1 : 0)}");
}
