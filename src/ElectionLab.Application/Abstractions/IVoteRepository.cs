using ElectionLab.Domain;

namespace ElectionLab.Application.Abstractions;

public interface IVoteRepository
{
    Task AddAsync(Vote vote);
}