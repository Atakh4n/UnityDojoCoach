using System.Diagnostics;
using System.Text.Json;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace Dojo.Tests;

public sealed class DojoWorkspace : IDisposable
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "dojo-test-" + Guid.NewGuid().ToString("N"));
    public string Curriculum => Path.Combine(Root, "curriculum", "csharp");
    public string ProgressPath => Path.Combine(Root, ".dojo", "progress.json");
    public ProgressStore Store => new(ProgressPath);

    public DojoWorkspace(int challengeCount = 2)
    {
        Directory.CreateDirectory(Curriculum);
        for (var i = 1; i <= challengeCount; i++) AddChallenge(i);
    }

    public void AddChallenge(int order)
    {
        var challenge = new Challenge
        {
            Id = $"csharp-{order:000}", Order = order, Title = $"Exercise {order}",
            Description = "Investigate a small C# behavior and explain your observations.",
            Hints = ["What do you expect?", "Try a smaller input.",
                "Compare the observation with the relevant language rule.", "Outline: predict, observe, compare."]
        };
        File.WriteAllText(Path.Combine(Curriculum, $"{order:000}.json"),
            JsonSerializer.Serialize(challenge, ProgressStore.JsonOptions));
    }

    public (int ExitCode, string Output, string Error) Run(params string[] args)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = Root, RedirectStandardOutput = true, RedirectStandardError = true,
            UseShellExecute = false,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8
        };
        start.ArgumentList.Add(typeof(Progress).Assembly.Location);
        foreach (var arg in args) start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(30000))
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException("CLI did not exit within 30 seconds.");
        }
        return (process.ExitCode, output.GetAwaiter().GetResult(), error.GetAwaiter().GetResult());
    }

    public void Dispose()
    {
        // Only this fixture's absolute temporary directory is ever removed.
        Directory.Delete(Root, recursive: true);
    }
}
