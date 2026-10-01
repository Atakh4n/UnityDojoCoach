using System.Text.Json;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Dojo.Tests")]

namespace Dojo;

public sealed class ProgressStore
{
    private readonly string path;
    private readonly Action<string, string> writeTemporary;

    public ProgressStore(string path) : this(path, File.WriteAllText) { }

    // A narrow test seam simulates an interrupted write without OS permission changes.
    internal ProgressStore(string path, Action<string, string> writeTemporary)
    {
        this.path = path;
        this.writeTemporary = writeTemporary;
    }

    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = false
    };

    public Progress Load()
    {
        if (!File.Exists(path))
        {
            if (Directory.Exists(path)) throw new IOException("Progress path is a directory.");
            return Progress.Fresh();
        }
        var progress = JsonSerializer.Deserialize<Progress>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException("Progress cannot be null.");
        progress.Validate();
        return progress;
    }

    public void Save(Progress progress)
    {
        progress.Validate();
        var directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        Directory.CreateDirectory(directory);
        var temporary = Path.Combine(directory, $"progress-{Guid.NewGuid():N}.tmp");
        try
        {
            writeTemporary(temporary, JsonSerializer.Serialize(progress, JsonOptions));
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            try { File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
