using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Events;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Voting.Aggregates;

public class Ballot : AggregateRoot
{
    public BallotId BallotId { get; }
    public SurveyId SurveyId { get; }
    public QuestionId QuestionId { get; }
    public OptionId SelectedOptionId { get; }
    public VoterFingerprint VoterFingerprint { get; }
    public DateTime CastedAt { get; }
    public DeviceInfo? DeviceInfo { get; }

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
