using FluentAssertions;
using NSubstitute;
using Parrhesia.Application.SurveyManagement.AddOption;
using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.Repositories;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Application.Tests.SurveyManagement;

public class AddOptionUseCaseTests
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly AddOptionUseCase _sut;

    public AddOptionUseCaseTests()
    {
        _surveyRepository = Substitute.For<ISurveyRepository>();
        _sut = new AddOptionUseCase(_surveyRepository);
    }

    private static Survey CreateSurveyWithQuestion()
    {
        var survey = Survey.Create(
            "Test Survey",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(15)
        );

        survey.AddQuestion("Test question?", 0);
        return survey;
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldAddOption()
    {
        var survey = CreateSurveyWithQuestion();
        var questionId = survey.Questions[0].QuestionId;
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>()).Returns(survey);

        var request = new AddOptionRequest(
            survey.SurveyId.Value,
            questionId.Value,
            "Candidato A",
            0
        );

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Text.Should().Be("Candidato A");
        result.Value.QuestionId.Should().Be(questionId.Value);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPersistChanges()
    {
        var survey = CreateSurveyWithQuestion();
        var questionId = survey.Questions[0].QuestionId;
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>()).Returns(survey);

        var request = new AddOptionRequest(survey.SurveyId.Value, questionId.Value, "Option", 0);

        await _sut.ExecuteAsync(request);

        await _surveyRepository.Received(1).UpdateAsync(Arg.Any<Survey>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenSurveyNotFound_ShouldFail()
    {
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>()).Returns((Survey?)null);

        var request = new AddOptionRequest(Guid.NewGuid(), Guid.NewGuid(), "Option", 0);

        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task ExecuteAsync_WhenQuestionNotFound_ShouldFail()
    {
        var survey = CreateSurveyWithQuestion();
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>()).Returns(survey);

        var request = new AddOptionRequest(
            survey.SurveyId.Value,
            Guid.NewGuid(),
            "Option",
            0);

        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Question not found");
    }

    [Fact]
    public async Task ExecuteAsync_WhenSurveyIsActive_ShouldFail()
    {
        var survey = CreateSurveyWithQuestion();
        var questionId = survey.Questions[0].QuestionId;
        survey.AddOption(questionId, "Option A", 0);
        survey.AddOption(questionId, "Option B", 1);
        survey.Activate();

        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>()).Returns(survey);

        var request = new AddOptionRequest(survey.SurveyId.Value, questionId.Value, "Option C", 2);

        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot modify");
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyText_ShouldFail()
    {
        var survey = CreateSurveyWithQuestion();
        var questionId = survey.Questions[0].QuestionId;
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>()).Returns(survey);

        var request = new AddOptionRequest(survey.SurveyId.Value, questionId.Value, "", 0);

        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidSurveyId_ShouldFail()
    {
        var request = new AddOptionRequest(Guid.Empty, Guid.NewGuid(), "Option", 0);

        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
    }
}
