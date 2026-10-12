namespace ElectionLab.Domain;

public class Candidate
{
    public Guid Id { get; private set; }
    public Guid ElectionId { get; private set; }
    public string Name { get; private set; }
    public int Number { get; private set; }

    private Candidate() { }

    public Candidate(Guid electionId, string name, int number)
    {
        if (electionId == Guid.Empty)
            throw new ArgumentException("Invalid election.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Candidate name is required.");

        if (number < 10 || number > 99)
            throw new ArgumentException("Candidate number must have 2 digits.");

        Id = Guid.NewGuid();
        ElectionId = electionId;
        Name = name;
        Number = number;
    }
}