namespace Parrhesia.Application.SurveyManagement.GetSurvey;

public record GetSurveyRequest(
    Guid SurveyId
);

public record GetSurveyResponse(
    Guid SurveyId,
    string Title,
    string Description,
    string Status,
    DateTime StartDate,
    DateTime EndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<QuestionDto> Questions
);

public record QuestionDto(
    Guid QuestionId,
    string Text,
    int Order,
    IReadOnlyList<OptionDto> Options
);

public record OptionDto(
    Guid OptionId,
    string Text,
    int Order
);
