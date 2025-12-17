namespace Parrhesia.Application.SurveyManagement.CreateSurvey;

public record CreateSurveyRequest(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate
);

public record CreateSurveyResponse(
    Guid SurveyId,
    string Title,
    string Status,
    DateTime StartDate,
    DateTime EndDate,
    DateTime CreatedAt
);