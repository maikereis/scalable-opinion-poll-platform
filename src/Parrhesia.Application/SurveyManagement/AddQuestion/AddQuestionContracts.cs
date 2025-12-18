namespace Parrhesia.Application.SurveyManagement.AddQuestion;

public record AddQuestionRequest(
    Guid SurveyId,
    string Text,
    int Order
);

public record AddQuestionResponse(
    Guid QuestionId,
    string Text,
    int Order
);