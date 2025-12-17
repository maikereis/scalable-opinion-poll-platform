using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Voting.Events;

public class BallotCastedEvent : IDomainEvent
{
    public BallotId BallotId { get; }
    public SurveyId SurveyId { get; }
    public QuestionId QuestionId { get; }
    public OptionId SelectedOptionId { get; }
    public DateTime CastedAt { get; }
    public DateTime OccurredAt { get; }

    public BallotCastedEvent(
        BallotId ballotId,
        SurveyId surveyId,
        QuestionId questionId,
        OptionId selectedOptionId,
        DateTime castedAt)
    {
        BallotId = ballotId;
        SurveyId = surveyId;
        QuestionId = questionId;
        SelectedOptionId = selectedOptionId;
        CastedAt = castedAt;
        OccurredAt = DateTime.UtcNow;
    }
}
