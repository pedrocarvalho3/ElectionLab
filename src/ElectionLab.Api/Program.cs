using ElectionLab.Application.Abstractions;
using ElectionLab.Application.UseCases.Candidate.Register;
using ElectionLab.Application.UseCases.Election.Register;
using ElectionLab.Infrastructure.Persistence;
using ElectionLab.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Database")
    )
);

builder.Services.AddScoped<IRegisterElectionUseCase, RegisterElectionUseCase>();
builder.Services.AddScoped<IElectionRepository, ElectionRepository>();
builder.Services.AddScoped<IRegisterCandidateUseCase, RegisterCandidateUseCase>();
builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();