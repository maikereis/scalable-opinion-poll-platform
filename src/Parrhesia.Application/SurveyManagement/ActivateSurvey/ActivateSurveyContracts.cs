namespace Parrhesia.Application.SurveyManagement.ActivateSurvey;

public record ActivateSurveyRequest(
    Guid SurveyId
);

public record ActivateSurveyResponse(
    Guid SurveyId,
    string Status,
    DateTime ActivatedAt
);
