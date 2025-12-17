using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.Tests.SurveyManagement.ValueObjects;

public class SurveyTitleTests
{
    [Fact]
    public void Create_WithValidTitle_ShouldSucceed()
    {
        var title = SurveyTitle.Create("Eleições 2026");

        title.Value.Should().Be("Eleições 2026");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        var title = SurveyTitle.Create("  Eleições 2026  ");

        title.Value.Should().Be("Eleições 2026");
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrow()
    {
        Action act = () => SurveyTitle.Create("");

        act.Should().Throw<ArgumentException>()
            .WithMessage("Survey title cannot be empty (Parameter 'value')");
    }

    [Fact]
    public void Create_WithWhitespaceOnly_ShouldThrow()
    {
        Action act = () => SurveyTitle.Create("   ");

        act.Should().Throw<ArgumentException>()
            .WithMessage("Survey title cannot be empty (Parameter 'value')");
    }

    [Fact]
    public void Create_WithNull_ShouldThrow()
    {
        Action act = () => SurveyTitle.Create(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ExceedingMaxLength_ShouldThrow()
    {
        var longTitle = new string('a', SurveyTitle.MaxLength + 1);

        Action act = () => SurveyTitle.Create(longTitle);

        act.Should().Throw<ArgumentException>()
            .WithMessage($"Survey title cannot exceed {SurveyTitle.MaxLength} characters (Parameter 'value')");
    }

    [Fact]
    public void Create_AtMaxLength_ShouldSucceed()
    {
        var maxTitle = new string('a', SurveyTitle.MaxLength);

        var title = SurveyTitle.Create(maxTitle);

        title.Value.Should().HaveLength(SurveyTitle.MaxLength);
    }

    [Fact]
    public void TwoTitles_WithSameValue_ShouldBeEqual()
    {
        var title1 = SurveyTitle.Create("Test");
        var title2 = SurveyTitle.Create("Test");

        title1.Should().Be(title2);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        var surveyTitle = SurveyTitle.Create("Test");

        string result = surveyTitle;

        result.Should().Be("Test");
    }
}
