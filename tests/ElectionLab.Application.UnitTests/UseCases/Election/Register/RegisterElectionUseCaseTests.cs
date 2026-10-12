using ElectionLab.Application.Abstractions;
using ElectionLab.Application.UseCases.Election.Register;
using ElectionLab.Domain.Enums;
using Xunit;

namespace ElectionLab.Application.UnitTests.UseCases.Election.Register;

public class RegisterElectionUseCaseTests
{
    [Fact]
    public async Task Execute_ShouldAddElectionWithRequestedName()
    {
        var repository = new InMemoryElectionRepository();
        var useCase = new RegisterElectionUseCase(repository);
        var request = new RegisterElectionRequest("Student Council 2026");

        await useCase.Execute(request);

        var election = Assert.Single(repository.AddedElections);
        Assert.Equal(request.Name, election.Name);
    }

    [Fact]
    public async Task Execute_ShouldCreateElectionAsDraftWithGeneratedId()
    {
        var repository = new InMemoryElectionRepository();
        var useCase = new RegisterElectionUseCase(repository);
        var request = new RegisterElectionRequest("Neighborhood Board");

        await useCase.Execute(request);

        var election = Assert.Single(repository.AddedElections);
        Assert.NotEqual(Guid.Empty, election.Id);
        Assert.Equal(ElectionStatus.Draft, election.Status);
    }

    private sealed class InMemoryElectionRepository : IElectionRepository
    {
        private readonly List<ElectionLab.Domain.Election> _addedElections = [];

        public IReadOnlyCollection<ElectionLab.Domain.Election> AddedElections => _addedElections;

        public Task AddAsync(ElectionLab.Domain.Election election)
        {
            _addedElections.Add(election);
            return Task.CompletedTask;
        }
    }
}
