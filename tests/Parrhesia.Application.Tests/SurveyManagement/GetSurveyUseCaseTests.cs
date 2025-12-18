using FluentAssertions;
using NSubstitute;
using Parrhesia.Application.SurveyManagement.GetSurvey;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.Repositories;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Application.Tests.SurveyManagement;

public class GetSurveyUseCaseTests
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly GetSurveyUseCase _sut;

    public GetSurveyUseCaseTests()
    {
        _surveyRepository = Substitute.For<ISurveyRepository>();
        _sut = new GetSurveyUseCase(_surveyRepository);
    }

    private static Survey CreateCompleteSurvey()
    {
        var survey = Survey.Create(
            "Eleições 2026",
            "Pesquisa de intenção de voto",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(15));

        survey.AddQuestion("Qual seu candidato favorito?", 0);
        survey.AddQuestion("Qual partido você apoia?", 1);

        var q1Id = survey.Questions[0].QuestionId;
        var q2Id = survey.Questions[1].QuestionId;

        survey.AddOption(q1Id, "Candidato A", 0);
        survey.AddOption(q1Id, "Candidato B", 1);
        survey.AddOption(q1Id, "Candidato C", 2);

        survey.AddOption(q2Id, "Partido X", 0);
        survey.AddOption(q2Id, "Partido Y", 1);

        return survey;
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingSurvey_ShouldReturnSurvey()
    {
        var survey = CreateCompleteSurvey();
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>())
            .Returns(survey);

        var request = new GetSurveyRequest(survey.SurveyId.Value);

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Eleições 2026");
        result.Value.Description.Should().Be("Pesquisa de intenção de voto");
        result.Value.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnQuestionsInOrder()
    {
        var survey = CreateCompleteSurvey();
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>())
            .Returns(survey);

        var request = new GetSurveyRequest(survey.SurveyId.Value);

        var result = await _sut.ExecuteAsync(request);

        result.Value.Questions.Should().HaveCount(2);
        result.Value.Questions[0].Text.Should().Be("Qual seu candidato favorito?");
        result.Value.Questions[0].Order.Should().Be(0);
        result.Value.Questions[1].Text.Should().Be("Qual partido você apoia?");
        result.Value.Questions[1].Order.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnOptionsForEachQuestion()
    {
        var survey = CreateCompleteSurvey();
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>())
            .Returns(survey);

        var request = new GetSurveyRequest(survey.SurveyId.Value);

        var result = await _sut.ExecuteAsync(request);

        result.Value.Questions[0].Options.Should().HaveCount(3);
        result.Value.Questions[0].Options[0].Text.Should().Be("Candidato A");
        result.Value.Questions[1].Options.Should().HaveCount(2);
        result.Value.Questions[1].Options[0].Text.Should().Be("Partido X");
    }

    [Fact]
    public async Task ExecuteAsync_WhenSurveyNotFound_ShouldFail()
    {
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>())
            .Returns((Survey?)null);

        var request = new GetSurveyRequest(Guid.NewGuid());

        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidSurveyId_ShouldFail()
    {
        var request = new GetSurveyRequest(Guid.Empty);

        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnCorrectDates()
    {
        var survey = CreateCompleteSurvey();
        _surveyRepository.GetByIdAsync(Arg.Any<SurveyId>(), Arg.Any<CancellationToken>())
            .Returns(survey);

        var request = new GetSurveyRequest(survey.SurveyId.Value);

        var result = await _sut.ExecuteAsync(request);

        result.Value.StartDate.Should().Be(survey.CollectionPeriod.StartDate);
        result.Value.EndDate.Should().Be(survey.CollectionPeriod.EndDate);
        result.Value.CreatedAt.Should().Be(survey.CreatedAt);
    }
}
