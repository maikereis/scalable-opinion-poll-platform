using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Aggregates;
using Parrhesia.Domain.Voting.ValueObjects;
using Parrhesia.Infrastructure.Persistence.Repositories;

namespace Parrhesia.Infrastructure.Tests.Repositories;

public class BallotRepositoryTests : IntegrationTestBase
{
    private readonly BallotRepository _sut;

    public BallotRepositoryTests()
    {
        _sut = new BallotRepository(Context);
    }

    private static VoterFingerprint CreateFingerprint(string seed = "default")
    {
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(seed));
        return VoterFingerprint.Create(Convert.ToHexString(hash));
    }

    private static Ballot CreateBallot(
        SurveyId? surveyId = null,
        OptionId? optionId = null,
        VoterFingerprint? fingerprint = null)
    {
        return Ballot.Cast(
            surveyId ?? SurveyId.NewId(),
            QuestionId.NewId(),
            optionId ?? OptionId.NewId(),
            fingerprint ?? CreateFingerprint());
    }

    [Fact]
    public async Task AddAsync_ShouldPersistBallot()
    {
        var ballot = CreateBallot();

        await _sut.AddAsync(ballot);

        Context.Ballots.Should().ContainSingle();
    }

    [Fact]
    public async Task HasVotedAsync_WhenVoted_ShouldReturnTrue()
    {
        var surveyId = SurveyId.NewId();
        var fingerprint = CreateFingerprint("user1");
        var ballot = CreateBallot(surveyId: surveyId, fingerprint: fingerprint);
        await _sut.AddAsync(ballot);

        var hasVoted = await _sut.HasVotedAsync(fingerprint, surveyId);

        hasVoted.Should().BeTrue();
    }

    [Fact]
    public async Task HasVotedAsync_WhenNotVoted_ShouldReturnFalse()
    {
        var surveyId = SurveyId.NewId();
        var fingerprint = CreateFingerprint("user1");

        var hasVoted = await _sut.HasVotedAsync(fingerprint, surveyId);

        hasVoted.Should().BeFalse();
    }

    [Fact]
    public async Task HasVotedAsync_DifferentSurvey_ShouldReturnFalse()
    {
        var surveyId1 = SurveyId.NewId();
        var surveyId2 = SurveyId.NewId();
        var fingerprint = CreateFingerprint("user1");
        var ballot = CreateBallot(surveyId: surveyId1, fingerprint: fingerprint);
        await _sut.AddAsync(ballot);

        var hasVoted = await _sut.HasVotedAsync(fingerprint, surveyId2);

        hasVoted.Should().BeFalse();
    }

    [Fact]
    public async Task CountBySurveyAsync_ShouldReturnCorrectCount()
    {
        var surveyId = SurveyId.NewId();
        await _sut.AddAsync(CreateBallot(surveyId: surveyId, fingerprint: CreateFingerprint("user1")));
        await _sut.AddAsync(CreateBallot(surveyId: surveyId, fingerprint: CreateFingerprint("user2")));
        await _sut.AddAsync(CreateBallot(surveyId: surveyId, fingerprint: CreateFingerprint("user3")));
        await _sut.AddAsync(CreateBallot(fingerprint: CreateFingerprint("user4"))); // Different survey

        var count = await _sut.CountBySurveyAsync(surveyId);

        count.Should().Be(3);
    }

    [Fact]
    public async Task CountByOptionAsync_ShouldReturnCorrectCount()
    {
        var optionId = OptionId.NewId();
        await _sut.AddAsync(CreateBallot(optionId: optionId, fingerprint: CreateFingerprint("user1")));
        await _sut.AddAsync(CreateBallot(optionId: optionId, fingerprint: CreateFingerprint("user2")));
        await _sut.AddAsync(CreateBallot(fingerprint: CreateFingerprint("user3"))); // Different option

        var count = await _sut.CountByOptionAsync(optionId);

        count.Should().Be(2);
    }

    [Fact]
    public async Task AddAsync_WithDeviceInfo_ShouldPersistDeviceInfo()
    {
        var deviceInfo = DeviceInfo.Create("device-123", "Mozilla/5.0", "192.168.1.1");
        var ballot = Ballot.Cast(
            SurveyId.NewId(),
            QuestionId.NewId(),
            OptionId.NewId(),
            CreateFingerprint(),
            deviceInfo);

        await _sut.AddAsync(ballot);

        var persisted = Context.Ballots.First();
        persisted.DeviceInfo.Should().NotBeNull();
        persisted.DeviceInfo!.DeviceId.Should().Be("device-123");
    }

    [Fact]
    public async Task AddAsync_WithoutDeviceInfo_ShouldPersistNullDeviceInfo()
    {
        var ballot = CreateBallot();

        await _sut.AddAsync(ballot);

        var persisted = Context.Ballots.First();
        persisted.DeviceInfo.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistCastedAt()
    {
        var ballot = CreateBallot();

        await _sut.AddAsync(ballot);

        var persisted = Context.Ballots.First();
        // Use BeCloseTo instead of BeOnOrAfter to avoid race conditions
        persisted.CastedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddAsync_ShouldPersistAllIds()
    {
        var surveyId = SurveyId.NewId();
        var questionId = QuestionId.NewId();
        var optionId = OptionId.NewId();
        var ballot = Ballot.Cast(surveyId, questionId, optionId, CreateFingerprint());

        await _sut.AddAsync(ballot);

        var persisted = Context.Ballots.First();
        persisted.SurveyId.Should().Be(surveyId);
        persisted.QuestionId.Should().Be(questionId);
        persisted.SelectedOptionId.Should().Be(optionId);
    }

    [Fact]
    public async Task HasVotedAsync_SameUserDifferentSurveys_ShouldTrackSeparately()
    {
        var fingerprint = CreateFingerprint("user1");
        var survey1 = SurveyId.NewId();
        var survey2 = SurveyId.NewId();

        await _sut.AddAsync(CreateBallot(surveyId: survey1, fingerprint: fingerprint));

        var hasVotedSurvey1 = await _sut.HasVotedAsync(fingerprint, survey1);
        var hasVotedSurvey2 = await _sut.HasVotedAsync(fingerprint, survey2);

        hasVotedSurvey1.Should().BeTrue();
        hasVotedSurvey2.Should().BeFalse();
    }

    [Fact]
    public async Task CountBySurveyAsync_WhenNoBallots_ShouldReturnZero()
    {
        var count = await _sut.CountBySurveyAsync(SurveyId.NewId());

        count.Should().Be(0);
    }

    [Fact]
    public async Task CountByOptionAsync_WhenNoBallots_ShouldReturnZero()
    {
        var count = await _sut.CountByOptionAsync(OptionId.NewId());

        count.Should().Be(0);
    }

    [Fact]
    public async Task CountBySurveyAsync_ShouldHandleLargeNumbers()
    {
        var surveyId = SurveyId.NewId();

        for (int i = 0; i < 100; i++)
        {
            await _sut.AddAsync(CreateBallot(
                surveyId: surveyId,
                fingerprint: CreateFingerprint($"user{i}")));
        }

        var count = await _sut.CountBySurveyAsync(surveyId);

        count.Should().Be(100);
    }

    [Fact]
    public async Task CountByOptionAsync_MultipleOptions_ShouldCountCorrectly()
    {
        var option1 = OptionId.NewId();
        var option2 = OptionId.NewId();
        var option3 = OptionId.NewId();

        await _sut.AddAsync(CreateBallot(optionId: option1, fingerprint: CreateFingerprint("u1")));
        await _sut.AddAsync(CreateBallot(optionId: option1, fingerprint: CreateFingerprint("u2")));
        await _sut.AddAsync(CreateBallot(optionId: option1, fingerprint: CreateFingerprint("u3")));
        await _sut.AddAsync(CreateBallot(optionId: option2, fingerprint: CreateFingerprint("u4")));
        await _sut.AddAsync(CreateBallot(optionId: option2, fingerprint: CreateFingerprint("u5")));
        await _sut.AddAsync(CreateBallot(optionId: option3, fingerprint: CreateFingerprint("u6")));

        var count1 = await _sut.CountByOptionAsync(option1);
        var count2 = await _sut.CountByOptionAsync(option2);
        var count3 = await _sut.CountByOptionAsync(option3);

        count1.Should().Be(3);
        count2.Should().Be(2);
        count3.Should().Be(1);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistVoterFingerprint()
    {
        var fingerprint = CreateFingerprint("unique-user");
        var ballot = CreateBallot(fingerprint: fingerprint);

        await _sut.AddAsync(ballot);

        var persisted = Context.Ballots.First();
        persisted.VoterFingerprint.Should().Be(fingerprint);
    }

    [Fact]
    public async Task AddAsync_DeviceInfo_ShouldPersistUserAgent()
    {
        var deviceInfo = DeviceInfo.Create("dev", "Custom UserAgent String", "10.0.0.1");
        var ballot = Ballot.Cast(
            SurveyId.NewId(),
            QuestionId.NewId(),
            OptionId.NewId(),
            CreateFingerprint(),
            deviceInfo);

        await _sut.AddAsync(ballot);

        var persisted = Context.Ballots.First();
        persisted.DeviceInfo!.UserAgent.Should().Be("Custom UserAgent String");
    }

    [Fact]
    public async Task AddAsync_DeviceInfo_ShouldHashIpAddress()
    {
        var deviceInfo = DeviceInfo.Create("dev", "Mozilla", "192.168.1.100");
        var ballot = Ballot.Cast(
            SurveyId.NewId(),
            QuestionId.NewId(),
            OptionId.NewId(),
            CreateFingerprint(),
            deviceInfo);

        await _sut.AddAsync(ballot);

        var persisted = Context.Ballots.First();
        persisted.DeviceInfo!.IpHash.Should().NotBe("192.168.1.100");
        persisted.DeviceInfo.IpHash.Should().HaveLength(64); // SHA256 hex
    }
}
