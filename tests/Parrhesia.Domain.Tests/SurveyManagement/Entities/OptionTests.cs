using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.Entities;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.Tests.SurveyManagement.Entities;

public class OptionTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        var questionId = QuestionId.NewId();
        var text = OptionText.Create("Candidato A");

        var option = Option.Create(questionId, text, 0);

        option.OptionId.Should().NotBeNull();
        option.QuestionId.Should().Be(questionId);
        option.Text.Should().Be(text);
        option.Order.Should().Be(0);
    }

    [Fact]
    public void Create_WithNegativeOrder_ShouldThrow()
    {
        var questionId = QuestionId.NewId();
        var text = OptionText.Create("Test");

        Action act = () => Option.Create(questionId, text, -1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Order must be non-negative*");
    }

    [Fact]
    public void Create_WithExistingId_ShouldUseProvidedId()
    {
        var optionId = OptionId.NewId();
        var questionId = QuestionId.NewId();
        var text = OptionText.Create("Test");

        var option = Option.Create(optionId, questionId, text, 0);

        option.OptionId.Should().Be(optionId);
    }

    [Fact]
    public void UpdateText_ShouldChangeText()
    {
        var option = Option.Create(QuestionId.NewId(), OptionText.Create("Old"), 0);
        var newText = OptionText.Create("New");

        option.UpdateText(newText);

        option.Text.Should().Be(newText);
    }

    [Fact]
    public void UpdateOrder_WithValidOrder_ShouldChangeOrder()
    {
        var option = Option.Create(QuestionId.NewId(), OptionText.Create("Test"), 0);

        option.UpdateOrder(3);

        option.Order.Should().Be(3);
    }

    [Fact]
    public void UpdateOrder_WithNegativeOrder_ShouldThrow()
    {
        var option = Option.Create(QuestionId.NewId(), OptionText.Create("Test"), 0);

        Action act = () => option.UpdateOrder(-1);

        act.Should().Throw<ArgumentException>();
    }
}
