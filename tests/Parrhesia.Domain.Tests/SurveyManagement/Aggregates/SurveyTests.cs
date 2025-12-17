using FluentAssertions;
using Parrhesia.Domain.Common.Exceptions;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.Events;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.Tests.SurveyManagement.Aggregates;

public class SurveyTests
{
    private static Survey CreateValidSurvey()
    {
        return Survey.Create(
            "Eleições 2026",
            "Pesquisa de intenção de voto",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(15));
    }

    private static Survey CreateSurveyReadyForActivation()
    {
        var survey = CreateValidSurvey();
        survey.AddQuestion("Qual candidato você prefere?", 0);
        var questionId = survey.Questions[0].QuestionId;
        survey.AddOption(questionId, "Candidato A", 0);
        survey.AddOption(questionId, "Candidato B", 1);
        return survey;
    }

    #region Create

    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        var survey = CreateValidSurvey();

        survey.SurveyId.Should().NotBeNull();
        survey.Title.Value.Should().Be("Eleições 2026");
        survey.Description.Should().Be("Pesquisa de intenção de voto");
        survey.Status.Should().Be(SurveyStatus.Draft);
        survey.Questions.Should().BeEmpty();
        survey.Options.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldRaiseSurveyCreatedEvent()
    {
        var survey = CreateValidSurvey();

        survey.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SurveyCreatedEvent>();
    }

    #endregion

    #region AddQuestion

    [Fact]
    public void AddQuestion_WhenDraft_ShouldAddQuestion()
    {
        var survey = CreateValidSurvey();

        survey.AddQuestion("Nova pergunta?", 0);

        survey.Questions.Should().ContainSingle();
        survey.Questions[0].Text.Value.Should().Be("Nova pergunta?");
    }

    [Fact]
    public void AddQuestion_WhenActive_ShouldThrow()
    {
        var survey = CreateSurveyReadyForActivation();
        survey.Activate();

        Action act = () => survey.AddQuestion("Outra pergunta?", 1);

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot modify survey after activation*");
    }

    #endregion

    #region AddOption

    [Fact]
    public void AddOption_WhenDraft_ShouldAddOption()
    {
        var survey = CreateValidSurvey();
        survey.AddQuestion("Pergunta?", 0);
        var questionId = survey.Questions[0].QuestionId;

        survey.AddOption(questionId, "Opção A", 0);

        survey.Options.Should().ContainSingle();
        survey.Options[0].Text.Value.Should().Be("Opção A");
    }

    [Fact]
    public void AddOption_WithInvalidQuestionId_ShouldThrow()
    {
        var survey = CreateValidSurvey();
        var invalidQuestionId = QuestionId.NewId();

        Action act = () => survey.AddOption(invalidQuestionId, "Opção", 0);

        act.Should().Throw<DomainException>()
            .WithMessage("*Question not found*");
    }

    [Fact]
    public void AddOption_WhenActive_ShouldThrow()
    {
        var survey = CreateSurveyReadyForActivation();
        survey.Activate();
        var questionId = survey.Questions[0].QuestionId;

        Action act = () => survey.AddOption(questionId, "Nova opção", 2);

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot modify survey after activation*");
    }

    #endregion

    #region RemoveQuestion

    [Fact]
    public void RemoveQuestion_ShouldRemoveQuestionAndItsOptions()
    {
        var survey = CreateSurveyReadyForActivation();
        var questionId = survey.Questions[0].QuestionId;

        survey.RemoveQuestion(questionId);

        survey.Questions.Should().BeEmpty();
        survey.Options.Should().BeEmpty();
    }

    [Fact]
    public void RemoveQuestion_WithInvalidId_ShouldDoNothing()
    {
        var survey = CreateSurveyReadyForActivation();
        var initialCount = survey.Questions.Count;

        survey.RemoveQuestion(QuestionId.NewId());

        survey.Questions.Should().HaveCount(initialCount);
    }

    #endregion

    #region Activate

    [Fact]
    public void Activate_WhenReadyAndDraft_ShouldSucceed()
    {
        var survey = CreateSurveyReadyForActivation();

        survey.Activate();

        survey.Status.Should().Be(SurveyStatus.Active);
    }

    [Fact]
    public void Activate_ShouldRaiseSurveyActivatedEvent()
    {
        var survey = CreateSurveyReadyForActivation();
        survey.ClearDomainEvents();

        survey.Activate();

        survey.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SurveyActivatedEvent>();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrow()
    {
        var survey = CreateSurveyReadyForActivation();
        survey.Activate();

        Action act = () => survey.Activate();

        act.Should().Throw<DomainException>()
            .WithMessage("*already active*");
    }

    [Fact]
    public void Activate_WhenClosed_ShouldThrow()
    {
        var survey = CreateSurveyReadyForActivation();
        survey.Activate();
        survey.Close();

        Action act = () => survey.Activate();

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot activate a closed survey*");
    }

    [Fact]
    public void Activate_WithoutQuestions_ShouldThrow()
    {
        var survey = CreateValidSurvey();

        Action act = () => survey.Activate();

        act.Should().Throw<DomainException>()
            .WithMessage("*must have at least one question*");
    }

    [Fact]
    public void Activate_WithLessThanTwoOptions_ShouldThrow()
    {
        var survey = CreateValidSurvey();
        survey.AddQuestion("Pergunta?", 0);
        survey.AddOption(survey.Questions[0].QuestionId, "Única opção", 0);

        Action act = () => survey.Activate();

        act.Should().Throw<DomainException>()
            .WithMessage("*at least 2 options*");
    }

    [Fact]
    public void Activate_WithMoreThanTenOptions_ShouldThrow()
    {
        var survey = CreateValidSurvey();
        survey.AddQuestion("Pergunta?", 0);
        var questionId = survey.Questions[0].QuestionId;

        for (int i = 0; i < 11; i++)
            survey.AddOption(questionId, $"Opção {i}", i);

        Action act = () => survey.Activate();

        act.Should().Throw<DomainException>()
            .WithMessage("*cannot have more than 10 options*");
    }

    #endregion

    #region Close

    [Fact]
    public void Close_WhenActive_ShouldSucceed()
    {
        var survey = CreateSurveyReadyForActivation();
        survey.Activate();

        survey.Close("Período encerrado");

        survey.Status.Should().Be(SurveyStatus.Closed);
    }

    [Fact]
    public void Close_ShouldRaiseSurveyClosedEvent()
    {
        var survey = CreateSurveyReadyForActivation();
        survey.Activate();
        survey.ClearDomainEvents();

        survey.Close("Teste");

        survey.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SurveyClosedEvent>();
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldThrow()
    {
        var survey = CreateSurveyReadyForActivation();
        survey.Activate();
        survey.Close();

        Action act = () => survey.Close();

        act.Should().Throw<DomainException>()
            .WithMessage("*already closed*");
    }

    #endregion

    #region IsAcceptingVotes

    [Fact]
    public void IsAcceptingVotes_WhenActiveAndWithinPeriod_ShouldReturnTrue()
    {
        var survey = Survey.Create(
            "Test",
            "Desc",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(15));
        survey.AddQuestion("Q?", 0);
        var qId = survey.Questions[0].QuestionId;
        survey.AddOption(qId, "A", 0);
        survey.AddOption(qId, "B", 1);
        survey.Activate();

        survey.IsAcceptingVotes().Should().BeTrue();
    }

    [Fact]
    public void IsAcceptingVotes_WhenDraft_ShouldReturnFalse()
    {
        var survey = CreateValidSurvey();

        survey.IsAcceptingVotes().Should().BeFalse();
    }

    [Fact]
    public void IsAcceptingVotes_WhenClosed_ShouldReturnFalse()
    {
        var survey = CreateSurveyReadyForActivation();
        survey.Activate();
        survey.Close();

        survey.IsAcceptingVotes().Should().BeFalse();
    }

    #endregion

    #region HasOption

    [Fact]
    public void HasOption_WithValidIds_ShouldReturnTrue()
    {
        var survey = CreateSurveyReadyForActivation();
        var questionId = survey.Questions[0].QuestionId;
        var optionId = survey.Options[0].OptionId;

        survey.HasOption(questionId, optionId).Should().BeTrue();
    }

    [Fact]
    public void HasOption_WithInvalidOptionId_ShouldReturnFalse()
    {
        var survey = CreateSurveyReadyForActivation();
        var questionId = survey.Questions[0].QuestionId;

        survey.HasOption(questionId, OptionId.NewId()).Should().BeFalse();
    }

    #endregion

    #region Update Methods

    [Fact]
    public void UpdateTitle_WhenDraft_ShouldSucceed()
    {
        var survey = CreateValidSurvey();

        survey.UpdateTitle("Novo título");

        survey.Title.Value.Should().Be("Novo título");
    }

    [Fact]
    public void UpdateTitle_WhenActive_ShouldThrow()
    {
        var survey = CreateSurveyReadyForActivation();
        survey.Activate();

        Action act = () => survey.UpdateTitle("Novo");

        act.Should().Throw<DomainException>();
    }

    #endregion
}
