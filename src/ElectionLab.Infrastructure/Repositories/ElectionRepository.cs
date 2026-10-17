using Dapper;
using ElectionLab.Application.Abstractions;
using ElectionLab.Domain;
using Npgsql;

namespace ElectionLab.Infrastructure.Repositories;

public class ElectionRepository(NpgsqlDataSource dataSource) : IElectionRepository
{
    public async Task AddAsync(Election election)
    {
        const string sql = """
            INSERT INTO elections ("Id", "Name", "Status")
            VALUES (@Id, @Name, @Status);
            """;

        await using var connection = await dataSource.OpenConnectionAsync();
        await connection.ExecuteAsync(sql, new
        {
            election.Id,
            election.Name,
            Status = (int)election.Status
        });
    }
}
