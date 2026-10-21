using ElectionLab.Application.Abstractions;
using VoteEntity = ElectionLab.Domain.Vote;
using ElectionLab.Domain.Enums;

namespace ElectionLab.Application.UseCases.Vote.Register;

public class RegisterVoteUseCase(IVoteRepository voteRepository, IPollingStationRepository pollingStationRepository, ICandidateRepository candidateRepository)
    : IRegisterVoteUseCase
{
    public async Task Execute(Guid pollingStationId, int? candidateNumber, VoteType type)
    {
        var pollingStation =
            await pollingStationRepository.GetByIdAsync(pollingStationId);
        
        if (pollingStation is null)
            throw new ArgumentException("Polling station not found.");
        
        var electionId = pollingStation.ElectionId;
        
        Guid? candidateId = null;

        if (type == VoteType.Candidate)
        {
            if (candidateNumber is null)
                throw new ArgumentException("Candidate number is required.");
            
            var candidate = await candidateRepository
                .GetByNumberAsync(electionId, candidateNumber.Value);
            
            if (candidate is null)
                throw new Exception("Candidate not found.");

            candidateId = candidate.Id;
        }
        
        var vote = new VoteEntity(
            pollingStationId,
            electionId,
            candidateId,
            type);

        await voteRepository.AddAsync(vote);
    }
}