using Microsoft.EntityFrameworkCore;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.Entities;
using Parrhesia.Domain.Voting.Aggregates;

namespace Parrhesia.Infrastructure.Persistence;

/// <summary>
/// Database context for the Parrhesia application.
/// All entity configurations are defined in separate IEntityTypeConfiguration classes.
/// </summary>
public class ParrhesiaDbContext : DbContext
{
    public ParrhesiaDbContext(DbContextOptions<ParrhesiaDbContext> options) : base(options)
    {
    }

    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<Ballot> Ballots => Set<Ballot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ParrhesiaDbContext).Assembly);
    }
}