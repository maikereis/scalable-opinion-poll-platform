using Parrhesia.Application.Common;
using Parrhesia.Domain.Common;
using Parrhesia.Domain.Common.Exceptions;
using Parrhesia.Domain.SurveyManagement.Repositories;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Application.SurveyManagement.AddQuestion;

public class AddQuestionUseCase : IUseCase<AddQuestionRequest, Result<AddQuestionResponse>>
{
    private readonly ISurveyRepository _surveyRepository;

    public AddQuestionUseCase(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }

    public async Task<Result<AddQuestionResponse>> ExecuteAsync(
        AddQuestionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var surveyId = SurveyId.Create(request.SurveyId);
            var survey = await _surveyRepository.GetByIdAsync(surveyId, cancellationToken);

            if (survey is null)
            {
                return Result.Failure<AddQuestionResponse>("Survey not found");
            }

            survey.AddQuestion(request.Text, request.Order);

            await _surveyRepository.UpdateAsync(survey, cancellationToken);

            var addedQuestion = survey.Questions.Last();

            var response = new AddQuestionResponse(
                addedQuestion.QuestionId.Value,
                addedQuestion.Text.Value,
                addedQuestion.Order
            );

            return Result.Success(response);
        }
        catch (DomainException ex)
        {
            return Result.Failure<AddQuestionResponse>(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<AddQuestionResponse>(ex.Message);
        }
    }
}
