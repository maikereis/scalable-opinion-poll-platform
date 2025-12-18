namespace Parrhesia.Application.SurveyManagement.AddOption;

public record AddOptionRequest(
    Guid SurveyId,
    Guid QuestionId,
    string Text,
    int Order
);

public record AddOptionResponse(
    Guid OptionId,
    Guid QuestionId,
    string Text,
    int Order
);