using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using DrawingColor = System.Drawing.Color;

namespace ParticleRecorder.Effects;

/// <summary>
/// An effect file (*.keparticle): JSON holding the particle and recording settings.
/// The texture path is stored relative to the effect file, so an effect can be moved
/// together with its textures.
/// </summary>
public static class EffectDocument
{
    public const string Extension = ".keparticle";
    public const int CurrentVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(), new HexColorConverter() },
    };

    public static void Save(string path, ParticleSettings particles, RecordingSettings recording)
    {
        var stored = particles.Clone();
        stored.Texture = ToRelativePath(path, particles.Texture);

        var file = new EffectFile { Version = CurrentVersion, Particles = stored, Recording = recording };
        File.WriteAllText(path, JsonSerializer.Serialize(file, JsonOptions));
    }

    /// <summary>Loads an effect file. Throws IOException, JsonException or InvalidDataException.</summary>
    public static (ParticleSettings Particles, RecordingSettings Recording) Load(string path)
    {
        var file = JsonSerializer.Deserialize<EffectFile>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException("The file is empty.");
        if (file.Version is < 1 or > CurrentVersion)
            throw new InvalidDataException($"Unsupported effect file version {file.Version}.");

        var particles = file.Particles ?? new ParticleSettings();
        particles.Texture = ToAbsolutePath(path, particles.Texture);
        return (particles, file.Recording ?? new RecordingSettings());
    }

    private static string ToRelativePath(string effectPath, string texture)
    {
        if (string.IsNullOrEmpty(texture)) return string.Empty;

        var directory = Path.GetDirectoryName(Path.GetFullPath(effectPath))!;
        var relative = Path.GetRelativePath(directory, texture);
        return Path.IsPathRooted(relative) ? texture : relative;   // stays absolute across drives
    }

    private static string ToAbsolutePath(string effectPath, string texture)
    {
        if (string.IsNullOrEmpty(texture) || Path.IsPathRooted(texture)) return texture ?? string.Empty;

        var directory = Path.GetDirectoryName(Path.GetFullPath(effectPath))!;
        return Path.GetFullPath(Path.Combine(directory, texture));
    }

    private sealed class EffectFile
    {
        public int Version { get; set; }
        public ParticleSettings? Particles { get; set; }
        public RecordingSettings? Recording { get; set; }
    }

    /// <summary>Colors as "#RRGGBB" (alpha is a separate setting).</summary>
    private sealed class HexColorConverter : JsonConverter<DrawingColor>
    {
        public override DrawingColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var text = reader.GetString();
            if (text is not { Length: 7 } || text[0] != '#'
                || !int.TryParse(text.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
                throw new JsonException($"Invalid color \"{text}\"; expected #RRGGBB.");

            return DrawingColor.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
        }

        public override void Write(Utf8JsonWriter writer, DrawingColor value, JsonSerializerOptions options) =>
            writer.WriteStringValue($"#{value.R:X2}{value.G:X2}{value.B:X2}");
    }
}
