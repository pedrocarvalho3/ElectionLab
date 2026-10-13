using ElectionLab.Domain;

namespace ElectionLab.Application.Abstractions;

public interface ICandidateRepository
{
    Task AddAsync(Candidate candidate);
}