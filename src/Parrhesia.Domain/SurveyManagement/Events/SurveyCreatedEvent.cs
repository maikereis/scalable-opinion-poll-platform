using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.SurveyManagement.Events;

public class SurveyCreatedEvent : IDomainEvent
{
    public SurveyId SurveyId { get; }
    public SurveyTitle Title { get; }
    public CollectionPeriod CollectionPeriod { get; }
    public DateTime OccurredAt { get; }

    public SurveyCreatedEvent(SurveyId surveyId, SurveyTitle title, CollectionPeriod period)
    {
        SurveyId = surveyId;
        Title = title;
        CollectionPeriod = period;
        OccurredAt = DateTime.UtcNow;
    }
}
