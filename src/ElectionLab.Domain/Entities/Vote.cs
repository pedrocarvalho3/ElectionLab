using ElectionLab.Domain.Enums;

namespace ElectionLab.Domain;

public class Vote
{
    public Guid Id { get; private set; }
    public Guid PollingStationId { get; private set; }
    public Guid ElectionId { get; private set; }
    public Guid? CandidateId { get; private set; }
    VoteType Type { get; set; }
    public DateTime CreatedAt { get; private set; }
    
    private Vote() { }

    public Vote(Guid electionId, Guid candidateId, VoteType type)
    {
        if (electionId == Guid.Empty)
            throw new ArgumentException(
                "Election id is required.",
                nameof(electionId));

        if (type == VoteType.Candidate)
            CandidateId = candidateId;
        
        Id = Guid.NewGuid();
        ElectionId = electionId;
        Type = type;
        CreatedAt = DateTime.UtcNow;
    }
}