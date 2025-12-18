using Parrhesia.Application.Common;
using Parrhesia.Domain.Common;
using Parrhesia.Domain.Common.Exceptions;
using Parrhesia.Domain.SurveyManagement.Repositories;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Application.SurveyManagement.AddOption;

public class AddOptionUseCase : IUseCase<AddOptionRequest, Result<AddOptionResponse>>
{
    private readonly ISurveyRepository _surveyRepository;

    public AddOptionUseCase(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }

    public async Task<Result<AddOptionResponse>> ExecuteAsync(
        AddOptionRequest request,
        CancellationToken cancellationToken = default)

    {
        try
        {
            var surveyId = SurveyId.Create(request.SurveyId);
            var questionId = QuestionId.Create(request.QuestionId);

            var survey = await _surveyRepository.GetByIdAsync(surveyId, cancellationToken);

            if (survey is null)
                return Result.Failure<AddOptionResponse>("Survey not found");

            survey.AddOption(questionId, request.Text, request.Order);

            await _surveyRepository.UpdateAsync(survey, cancellationToken);

            var addedOption = survey.Options.Last();

            var response = new AddOptionResponse(
                addedOption.OptionId.Value,
                addedOption.QuestionId.Value,
                addedOption.Text.Value,
                addedOption.Order
             );

            return Result.Success(response);
        }
        catch (DomainException ex)
        {
            return Result.Failure<AddOptionResponse>(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<AddOptionResponse>(ex.Message);
        }
    }
}