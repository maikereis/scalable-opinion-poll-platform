using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parrhesia.Domain.SurveyManagement.Entities;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Infrastructure.Persistence.Configurations;

public class OptionConfiguration : IEntityTypeConfiguration<Option>
{
    public void Configure(EntityTypeBuilder<Option> builder)
    {
        builder.ToTable("Options");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Ignore(o => o.OptionId);

        builder.Property(o => o.QuestionId)
            .HasConversion(
                v => v.Value,
                v => QuestionId.Reconstitute(v))
            .IsRequired();

        builder.Property(o => o.Text)
            .HasConversion(
                v => v.Value,
                v => OptionText.Reconstitute(v))
            .HasMaxLength(OptionText.MaxLength)
            .IsRequired();

        builder.Property(o => o.Order)
            .IsRequired();

        builder.Property<Guid>("SurveyId")
            .IsRequired();

        builder.HasIndex("SurveyId");
        builder.HasIndex(o => o.QuestionId);
    }
}