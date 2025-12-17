using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.SurveyManagement.Events;

public class SurveyActivatedEvent : IDomainEvent
{
    public SurveyId SurveyId { get; }
    public DateTime ActivatedAt { get; }
    public DateTime OccurredAt { get; }

    public SurveyActivatedEvent(SurveyId surveyId, DateTime activatedAt)
    {
        SurveyId = surveyId;
        ActivatedAt = activatedAt;
        OccurredAt = DateTime.UtcNow;
    }
}
