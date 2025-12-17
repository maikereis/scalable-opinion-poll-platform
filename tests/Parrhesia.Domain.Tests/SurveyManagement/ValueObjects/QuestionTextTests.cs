using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.Tests.SurveyManagement.ValueObjects;

public class QuestionTextTests
{
    [Fact]
    public void Create_WithValidText_ShouldSucceed()
    {
        var text = QuestionText.Create("Qual candidato você prefere?");

        text.Value.Should().Be("Qual candidato você prefere?");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        var text = QuestionText.Create("  Question  ");

        text.Value.Should().Be("Question");
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrow()
    {
        Action act = () => QuestionText.Create("");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Create_ExceedingMaxLength_ShouldThrow()
    {
        var longText = new string('a', QuestionText.MaxLength + 1);

        Action act = () => QuestionText.Create(longText);

        act.Should().Throw<ArgumentException>()
            .WithMessage($"*cannot exceed {QuestionText.MaxLength} characters*");
    }

    [Fact]
    public void TwoTexts_WithSameValue_ShouldBeEqual()
    {
        var text1 = QuestionText.Create("Test?");
        var text2 = QuestionText.Create("Test?");

        text1.Should().Be(text2);
    }
}
