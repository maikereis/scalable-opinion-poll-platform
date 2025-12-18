using Parrhesia.Application.Common;
using Parrhesia.Domain.Common;
using Parrhesia.Domain.Common.Exceptions;
using Parrhesia.Domain.SurveyManagement.Repositories;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Application.SurveyManagement.ActivateSurvey;

public class ActivateSurveyUseCase : IUseCase<ActivateSurveyRequest, Result<ActivateSurveyResponse>>
{
    private readonly ISurveyRepository _surveyRepository;
    private const int MaxActiveSurveys = 5;

    public ActivateSurveyUseCase(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }

    public async Task<Result<ActivateSurveyResponse>> ExecuteAsync(
        ActivateSurveyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var surveyId = SurveyId.Create(request.SurveyId);
            var survey = await _surveyRepository.GetByIdAsync(surveyId, cancellationToken);

            if (survey == null)
                return Result.Failure<ActivateSurveyResponse>("Survey not found");

            // Global rule: max 5 active surveys
            var activeCount = await _surveyRepository.CountActiveAsync(cancellationToken);
            if (activeCount >= MaxActiveSurveys)
                return Result.Failure<ActivateSurveyResponse>($"Maximum of {MaxActiveSurveys} active surveys reached");

            survey.Activate();

            await _surveyRepository.UpdateAsync(survey, cancellationToken);

            var response = new ActivateSurveyResponse(
                survey.SurveyId.Value,
                survey.Status.Value,
                survey.UpdatedAt);

            return Result.Success(response);
        }
        catch (DomainException ex)
        {
            return Result.Failure<ActivateSurveyResponse>(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<ActivateSurveyResponse>(ex.Message);
        }
    }
}
