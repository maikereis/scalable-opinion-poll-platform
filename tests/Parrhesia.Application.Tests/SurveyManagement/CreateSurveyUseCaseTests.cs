using FluentAssertions;
using NSubstitute;
using Parrhesia.Application.SurveyManagement.CreateSurvey;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.Repositories;

namespace Parrhesia.Application.Tests.SurveyManagement;

public class CreateSurveyUseCaseTests
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly CreateSurveyUseCase _sut;

    public CreateSurveyUseCaseTests()
    {
        _surveyRepository = Substitute.For<ISurveyRepository>();
        _sut = new CreateSurveyUseCase(_surveyRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldSucceed()
    {
        var request = new CreateSurveyRequest(
            "Eleições 2026",
            "Pesquisa de intenção de voto",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(15)
        );

        var result = await _sut.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(request.Title);
        result.Value.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPersistSurvey()
    {
        var request = new CreateSurveyRequest(
            "Test Survey",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(15)
        );

        await _sut.ExecuteAsync(request);

        await _surveyRepository.Received(1).AddAsync(Arg.Any<Survey>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyTitle_ShouldFail()
    {
        var request = new CreateSurveyRequest(
            "",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(15)
        );
        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Survey title cannot be empty (Parameter 'value')");
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPeriod_ShouldFail()
    {
        var request = new CreateSurveyRequest(
            "Test Survey",
            "Description",
            DateTime.UtcNow.AddDays(15),
            DateTime.UtcNow.AddDays(1)
        );

        var result = await _sut.ExecuteAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("End date must be after start date (Parameter 'endDate')");
    }
}