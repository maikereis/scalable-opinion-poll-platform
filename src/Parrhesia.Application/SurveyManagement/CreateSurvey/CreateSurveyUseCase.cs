using Parrhesia.Application.Common;
using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.Repositories;

namespace Parrhesia.Application.SurveyManagement.CreateSurvey;

public class CreateSurveyUseCase : IUseCase<CreateSurveyRequest, Result<CreateSurveyResponse>>
{
    private readonly ISurveyRepository _surveyRepository;

    public CreateSurveyUseCase(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }

    public async Task<Result<CreateSurveyResponse>> ExecuteAsync(
        CreateSurveyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var survey = Survey.Create(
                request.Title,
                request.Description,
                request.StartDate,
                request.EndDate);

            await _surveyRepository.AddAsync(survey, cancellationToken);

            var response = new CreateSurveyResponse(
                survey.SurveyId.Value,
                survey.Title.Value,
                survey.Status.Value,
                survey.CollectionPeriod.StartDate,
                survey.CollectionPeriod.EndDate,
                survey.CreatedAt);

            return Result.Success(response);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<CreateSurveyResponse>(ex.Message);
        }
    }
}
