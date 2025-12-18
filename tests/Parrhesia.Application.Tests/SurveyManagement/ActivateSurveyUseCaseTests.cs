using FluentAssertions;
using NSubstitute;
using Parrhesia.Application.SurveyManagement.ActivateSurvey;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.Repositories;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Application.Tests.SurveyManagement;

public class ActivateSurveyUseCaseTests
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly ActivateSurveyUseCase _sut;

    public ActivateSurveyUseCaseTests()
    {
        _surveyRepository = Substitute.For<ISurveyRepository>();
        _sut = new ActivateSurveyUseCase(_surveyRepository);
    }

    private static Survey CreateReadySurvey()
    {
        var survey = Survey.Create(
            "Test Survey",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(15));

        survey.AddQuestion("Question 1?", 0);
        var questionId = survey.Questions[0].QuestionId;
        survey.AddOption(questionId, "Option A", 0);
        survey.AddOption(questionId, "Option B", 1);

        return survey;
    }

    [Fact]
    public async Task ExecuteAsync_WithValidSurvey_ShouldActivate()
    {
        var survey = CreateReadySurvey();
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>())
            .Returns(survey);
        _surveyRepository.CountActiveAsync(Arg.Any<CancellationToken>()).Returns(0);

        var request = new ActivateSurveyRequest(survey.SurveyId.Value);
        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Active");
    }

    [Fact]
    public async Task ExecuteAsync_WhenSurveyNotFound_ShouldFail()
    {
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>())
            .Returns((Survey?)null);

        var request = new ActivateSurveyRequest(Guid.NewGuid());
        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task ExecuteAsync_WhenMaxActiveSurveysReached_ShouldFail()
    {
        var survey = CreateReadySurvey();
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>())
            .Returns(survey);
        _surveyRepository.CountActiveAsync(Arg.Any<CancellationToken>()).Returns(5);

        var request = new ActivateSurveyRequest(survey.SurveyId.Value);
        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Maximum");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPersistChanges()
    {
        var survey = CreateReadySurvey();
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>())
            .Returns(survey);
        _surveyRepository.CountActiveAsync(Arg.Any<CancellationToken>()).Returns(0);

        var request = new ActivateSurveyRequest(survey.SurveyId.Value);
        await _sut.ExecuteAsync(request);

        await _surveyRepository.Received(1).UpdateAsync(Arg.Any<Survey>(), Arg.Any<CancellationToken>());
    }
}
