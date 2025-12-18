using Parrhesia.Application.Common;
using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.Repositories;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Application.SurveyManagement.GetSurvey;

public class GetSurveyUseCase : IUseCase<GetSurveyRequest, Result<GetSurveyResponse>>
{
    private readonly ISurveyRepository _surveyRepository;

    public GetSurveyUseCase(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }

    public async Task<Result<GetSurveyResponse>> ExecuteAsync(
        GetSurveyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var surveyId = SurveyId.Create(request.SurveyId);
            var survey = await _surveyRepository.GetByIdAsync(surveyId, cancellationToken);

            if (survey == null)
                return Result.Failure<GetSurveyResponse>("Survey not found");

            var questions = survey.Questions
                .OrderBy(q => q.Order)
                .Select(q => new QuestionDto(
                    q.QuestionId.Value,
                    q.Text.Value,
                    q.Order,
                    survey.GetOptionsForQuestion(q.QuestionId)
                        .Select(o => new OptionDto(o.OptionId.Value, o.Text.Value, o.Order))
                        .ToList()))
                .ToList();

            var response = new GetSurveyResponse(
                survey.SurveyId.Value,
                survey.Title.Value,
                survey.Description,
                survey.Status.Value,
                survey.CollectionPeriod.StartDate,
                survey.CollectionPeriod.EndDate,
                survey.CreatedAt,
                survey.UpdatedAt,
                questions);

            return Result.Success(response);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<GetSurveyResponse>(ex.Message);
        }
    }
}
