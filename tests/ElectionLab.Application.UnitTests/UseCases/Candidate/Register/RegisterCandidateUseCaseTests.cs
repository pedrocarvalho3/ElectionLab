using ElectionLab.Application.Abstractions;
using ElectionLab.Application.UseCases.Candidate.Register;
using Xunit;

namespace ElectionLab.Application.UnitTests.UseCases.Candidate.Register;

public class RegisterCandidateUseCaseTests
{
    [Fact]
    public async Task Execute_ShouldAddCandidateWithRequestedElectionIdNameAndNumber()
    {
        var candidateRepository = new InMemoryCandidateRepository();
        var candidateUseCase = new RegisterCandidateUseCase(candidateRepository);
        var electionId = Guid.NewGuid();
        var candidateRequest = new RegisterCandidateRequest("John Doe", 99);

        await candidateUseCase.Execute(electionId, candidateRequest);

        var candidate = Assert.Single(candidateRepository.AddedCandidates);
        Assert.Equal(electionId, candidate.ElectionId);
        Assert.Equal(candidateRequest.Name, candidate.Name);
        Assert.Equal(candidateRequest.Number, candidate.Number);
    }
    
    private sealed class InMemoryCandidateRepository : ICandidateRepository
    {
        private readonly List<Domain.Candidate> _addedCandidates = [];
        
        public IReadOnlyCollection<Domain.Candidate> AddedCandidates => _addedCandidates;

        public Task AddAsync(Domain.Candidate candidate)
        {
           _addedCandidates.Add(candidate);
           return Task.CompletedTask;
        }

        public Task<Domain.Candidate?> GetByNumberAsync(Guid electionId, int number)
        {
            throw new NotImplementedException();
        }
    }
}