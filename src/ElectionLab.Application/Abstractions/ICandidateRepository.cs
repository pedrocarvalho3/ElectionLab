using ElectionLab.Domain;

namespace ElectionLab.Application.Abstractions;

public interface ICandidateRepository
{
    Task AddAsync(Candidate candidate);
    Task<Candidate?> GetByNumberAsync(Guid electionId, int number);
}