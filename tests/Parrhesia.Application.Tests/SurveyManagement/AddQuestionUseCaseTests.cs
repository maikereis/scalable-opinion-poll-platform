using FluentAssertions;
using NSubstitute;
using Parrhesia.Application.SurveyManagement.AddQuestion;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.Repositories;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Application.Tests.SurveyManagement;

public class AddQuestionUseCaseTests
{
    private readonly ISurveyRepository _surveyRespository;
    private readonly AddQuestionUseCase _sut;

    public AddQuestionUseCaseTests()
    {
        _surveyRespository = Substitute.For<ISurveyRepository>();
        _sut = new AddQuestionUseCase(_surveyRespository);
    }

    private static Survey CreateDraftSurvey()
    {
        return Survey.Create(
            "Test Survey",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(15)
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldAddQuestion()
    {
        var survey = CreateDraftSurvey();
        _surveyRespository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>()).Returns(survey);

        var request = new AddQuestionRequest(
            survey.SurveyId.Value,
            "Qual candidato você prefere?",
            0);

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Text.Should().Be("Qual candidato você prefere?");
        result.Value.Order.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPersistChanges()
    {
        var survey = CreateDraftSurvey();
        _surveyRespository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>()).Returns(survey);

        var request = new AddQuestionRequest(survey.SurveyId.Value, "Question?", 0);

        await _sut.ExecuteAsync(request);

        await _surveyRespository.Received(1).UpdateAsync(Arg.Any<Survey>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenSurveyNotFound_ShouldFail()
    {
        _surveyRespository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>()).Returns((Survey?)null);

        var request = new AddQuestionRequest(Guid.NewGuid(), "Question?", 0);

        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task ExecuteAsync_WhenSurveyIsActive_ShouldFail()
    {
        var survey = CreateDraftSurvey();
        survey.AddQuestion("Existing question?", 0);
        var questionId = survey.Questions[0].QuestionId;
        survey.AddOption(questionId, "Option A", 0);
        survey.AddOption(questionId, "Option B", 1);
        survey.Activate();

        _surveyRespository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>()).Returns(survey);

        var request = new AddQuestionRequest(survey.SurveyId.Value, "New Question?", 1);

        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot modify");
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyText_ShouldFail()
    {
        var survey = CreateDraftSurvey();
        _surveyRespository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>()).Returns(survey);

        var request = new AddQuestionRequest(survey.SurveyId.Value, "", 0);

        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Question text cannot be empty (Parameter 'value')");
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidSurveyId_ShouldFail()
    {
        var request = new AddQuestionRequest(Guid.Empty, "Question?", 0);

        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
    }
}