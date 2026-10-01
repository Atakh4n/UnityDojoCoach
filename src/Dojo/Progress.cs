namespace Dojo;

public sealed class Progress
{
    public required string? ActiveChallengeId { get; set; }
    public required List<string> CompletedChallengeIds { get; set; }
    public required Dictionary<string, int> HintCounts { get; set; }

    public static Progress Fresh() => new()
    {
        ActiveChallengeId = null,
        CompletedChallengeIds = [],
        HintCounts = []
    };

    public void Validate()
    {
        if (ActiveChallengeId is not null && string.IsNullOrWhiteSpace(ActiveChallengeId) ||
            CompletedChallengeIds is null || HintCounts is null)
            throw new InvalidDataException("Invalid progress fields.");
        if (CompletedChallengeIds.Any(string.IsNullOrWhiteSpace) ||
            CompletedChallengeIds.Distinct(StringComparer.Ordinal).Count() != CompletedChallengeIds.Count ||
            ActiveChallengeId is not null && CompletedChallengeIds.Contains(ActiveChallengeId) ||
            HintCounts.Any(pair => string.IsNullOrWhiteSpace(pair.Key) || pair.Value < 0 || pair.Value > 4))
            throw new InvalidDataException("Invalid progress IDs or hint counts.");
    }
}
