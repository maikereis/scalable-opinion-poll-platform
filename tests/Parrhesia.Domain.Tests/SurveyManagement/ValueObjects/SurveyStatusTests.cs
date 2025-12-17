using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.Tests.SurveyManagement.ValueObjects;

public class SurveyStatusTests
{
    [Fact]
    public void Draft_ShouldHaveCorrectValue()
    {
        SurveyStatus.Draft.Value.Should().Be("Draft");
        SurveyStatus.Draft.IsDraft.Should().BeTrue();
        SurveyStatus.Draft.IsActive.Should().BeFalse();
        SurveyStatus.Draft.IsClosed.Should().BeFalse();
    }

    [Fact]
    public void Active_ShouldHaveCorrectValue()
    {
        SurveyStatus.Active.Value.Should().Be("Active");
        SurveyStatus.Active.IsDraft.Should().BeFalse();
        SurveyStatus.Active.IsActive.Should().BeTrue();
        SurveyStatus.Active.IsClosed.Should().BeFalse();
    }

    [Fact]
    public void Closed_ShouldHaveCorrectValue()
    {
        SurveyStatus.Closed.Value.Should().Be("Closed");
        SurveyStatus.Closed.IsDraft.Should().BeFalse();
        SurveyStatus.Closed.IsActive.Should().BeFalse();
        SurveyStatus.Closed.IsClosed.Should().BeTrue();
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Active")]
    [InlineData("Closed")]
    public void FromString_WithValidValue_ShouldReturnCorrectStatus(string value)
    {
        var status = SurveyStatus.FromString(value);

        status.Value.Should().Be(value);
    }

    [Fact]
    public void FromString_WithInvalidValue_ShouldThrow()
    {
        Action act = () => SurveyStatus.FromString("Invalid");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid survey status*");
    }

    [Fact]
    public void SameStatuses_ShouldBeEqual()
    {
        var status1 = SurveyStatus.Draft;
        var status2 = SurveyStatus.Draft;

        status1.Should().Be(status2);
        (status1 == status2).Should().BeTrue();
    }

    [Fact]
    public void DifferentStatuses_ShouldNotBeEqual()
    {
        SurveyStatus.Draft.Should().NotBe(SurveyStatus.Active);
        SurveyStatus.Active.Should().NotBe(SurveyStatus.Closed);
    }
}
