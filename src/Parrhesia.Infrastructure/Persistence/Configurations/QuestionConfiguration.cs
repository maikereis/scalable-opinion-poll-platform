using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parrhesia.Domain.SurveyManagement.Entities;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .ValueGeneratedNever();

        builder.Ignore(q => q.QuestionId);

        builder.Property(q => q.Text)
            .HasConversion(
                v => v.Value,
                v => QuestionText.Create(v))
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(q => q.Order)
            .IsRequired();

        builder.Property<Guid>("SurveyId")
            .IsRequired();

        builder.HasIndex("SurveyId");
    }
}
