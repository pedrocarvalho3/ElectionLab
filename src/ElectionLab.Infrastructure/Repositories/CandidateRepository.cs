using ElectionLab.Application.Abstractions;
using ElectionLab.Domain;
using ElectionLab.Infrastructure.Persistence;

namespace ElectionLab.Infrastructure.Repositories;

public class CandidateRepository(AppDbContext db) : ICandidateRepository
{
    public async Task AddAsync(Candidate candidate)
    {
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();
    }
}