using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.Entities;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.Tests.SurveyManagement.Entities;

public class QuestionTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        var text = QuestionText.Create("Qual candidato você prefere?");

        var question = Question.Create(text, 0);

        question.QuestionId.Should().NotBeNull();
        question.Text.Should().Be(text);
        question.Order.Should().Be(0);
    }

    [Fact]
    public void Create_WithNegativeOrder_ShouldThrow()
    {
        var text = QuestionText.Create("Test?");

        Action act = () => Question.Create(text, -1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Order must be non-negative*");
    }

    [Fact]
    public void Create_WithExistingId_ShouldUseProvidedId()
    {
        var id = QuestionId.NewId();
        var text = QuestionText.Create("Test?");

        var question = Question.Create(id, text, 0);

        question.QuestionId.Should().Be(id);
    }

    [Fact]
    public void UpdateText_ShouldChangeText()
    {
        var question = Question.Create(QuestionText.Create("Old"), 0);
        var newText = QuestionText.Create("New");

        question.UpdateText(newText);

        question.Text.Should().Be(newText);
    }

    [Fact]
    public void UpdateOrder_WithValidOrder_ShouldChangeOrder()
    {
        var question = Question.Create(QuestionText.Create("Test"), 0);

        question.UpdateOrder(5);

        question.Order.Should().Be(5);
    }

    [Fact]
    public void UpdateOrder_WithNegativeOrder_ShouldThrow()
    {
        var question = Question.Create(QuestionText.Create("Test"), 0);

        Action act = () => question.UpdateOrder(-1);

        act.Should().Throw<ArgumentException>();
    }
}
