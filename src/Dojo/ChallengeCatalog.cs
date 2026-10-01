using System.Text.Json;

namespace Dojo;

public static class ChallengeCatalog
{
    public static List<Challenge> Load(string directory)
    {
        if (!Directory.Exists(directory)) return [];
        var challenges = new List<Challenge>();
        foreach (var path in Directory.EnumerateFiles(directory, "*.json"))
        {
            var challenge = JsonSerializer.Deserialize<Challenge>(File.ReadAllText(path), ProgressStore.JsonOptions)
                ?? throw new InvalidDataException("Challenge cannot be null.");
            challenge.Validate();
            challenges.Add(challenge);
        }
        if (challenges.Select(c => c.Id).Distinct(StringComparer.Ordinal).Count() != challenges.Count ||
            challenges.Select(c => c.Order).Distinct().Count() != challenges.Count)
            throw new InvalidDataException("Challenge IDs and orders must be unique.");
        return challenges.OrderBy(c => c.Order).ToList();
    }
}
