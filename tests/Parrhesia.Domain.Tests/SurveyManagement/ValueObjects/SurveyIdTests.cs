using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.Tests.SurveyManagement.ValueObjects;

public class SurveyIdTests
{
    [Fact]
    public void NewId_ShouldGenerateUniqueId()
    {
        var id1 = SurveyId.NewId();
        var id2 = SurveyId.NewId();

        id1.Value.Should().NotBe(Guid.Empty);
        id2.Value.Should().NotBe(Guid.Empty);
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void Create_WithValidGuid_ShouldSucceed()
    {
        var guid = Guid.NewGuid();

        var id = SurveyId.Create(guid);

        id.Value.Should().Be(guid);
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldThrow()
    {
        Action act = () => SurveyId.Create(Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void TwoSurveyIds_WithSameValue_ShouldBeEqual()
    {
        var guid = Guid.NewGuid();
        var id1 = SurveyId.Create(guid);
        var id2 = SurveyId.Create(guid);

        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldWork()
    {
        var guid = Guid.NewGuid();
        var surveyId = SurveyId.Create(guid);

        Guid result = surveyId;

        result.Should().Be(guid);
    }
}
