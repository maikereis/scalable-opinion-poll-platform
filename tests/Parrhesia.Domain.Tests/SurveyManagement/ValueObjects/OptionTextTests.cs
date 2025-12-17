using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.Tests.SurveyManagement.ValueObjects;

public class OptionTextTests
{
    [Fact]
    public void Create_WithValidText_ShouldSucceed()
    {
        var text = OptionText.Create("Candidato A");

        text.Value.Should().Be("Candidato A");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        var text = OptionText.Create("  Option  ");

        text.Value.Should().Be("Option");
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrow()
    {
        Action act = () => OptionText.Create("");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Create_ExceedingMaxLength_ShouldThrow()
    {
        var longText = new string('a', OptionText.MaxLength + 1);

        Action act = () => OptionText.Create(longText);

        act.Should().Throw<ArgumentException>()
            .WithMessage($"*cannot exceed {OptionText.MaxLength} characters*");
    }

    [Fact]
    public void TwoTexts_WithSameValue_ShouldBeEqual()
    {
        var text1 = OptionText.Create("Test");
        var text2 = OptionText.Create("Test");

        text1.Should().Be(text2);
    }
}
