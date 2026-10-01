namespace Dojo;

public sealed class Challenge
{
    public required string Id { get; set; }
    public required int Order { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string[] Hints { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id) || Order <= 0 ||
            string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description) ||
            Hints is null || Hints.Length != 4 || Hints.Any(string.IsNullOrWhiteSpace))
            throw new InvalidDataException("Challenge requires an ID, positive order, title, description, and four nonempty hints.");
    }
}
