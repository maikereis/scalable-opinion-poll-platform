using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Infrastructure.Persistence.Repositories;

namespace Parrhesia.Infrastructure.Tests.Repositories;

public class SurveyRepositoryTests : IntegrationTestBase
{
    private readonly SurveyRepository _sut;

    public SurveyRepositoryTests()
    {
        _sut = new SurveyRepository(Context);
    }

    private static Survey CreateSurvey(string title = "Test Survey")
    {
        return Survey.Create(
            title,
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(15));
    }

    private static Survey CreateCompleteSurvey()
    {
        var survey = CreateSurvey();
        survey.AddQuestion("Question 1?", 0);
        survey.AddQuestion("Question 2?", 1);

        var q1Id = survey.Questions[0].QuestionId;
        var q2Id = survey.Questions[1].QuestionId;

        survey.AddOption(q1Id, "Option A", 0);
        survey.AddOption(q1Id, "Option B", 1);
        survey.AddOption(q2Id, "Option X", 0);
        survey.AddOption(q2Id, "Option Y", 1);

        return survey;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistSurvey()
    {
        var survey = CreateSurvey();

        await _sut.AddAsync(survey);

        var retrieved = await _sut.GetByIdAsync(survey.SurveyId);
        retrieved.Should().NotBeNull();
        retrieved!.Title.Value.Should().Be("Test Survey");
    }

    [Fact]
    public async Task AddAsync_ShouldPersistQuestionsAndOptions()
    {
        var survey = CreateCompleteSurvey();

        await _sut.AddAsync(survey);

        var retrieved = await _sut.GetByIdAsync(survey.SurveyId);
        retrieved!.Questions.Should().HaveCount(2);
        retrieved.Options.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        var result = await _sut.GetByIdAsync(SurveyId.NewId());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnQuestionsInOrder()
    {
        var survey = CreateCompleteSurvey();
        await _sut.AddAsync(survey);

        var retrieved = await _sut.GetByIdAsync(survey.SurveyId);

        retrieved!.Questions[0].Order.Should().Be(0);
        retrieved.Questions[1].Order.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var survey = CreateSurvey();
        await _sut.AddAsync(survey);

        survey.UpdateTitle("Updated Title");
        await _sut.UpdateAsync(survey);

        var retrieved = await _sut.GetByIdAsync(survey.SurveyId);
        retrieved!.Title.Value.Should().Be("Updated Title");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateQuestionsAndOptions()
    {
        var survey = CreateCompleteSurvey();
        await _sut.AddAsync(survey);

        survey.AddQuestion("Question 3?", 2);
        var q3Id = survey.Questions[2].QuestionId;
        survey.AddOption(q3Id, "Option Z", 0);
        survey.AddOption(q3Id, "Option W", 1);
        await _sut.UpdateAsync(survey);

        var retrieved = await _sut.GetByIdAsync(survey.SurveyId);
        retrieved!.Questions.Should().HaveCount(3);
        retrieved.Options.Should().HaveCount(6);
    }

    [Fact]
    public async Task CountActiveAsync_ShouldReturnCorrectCount()
    {
        var survey1 = CreateCompleteSurvey();
        var survey2 = CreateCompleteSurvey();
        var survey3 = CreateSurvey();

        await _sut.AddAsync(survey1);
        await _sut.AddAsync(survey2);
        await _sut.AddAsync(survey3);

        survey1.Activate();
        survey2.Activate();
        await _sut.UpdateAsync(survey1);
        await _sut.UpdateAsync(survey2);

        var count = await _sut.CountActiveAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task GetActiveAsync_ShouldReturnOnlyActiveSurveys()
    {
        var survey1 = CreateCompleteSurvey();
        var survey2 = CreateCompleteSurvey();
        var survey3 = CreateSurvey();

        await _sut.AddAsync(survey1);
        await _sut.AddAsync(survey2);
        await _sut.AddAsync(survey3);

        survey1.Activate();
        await _sut.UpdateAsync(survey1);

        var active = await _sut.GetActiveAsync();

        active.Should().HaveCount(1);
        active[0].SurveyId.Should().Be(survey1.SurveyId);
    }

    [Fact]
    public async Task ExistsAsync_WhenExists_ShouldReturnTrue()
    {
        var survey = CreateSurvey();
        await _sut.AddAsync(survey);

        var exists = await _sut.ExistsAsync(survey.SurveyId);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenNotExists_ShouldReturnFalse()
    {
        var exists = await _sut.ExistsAsync(SurveyId.NewId());

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistSurveyStatus()
    {
        var survey = CreateSurvey();

        await _sut.AddAsync(survey);

        var retrieved = await _sut.GetByIdAsync(survey.SurveyId);
        retrieved!.Status.IsDraft.Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistCollectionPeriod()
    {
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(15);
        var survey = Survey.Create("Test", "Desc", startDate, endDate);

        await _sut.AddAsync(survey);

        var retrieved = await _sut.GetByIdAsync(survey.SurveyId);
        retrieved!.CollectionPeriod.StartDate.Should().BeCloseTo(startDate, TimeSpan.FromSeconds(1));
        retrieved.CollectionPeriod.EndDate.Should().BeCloseTo(endDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistStatusChange()
    {
        var survey = CreateCompleteSurvey();
        await _sut.AddAsync(survey);

        survey.Activate();
        await _sut.UpdateAsync(survey);

        var retrieved = await _sut.GetByIdAsync(survey.SurveyId);
        retrieved!.Status.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_RemovingQuestion_ShouldRemoveQuestionAndOptions()
    {
        var survey = CreateCompleteSurvey();
        await _sut.AddAsync(survey);
        var questionToRemove = survey.Questions[0].QuestionId;

        survey.RemoveQuestion(questionToRemove);
        await _sut.UpdateAsync(survey);

        var retrieved = await _sut.GetByIdAsync(survey.SurveyId);
        retrieved!.Questions.Should().HaveCount(1);
        retrieved.Options.Should().HaveCount(2); // Only options from remaining question
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOptionsInOrder()
    {
        var survey = CreateSurvey();
        survey.AddQuestion("Q?", 0);
        var qId = survey.Questions[0].QuestionId;
        survey.AddOption(qId, "Option C", 2);
        survey.AddOption(qId, "Option A", 0);
        survey.AddOption(qId, "Option B", 1);
        await _sut.AddAsync(survey);

        var retrieved = await _sut.GetByIdAsync(survey.SurveyId);

        retrieved!.Options[0].Order.Should().Be(0);
        retrieved.Options[1].Order.Should().Be(1);
        retrieved.Options[2].Order.Should().Be(2);
    }

    [Fact]
    public async Task GetActiveAsync_WhenNoActiveSurveys_ShouldReturnEmptyList()
    {
        var survey = CreateSurvey();
        await _sut.AddAsync(survey);

        var active = await _sut.GetActiveAsync();

        active.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveAsync_ShouldReturnSurveysWithQuestionsAndOptions()
    {
        var survey = CreateCompleteSurvey();
        survey.Activate();
        await _sut.AddAsync(survey);

        var active = await _sut.GetActiveAsync();

        active.Should().HaveCount(1);
        active[0].Questions.Should().HaveCount(2);
        active[0].Options.Should().HaveCount(4);
    }

    [Fact]
    public async Task CountActiveAsync_WhenNoSurveys_ShouldReturnZero()
    {
        var count = await _sut.CountActiveAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task CountActiveAsync_ShouldNotCountClosedSurveys()
    {
        var survey = CreateCompleteSurvey();
        survey.Activate();
        await _sut.AddAsync(survey);

        survey.Close("Test");
        await _sut.UpdateAsync(survey);

        var count = await _sut.CountActiveAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task AddAsync_MultipleSurveys_ShouldPersistAll()
    {
        var survey1 = CreateSurvey("Survey 1");
        var survey2 = CreateSurvey("Survey 2");
        var survey3 = CreateSurvey("Survey 3");

        await _sut.AddAsync(survey1);
        await _sut.AddAsync(survey2);
        await _sut.AddAsync(survey3);

        var s1 = await _sut.GetByIdAsync(survey1.SurveyId);
        var s2 = await _sut.GetByIdAsync(survey2.SurveyId);
        var s3 = await _sut.GetByIdAsync(survey3.SurveyId);

        s1.Should().NotBeNull();
        s2.Should().NotBeNull();
        s3.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPreserveCreatedAt()
    {
        var survey = CreateSurvey();
        await _sut.AddAsync(survey);
        var originalCreatedAt = survey.CreatedAt;

        await Task.Delay(10);
        survey.UpdateTitle("New Title");
        await _sut.UpdateAsync(survey);

        var retrieved = await _sut.GetByIdAsync(survey.SurveyId);
        retrieved!.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldMapOptionsToCorrectQuestions()
    {
        var survey = CreateCompleteSurvey();
        await _sut.AddAsync(survey);

        var retrieved = await _sut.GetByIdAsync(survey.SurveyId);

        var q1Id = retrieved!.Questions[0].QuestionId;
        var q1Options = retrieved.GetOptionsForQuestion(q1Id);
        q1Options.Should().HaveCount(2);
        q1Options.All(o => o.QuestionId == q1Id).Should().BeTrue();
    }
}
