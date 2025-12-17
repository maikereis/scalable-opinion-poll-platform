using FluentAssertions;
using NSubstitute;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Aggregates;
using Parrhesia.Domain.Voting.Repositories;
using Parrhesia.Domain.Voting.Services;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Tests.Voting.Services;

public class VotingServiceTests
{
    private readonly IBallotRepository _ballotRepository;
    private readonly IFingerprintGenerator _fingerprintGenerator;
    private readonly ISurveyQueryService _surveyQueryService;
    private readonly VotingService _sut;

    private readonly UserId _userId;
    private readonly SurveyId _surveyId;
    private readonly QuestionId _questionId;
    private readonly OptionId _optionId;
    private readonly VoterFingerprint _fingerprint;

    public VotingServiceTests()
    {
        _ballotRepository = Substitute.For<IBallotRepository>();
        _fingerprintGenerator = Substitute.For<IFingerprintGenerator>();
        _surveyQueryService = Substitute.For<ISurveyQueryService>();

        _sut = new VotingService(_ballotRepository, _fingerprintGenerator, _surveyQueryService);

        _userId = UserId.Create(Guid.NewGuid());
        _surveyId = SurveyId.NewId();
        _questionId = QuestionId.NewId();
        _optionId = OptionId.NewId();
        _fingerprint = VoterFingerprint.Create("A1B2C3D4E5F6A1B2C3D4E5F6A1B2C3D4E5F6A1B2C3D4E5F6A1B2C3D4E5F6A1B2");
    }

    private SurveyStatusDto CreateActiveSurveyStatus()
    {
        return new SurveyStatusDto
        {
            SurveyId = _surveyId,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(7),
            QuestionsWithOptions = new Dictionary<QuestionId, List<OptionId>>
            {
                { _questionId, [_optionId, OptionId.NewId()] }
            }
        };
    }

    private void SetupHappyPath()
    {
        _surveyQueryService.GetSurveyStatusAsync(_surveyId, Arg.Any<CancellationToken>())
            .Returns(CreateActiveSurveyStatus());
        _fingerprintGenerator.Generate(_userId, _surveyId).Returns(_fingerprint);
        _ballotRepository.HasVotedAsync(_fingerprint, _surveyId, Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task CastVoteAsync_WithValidData_ShouldSucceed()
    {
        SetupHappyPath();

        var result = await _sut.CastVoteAsync(_userId, _surveyId, _questionId, _optionId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.SurveyId.Should().Be(_surveyId);
        result.Value.QuestionId.Should().Be(_questionId);
        result.Value.SelectedOptionId.Should().Be(_optionId);
    }

    [Fact]
    public async Task CastVoteAsync_ShouldPersistBallot()
    {
        SetupHappyPath();

        await _sut.CastVoteAsync(_userId, _surveyId, _questionId, _optionId);

        await _ballotRepository.Received(1).AddAsync(Arg.Any<Ballot>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CastVoteAsync_WhenSurveyNotFound_ShouldFail()
    {
        _surveyQueryService.GetSurveyStatusAsync(_surveyId, Arg.Any<CancellationToken>())
            .Returns((SurveyStatusDto?)null);

        var result = await _sut.CastVoteAsync(_userId, _surveyId, _questionId, _optionId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task CastVoteAsync_WhenSurveyNotActive_ShouldFail()
    {
        var inactiveSurvey = CreateActiveSurveyStatus();
        inactiveSurvey = inactiveSurvey with { IsActive = false };
        _surveyQueryService.GetSurveyStatusAsync(_surveyId, Arg.Any<CancellationToken>())
            .Returns(inactiveSurvey);

        var result = await _sut.CastVoteAsync(_userId, _surveyId, _questionId, _optionId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not active");
    }

    [Fact]
    public async Task CastVoteAsync_WhenOptionInvalid_ShouldFail()
    {
        _surveyQueryService.GetSurveyStatusAsync(_surveyId, Arg.Any<CancellationToken>())
            .Returns(CreateActiveSurveyStatus());
        var invalidOptionId = OptionId.NewId();

        var result = await _sut.CastVoteAsync(_userId, _surveyId, _questionId, invalidOptionId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid option");
    }

    [Fact]
    public async Task CastVoteAsync_WhenAlreadyVoted_ShouldFail()
    {
        _surveyQueryService.GetSurveyStatusAsync(_surveyId, Arg.Any<CancellationToken>())
            .Returns(CreateActiveSurveyStatus());
        _fingerprintGenerator.Generate(_userId, _surveyId).Returns(_fingerprint);
        _ballotRepository.HasVotedAsync(_fingerprint, _surveyId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.CastVoteAsync(_userId, _surveyId, _questionId, _optionId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already voted");
    }

    [Fact]
    public async Task CastVoteAsync_ShouldNotPersist_WhenValidationFails()
    {
        _surveyQueryService.GetSurveyStatusAsync(_surveyId, Arg.Any<CancellationToken>())
            .Returns((SurveyStatusDto?)null);

        await _sut.CastVoteAsync(_userId, _surveyId, _questionId, _optionId);

        await _ballotRepository.DidNotReceive().AddAsync(Arg.Any<Ballot>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CastVoteAsync_WithDeviceInfo_ShouldIncludeInBallot()
    {
        SetupHappyPath();
        var deviceInfo = DeviceInfo.Create("device-123", "Mozilla", "192.168.1.1");
        Ballot? capturedBallot = null;
        await _ballotRepository.AddAsync(Arg.Do<Ballot>(b => capturedBallot = b), Arg.Any<CancellationToken>());

        await _sut.CastVoteAsync(_userId, _surveyId, _questionId, _optionId, deviceInfo);

        capturedBallot.Should().NotBeNull();
        capturedBallot!.DeviceInfo.Should().Be(deviceInfo);
    }
}
