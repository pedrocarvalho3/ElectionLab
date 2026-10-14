using ElectionLab.Application.Abstractions;
using ElectionLab.Domain;
using ElectionLab.Infrastructure.Persistence;

namespace ElectionLab.Infrastructure.Repositories;

public class PollingStationRepository(AppDbContext db) : IPollingStationRepository
{
    public async Task AddAsync(PollingStation pollingStation)
    {
        db.PollingStations.Add(pollingStation);
        await db.SaveChangesAsync();
    }
}