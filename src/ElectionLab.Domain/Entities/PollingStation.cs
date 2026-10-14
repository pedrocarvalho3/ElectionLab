using ElectionLab.Domain.Enums;

namespace ElectionLab.Domain;

public class PollingStation
{
    public Guid Id { get; private set; }
    public Guid ElectionId { get; private set; }

    public string Code { get; private set; }

    public PollingStationStatus Status { get; private set; }

    public DateTime? OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    private PollingStation() { }

    public PollingStation(Guid electionId, string code)
    {
        if (electionId == Guid.Empty)
            throw new ArgumentException(
                "Election id is required.",
                nameof(electionId));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                "Polling station code is required.",
                nameof(code));

        Id = Guid.NewGuid();
        ElectionId = electionId;
        Code = code;
        Status = PollingStationStatus.Draft;
    }

    public void Open()
    {
        if (Status != PollingStationStatus.Draft)
            throw new InvalidOperationException(
                "Only draft polling stations can be opened.");

        Status = PollingStationStatus.Open;
        OpenedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        if (Status != PollingStationStatus.Open)
            throw new InvalidOperationException(
                "Only open polling stations can be closed.");

        Status = PollingStationStatus.Closed;
        ClosedAt = DateTime.UtcNow;
    }
}