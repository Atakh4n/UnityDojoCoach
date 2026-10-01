using System.Text.Json;
using Dojo;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

if (args.Length != 1 || args[0] is not ("next" or "status" or "hint"))
{
    Console.Error.WriteLine("Usage: dojo <status|next|hint>");
    return 2;
}

try
{
    var challenges = ChallengeCatalog.Load(Path.Combine(Environment.CurrentDirectory, "curriculum", "csharp"));
    var store = new ProgressStore(Path.Combine(Environment.CurrentDirectory, ".dojo", "progress.json"));
    var app = new DojoApp(challenges, store);
    Console.WriteLine(args[0] switch
    {
        "next" => app.Next(),
        "status" => app.Status(),
        _ => app.Hint()
    });
    return 0;
}
catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException or JsonException)
{
    Console.Error.WriteLine($"Error: {error.Message}");
    return 1;
}
