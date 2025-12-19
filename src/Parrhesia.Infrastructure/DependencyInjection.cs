using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Parrhesia.Domain.SurveyManagement.Repositories;
using Parrhesia.Domain.Voting.Repositories;
using Parrhesia.Domain.Voting.Services;
using Parrhesia.Infrastructure.Persistence;
using Parrhesia.Infrastructure.Persistence.Repositories;
using Parrhesia.Infrastructure.Persistence.Services;

namespace Parrhesia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        string connectionString,
        string fingerprintSalt)
    {
        // DbContext updated to use SQL Server
        services.AddDbContext<ParrhesiaDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<ISurveyRepository, SurveyRepository>();
        services.AddScoped<IBallotRepository, BallotRepository>();

        // Domain Services
        services.AddSingleton<IFingerprintGenerator>(_ => 
            new FingerprintGenerator(fingerprintSalt));
        services.AddScoped<ISurveyQueryService, SurveyQueryService>();
        services.AddScoped<IVotingService, VotingService>();

        return services;
    }
}