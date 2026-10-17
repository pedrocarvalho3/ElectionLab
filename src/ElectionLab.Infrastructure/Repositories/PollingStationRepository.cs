using Dapper;
using ElectionLab.Application.Abstractions;
using ElectionLab.Domain;
using Npgsql;

namespace ElectionLab.Infrastructure.Repositories;

public class PollingStationRepository(NpgsqlDataSource dataSource) : IPollingStationRepository
{
    public async Task AddAsync(PollingStation pollingStation)
    {
        const string sql = """
            INSERT INTO polling_stations ("Id", "ElectionId", "Code", "Status", "OpenedAt", "ClosedAt")
            VALUES (@Id, @ElectionId, @Code, @Status, @OpenedAt, @ClosedAt);
            """;

        await using var connection = await dataSource.OpenConnectionAsync();
        await connection.ExecuteAsync(sql, new
        {
            pollingStation.Id,
            pollingStation.ElectionId,
            pollingStation.Code,
            Status = (int)pollingStation.Status,
            pollingStation.OpenedAt,
            pollingStation.ClosedAt
        });
    }
}
