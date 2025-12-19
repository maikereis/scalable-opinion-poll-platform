using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Events;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Voting.Aggregates;

public class Ballot : AggregateRoot
{
    public BallotId BallotId { get; } = null!;
    public SurveyId SurveyId { get; } = null!;
    public QuestionId QuestionId { get; } = null!;
    public OptionId SelectedOptionId { get; } = null!;
    public VoterFingerprint VoterFingerprint { get; } = null!;
    public DateTime CastedAt { get; }
    public DeviceInfo? DeviceInfo { get; }

    /// <summary>
    /// EF Core constructor - required for materialization
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value
    private Ballot() : base() { }
#pragma warning restore CS8618

    private Ballot(
        SurveyId surveyId,
        QuestionId questionId,
        OptionId selectedOptionId,
        VoterFingerprint fingerprint,
        DeviceInfo? deviceInfo) : base()
    {
        BallotId = BallotId.Create(Id);
        SurveyId = surveyId;
        QuestionId = questionId;
        SelectedOptionId = selectedOptionId;
        VoterFingerprint = fingerprint;
        DeviceInfo = deviceInfo;
        CastedAt = DateTime.UtcNow;
    }

    private Ballot(
        BallotId ballotId,
        SurveyId surveyId,
        QuestionId questionId,
        OptionId selectedOptionId,
        VoterFingerprint fingerprint,
        DateTime castedAt,
        DeviceInfo? deviceInfo) : base(ballotId.Value)
    {
        BallotId = ballotId;
        SurveyId = surveyId;
        QuestionId = questionId;
        SelectedOptionId = selectedOptionId;
        VoterFingerprint = fingerprint;
        CastedAt = castedAt;
        DeviceInfo = deviceInfo;
    }

    public static Ballot Cast(
        SurveyId surveyId,
        QuestionId questionId,
        OptionId selectedOptionId,
        VoterFingerprint fingerprint,
        DeviceInfo? deviceInfo = null)
    {
        ArgumentNullException.ThrowIfNull(surveyId);
        ArgumentNullException.ThrowIfNull(questionId);
        ArgumentNullException.ThrowIfNull(selectedOptionId);
        ArgumentNullException.ThrowIfNull(fingerprint);

        var ballot = new Ballot(surveyId, questionId, selectedOptionId, fingerprint, deviceInfo);

        ballot.AddDomainEvent(new BallotCastedEvent(
            ballot.BallotId,
            ballot.SurveyId,
            ballot.QuestionId,
            ballot.SelectedOptionId,
            ballot.CastedAt));

        return ballot;
    }

    public static Ballot Reconstitute(
        BallotId ballotId,
        SurveyId surveyId,
        QuestionId questionId,
        OptionId selectedOptionId,
        VoterFingerprint fingerprint,
        DateTime castedAt,
        DeviceInfo? deviceInfo = null)
    {
        return new Ballot(ballotId, surveyId, questionId, selectedOptionId, fingerprint, castedAt, deviceInfo);
    }
}