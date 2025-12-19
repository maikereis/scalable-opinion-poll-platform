using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.Voting.Services;

/// <summary>
/// Anti-Corruption Layer: Protects Voting Context from Survey Management Context changes.
/// Interface defined in Voting Context, implementation in Infrastructure.
/// </summary>
public interface ISurveyQueryService
{
    Task<SurveyStatusDto?> GetSurveyStatusAsync(SurveyId surveyId, CancellationToken cancellationToken = default);
}

public record SurveyStatusDto
{
    public SurveyId SurveyId { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Dictionary<QuestionId, List<OptionId>> QuestionsWithOptions { get; init; } = [];

    public bool HasOption(QuestionId questionId, OptionId optionId)
    {
        var question = QuestionsWithOptions
            .FirstOrDefault(kvp => kvp.Key.Value == questionId.Value);

        if (question.Key == null || question.Value == null)
            return false;

        return question.Value
            .Where(o => o != null) 
            .Any(o => o.Value == optionId.Value);
    }
}