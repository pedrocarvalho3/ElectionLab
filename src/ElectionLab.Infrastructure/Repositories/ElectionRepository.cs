using ElectionLab.Application.Abstractions;
using ElectionLab.Domain;
using ElectionLab.Infrastructure.Persistence;

namespace ElectionLab.Infrastructure.Repositories;

public class ElectionRepository(AppDbContext db) : IElectionRepository
{
    public async Task AddAsync(Election election)
    {
        db.Elections.Add(election);
        await db.SaveChangesAsync();
    }
}