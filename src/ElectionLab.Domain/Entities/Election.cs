using ElectionLab.Domain.Enums;

namespace ElectionLab.Domain;

public class Election
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public ElectionStatus Status { get; private set; }
    
    private Election() { }

    public Election(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Election name is required.");
        
        Id = Guid.NewGuid();
        Name = name;
        Status = ElectionStatus.Draft;
    }

    public void Open()
    {
        if (Status != ElectionStatus.Draft)
            throw new InvalidOperationException(
                "Only draft elections can be opened.");

        Status = ElectionStatus.Open;
    }
}