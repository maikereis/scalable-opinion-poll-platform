using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Aggregates;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Infrastructure.Persistence.Configurations;

public class BallotConfiguration : IEntityTypeConfiguration<Ballot>
{
    public void Configure(EntityTypeBuilder<Ballot> builder)
    {
        builder.ToTable("Ballots");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Ignore(b => b.BallotId);

        builder.Property(b => b.SurveyId)
            .HasConversion(
                v => v.Value,
                v => SurveyId.Reconstitute(v))
            .IsRequired();

        builder.Property(b => b.QuestionId)
            .HasConversion(
                v => v.Value,
                v => QuestionId.Reconstitute(v))
            .IsRequired();

        builder.Property(b => b.SelectedOptionId)
            .HasConversion(
                v => v.Value,
                v => OptionId.Reconstitute(v))
            .IsRequired();

        builder.Property(b => b.VoterFingerprint)
            .HasConversion(
                v => v.Value,
                v => VoterFingerprint.Reconstitute(v))
            .HasMaxLength(64)
            .IsRequired();

        builder.OwnsOne(b => b.DeviceInfo, di =>
        {
            di.Property(d => d.DeviceId)
                .HasColumnName("DeviceId")
                .HasMaxLength(255);

            di.Property(d => d.UserAgent)
                .HasColumnName("UserAgent")
                .HasMaxLength(500);

            di.Property(d => d.IpHash)
                .HasColumnName("IpHash")
                .HasMaxLength(64);
        });

        builder.Property(b => b.CastedAt)
            .IsRequired();

        builder.Ignore(b => b.DomainEvents);

        builder.HasIndex(b => new { b.VoterFingerprint, b.SurveyId })
            .IsUnique();

        builder.HasIndex(b => b.SurveyId);
        builder.HasIndex(b => b.SelectedOptionId);
        builder.HasIndex(b => b.CastedAt);
    }
}