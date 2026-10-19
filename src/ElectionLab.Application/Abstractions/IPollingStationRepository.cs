using ElectionLab.Domain;

namespace ElectionLab.Application.Abstractions;

public interface IPollingStationRepository
{
    Task AddAsync(PollingStation pollingStation);
    Task<PollingStation?> GetByIdAsync(Guid pollingStationId);
}