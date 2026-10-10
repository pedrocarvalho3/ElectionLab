using ElectionLab.Domain;

namespace ElectionLab.Application.Abstractions;

public interface IElectionRepository
{
    Task AddAsync(Election election);
}