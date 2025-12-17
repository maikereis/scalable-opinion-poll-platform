using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.SurveyManagement.Events;

public class SurveyClosedEvent : IDomainEvent
{
    public SurveyId SurveyId { get; }
    public DateTime ClosedAt { get; }
    public string Reason { get; }
    public DateTime OccurredAt { get; }

    public SurveyClosedEvent(SurveyId surveyId, DateTime closedAt, string reason)
    {
        SurveyId = surveyId;
        ClosedAt = closedAt;
        Reason = reason;
        OccurredAt = DateTime.UtcNow;
    }
}
