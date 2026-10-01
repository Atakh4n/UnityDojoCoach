namespace Dojo;

public sealed class DojoApp(List<Challenge> challenges, ProgressStore store)
{
    public string Status()
    {
        var progress = store.Load();
        var active = challenges.Find(c => c.Id == progress.ActiveChallengeId);
        var current = progress.ActiveChallengeId is null ? "none" :
            active is null ? $"{progress.ActiveChallengeId} (unavailable)" : $"{active.Id} — {active.Title}";
        var lines = new List<string>
        {
            $"Current challenge: {current}",
            $"Completed challenges: {progress.CompletedChallengeIds.Count}",
            $"Hints used: {progress.HintCounts.Values.Sum(count => (long)count)}"
        };
        var referencedIds = progress.CompletedChallengeIds.Concat(progress.HintCounts.Keys);
        if (progress.ActiveChallengeId is not null) referencedIds = referencedIds.Append(progress.ActiveChallengeId);
        var unavailable = referencedIds.Distinct(StringComparer.Ordinal)
            .Where(id => !challenges.Any(c => c.Id == id)).Order(StringComparer.Ordinal).ToList();
        if (unavailable.Count > 0) lines.Add($"Unavailable challenges: {string.Join(", ", unavailable)}");
        return string.Join(Environment.NewLine, lines);
    }

    public string Next()
    {
        var progress = store.Load();
        Challenge? next;
        if (progress.ActiveChallengeId is null)
        {
            next = challenges.FirstOrDefault(c => !progress.CompletedChallengeIds.Contains(c.Id));
            if (next is null)
                return challenges.Count == 0 ? "No challenges available." : "Curriculum complete.";
        }
        else
        {
            var active = RequireActive(progress.ActiveChallengeId);
            next = challenges.FirstOrDefault(c => c.Order > active.Order && !progress.CompletedChallengeIds.Contains(c.Id));
            progress.CompletedChallengeIds.Add(active.Id);
        }
        progress.ActiveChallengeId = next?.Id;
        store.Save(progress);
        return next is null ? "Curriculum complete." : $"Challenge: {next.Id} — {next.Title}{Environment.NewLine}{next.Description}";
    }

    private Challenge RequireActive(string id) => challenges.Find(c => c.Id == id)
        ?? throw new InvalidDataException($"Challenge {id} is unavailable.");

    public string Hint()
    {
        var progress = store.Load();
        if (progress.ActiveChallengeId is null) return "No active challenge. Run dojo next.";
        var active = RequireActive(progress.ActiveChallengeId);
        var count = progress.HintCounts.GetValueOrDefault(active.Id);
        if (count == 4) return "No more hints available.";
        progress.HintCounts[active.Id] = count + 1;
        store.Save(progress);
        return $"Hint {count + 1}/4: {active.Hints[count]}";
    }
}
