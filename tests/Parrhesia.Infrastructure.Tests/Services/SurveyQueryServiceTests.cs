using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Infrastructure.Persistence.Repositories;
using Parrhesia.Infrastructure.Persistence.Services;

namespace Parrhesia.Infrastructure.Tests.Services;

public class SurveyQueryServiceTests : IntegrationTestBase
{
    private readonly SurveyQueryService _sut;
    private readonly SurveyRepository _surveyRepository;

    public SurveyQueryServiceTests()
    {
        _sut = new SurveyQueryService(Context);
        _surveyRepository = new SurveyRepository(Context);
    }

    private static Survey CreateActiveSurvey()
    {
        var survey = Survey.Create(
            "Test Survey",
            "Description",
            DateTime.UtcNow.AddDays(-1), // Started yesterday
            DateTime.UtcNow.AddDays(15));

        survey.AddQuestion("Question?", 0);
        var qId = survey.Questions[0].QuestionId;
        survey.AddOption(qId, "Option A", 0);
        survey.AddOption(qId, "Option B", 1);
        survey.Activate();

        return survey;
    }

    [Fact]
    public async Task GetSurveyStatusAsync_WhenActive_ShouldReturnIsActiveTrue()
    {
        var survey = CreateActiveSurvey();
        await _surveyRepository.AddAsync(survey);

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status.Should().NotBeNull();
        status!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetSurveyStatusAsync_WhenDraft_ShouldReturnIsActiveFalse()
    {
        var survey = Survey.Create(
            "Draft Survey",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(15));
        await _surveyRepository.AddAsync(survey);

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status.Should().NotBeNull();
        status!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetSurveyStatusAsync_WhenNotFound_ShouldReturnNull()
    {
        var status = await _sut.GetSurveyStatusAsync(SurveyId.NewId());

        status.Should().BeNull();
    }

    [Fact]
    public async Task GetSurveyStatusAsync_ShouldReturnQuestionsWithOptions()
    {
        var survey = CreateActiveSurvey();
        await _surveyRepository.AddAsync(survey);

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status!.QuestionsWithOptions.Should().HaveCount(1);
        var questionId = survey.Questions[0].QuestionId;
        status.QuestionsWithOptions[questionId].Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSurveyStatusAsync_HasOption_ShouldReturnTrueForValidOption()
    {
        var survey = CreateActiveSurvey();
        await _surveyRepository.AddAsync(survey);
        var questionId = survey.Questions[0].QuestionId;
        var optionId = survey.Options[0].OptionId;

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status!.HasOption(questionId, optionId).Should().BeTrue();
    }

    [Fact]
    public async Task GetSurveyStatusAsync_HasOption_ShouldReturnFalseForInvalidOption()
    {
        var survey = CreateActiveSurvey();
        await _surveyRepository.AddAsync(survey);
        var questionId = survey.Questions[0].QuestionId;

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status!.HasOption(questionId, OptionId.NewId()).Should().BeFalse();
    }

    [Fact]
    public async Task GetSurveyStatusAsync_WhenPeriodEnded_ShouldReturnIsActiveFalse()
    {
        var survey = Survey.Create(
            "Ended Survey",
            "Description",
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-5)); // Ended 5 days ago

        survey.AddQuestion("Q?", 0);
        survey.AddOption(survey.Questions[0].QuestionId, "A", 0);
        survey.AddOption(survey.Questions[0].QuestionId, "B", 1);
        survey.Activate();
        await _surveyRepository.AddAsync(survey);

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetSurveyStatusAsync_WhenPeriodNotStarted_ShouldReturnIsActiveFalse()
    {
        var survey = Survey.Create(
            "Future Survey",
            "Description",
            DateTime.UtcNow.AddDays(5), // Starts in 5 days
            DateTime.UtcNow.AddDays(15));

        survey.AddQuestion("Q?", 0);
        survey.AddOption(survey.Questions[0].QuestionId, "A", 0);
        survey.AddOption(survey.Questions[0].QuestionId, "B", 1);
        survey.Activate();
        await _surveyRepository.AddAsync(survey);

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetSurveyStatusAsync_ShouldReturnCorrectDates()
    {
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(15);
        var survey = Survey.Create("Test", "Desc", startDate, endDate);
        survey.AddQuestion("Q?", 0);
        survey.AddOption(survey.Questions[0].QuestionId, "A", 0);
        survey.AddOption(survey.Questions[0].QuestionId, "B", 1);
        survey.Activate();
        await _surveyRepository.AddAsync(survey);

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status!.StartDate.Should().BeCloseTo(startDate, TimeSpan.FromSeconds(1));
        status.EndDate.Should().BeCloseTo(endDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetSurveyStatusAsync_MultipleQuestions_ShouldReturnAllQuestionsWithOptions()
    {
        var survey = Survey.Create(
            "Multi Question Survey",
            "Description",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(15));

        survey.AddQuestion("Question 1?", 0);
        survey.AddQuestion("Question 2?", 1);
        survey.AddQuestion("Question 3?", 2);

        var q1 = survey.Questions[0].QuestionId;
        var q2 = survey.Questions[1].QuestionId;
        var q3 = survey.Questions[2].QuestionId;

        survey.AddOption(q1, "A1", 0);
        survey.AddOption(q1, "A2", 1);
        survey.AddOption(q2, "B1", 0);
        survey.AddOption(q2, "B2", 1);
        survey.AddOption(q2, "B3", 2);
        survey.AddOption(q3, "C1", 0);
        survey.AddOption(q3, "C2", 1);

        survey.Activate();
        await _surveyRepository.AddAsync(survey);

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status!.QuestionsWithOptions.Should().HaveCount(3);
        status.QuestionsWithOptions[q1].Should().HaveCount(2);
        status.QuestionsWithOptions[q2].Should().HaveCount(3);
        status.QuestionsWithOptions[q3].Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSurveyStatusAsync_HasOption_InvalidQuestion_ShouldReturnFalse()
    {
        var survey = CreateActiveSurvey();
        await _surveyRepository.AddAsync(survey);
        var validOptionId = survey.Options[0].OptionId;

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status!.HasOption(QuestionId.NewId(), validOptionId).Should().BeFalse();
    }

    [Fact]
    public async Task GetSurveyStatusAsync_WhenClosed_ShouldReturnIsActiveFalse()
    {
        var survey = Survey.Create(
            "Closed Survey",
            "Description",
            DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow.AddDays(10));

        survey.AddQuestion("Q?", 0);
        survey.AddOption(survey.Questions[0].QuestionId, "A", 0);
        survey.AddOption(survey.Questions[0].QuestionId, "B", 1);
        survey.Activate();
        survey.Close("Manual close");
        await _surveyRepository.AddAsync(survey);

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetSurveyStatusAsync_ShouldReturnCorrectSurveyId()
    {
        var survey = CreateActiveSurvey();
        await _surveyRepository.AddAsync(survey);

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status!.SurveyId.Should().Be(survey.SurveyId);
    }

    [Fact]
    public async Task GetSurveyStatusAsync_ActiveWithinPeriod_ShouldReturnIsActiveTrue()
    {
        var survey = Survey.Create(
            "Active Survey",
            "Description",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1));

        survey.AddQuestion("Q?", 0);
        survey.AddOption(survey.Questions[0].QuestionId, "A", 0);
        survey.AddOption(survey.Questions[0].QuestionId, "B", 1);
        survey.Activate();
        await _surveyRepository.AddAsync(survey);

        var status = await _sut.GetSurveyStatusAsync(survey.SurveyId);

        status!.IsActive.Should().BeTrue();
    }
}
