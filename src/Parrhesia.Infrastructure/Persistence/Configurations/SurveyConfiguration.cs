using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Infrastructure.Persistence.Configurations;

public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> builder)
    {
        builder.ToTable("Surveys");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Ignore(s => s.SurveyId);

        builder.Property(s => s.Title)
            .HasConversion(
                v => v.Value,
                v => SurveyTitle.Create(v))
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(2000);

        builder.Property(s => s.Status)
            .HasConversion(
                v => v.Value,
                v => SurveyStatus.FromString(v))
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(s => s.CollectionPeriod, cp =>
        {
            cp.Property(p => p.StartDate)
                .HasColumnName("StartDate")
                .IsRequired();

            cp.Property(p => p.EndDate)
                .HasColumnName("EndDate")
                .IsRequired();
        });

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        builder.Ignore(s => s.Questions);
        builder.Ignore(s => s.Options);
        builder.Ignore(s => s.DomainEvents);

        builder.HasIndex(s => s.Status);
    }
}
