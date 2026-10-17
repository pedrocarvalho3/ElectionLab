using ElectionLab.Application.Abstractions;
using ElectionLab.Application.UseCases.Candidate.Register;
using ElectionLab.Application.UseCases.Election.Register;
using ElectionLab.Application.UseCases.PollingStation.Register;
using ElectionLab.Infrastructure.Persistence;
using ElectionLab.Infrastructure.Repositories;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(
    builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("Connection string 'Database' is required.")));
builder.Services.AddTransient<DatabaseMigrator>();

builder.Services.AddScoped<IRegisterElectionUseCase, RegisterElectionUseCase>();
builder.Services.AddScoped<IElectionRepository, ElectionRepository>();
builder.Services.AddScoped<IRegisterCandidateUseCase, RegisterCandidateUseCase>();
builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
builder.Services.AddScoped<IRegisterPollingStationUseCase, RegisterPollingStationUseCase>();
builder.Services.AddScoped<IPollingStationRepository, PollingStationRepository>();

var app = builder.Build();

if (args.Contains("--migrate", StringComparer.Ordinal))
{
    await app.Services.GetRequiredService<DatabaseMigrator>().MigrateAsync();
    await app.DisposeAsync();
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
