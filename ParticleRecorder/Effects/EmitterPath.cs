using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;

namespace ParticleRecorder.Effects;

/// <summary>
/// The path the emitter follows during a recording, drawn with the pen tool: cubic Bézier
/// segments between points, open (start to end) or closed (a loop). Positions are in pixels
/// relative to the effect origin (where the emitter rests without a path). The emitter
/// covers the path once per recording Duration, at constant speed.
/// </summary>
public sealed class EmitterPath
{
    /// <summary>A closed path joins the last point back to the first and loops seamlessly.</summary>
    public bool Closed { get; set; }

    public List<PathPoint> Points { get; set; } = [];

    [JsonIgnore]
    public bool IsEmpty => Points.Count == 0;

    public EmitterPath Clone() => new() { Closed = Closed, Points = Points.Select(point => point.Clone()).ToList() };

    /// <summary>The value of the native "set Path" message (see EmitterPath::Parse in the native project).</summary>
    public string ToMessageValue() =>
        (Closed ? "1 " : "0 ") + string.Join(";", Points.Select(point => string.Create(CultureInfo.InvariantCulture,
            $"{point.X},{point.Y},{point.InX},{point.InY},{point.OutX},{point.OutY},{point.Rate}")));
}

/// <summary>A path point with its Bézier handles (offsets from the point) and emission rate.</summary>
public sealed class PathPoint
{
    public float X { get; set; }
    public float Y { get; set; }

    public float InX { get; set; }
    public float InY { get; set; }
    public float OutX { get; set; }
    public float OutY { get; set; }

    /// <summary>Smooth points keep their handles mirrored; corner points move them independently.</summary>
    public bool Smooth { get; set; }

    /// <summary>Emission rate here, as a percentage of EmissionRate. Blends linearly to the next point.</summary>
    public float Rate { get; set; } = 100f;

    [JsonIgnore]
    public Vector2 Position
    {
        get => new(X, Y);
        set => (X, Y) = (value.X, value.Y);
    }

    [JsonIgnore]
    public Vector2 In
    {
        get => new(InX, InY);
        set => (InX, InY) = (value.X, value.Y);
    }

    [JsonIgnore]
    public Vector2 Out
    {
        get => new(OutX, OutY);
        set => (OutX, OutY) = (value.X, value.Y);
    }

    public PathPoint Clone() => (PathPoint)MemberwiseClone();
}
