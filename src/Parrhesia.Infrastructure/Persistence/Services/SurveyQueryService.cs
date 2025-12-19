using Microsoft.EntityFrameworkCore;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Services;

namespace Parrhesia.Infrastructure.Persistence.Services;

public class SurveyQueryService : ISurveyQueryService
{
    private readonly ParrhesiaDbContext _context;

    public SurveyQueryService(ParrhesiaDbContext context)
    {
        _context = context;
    }

    public async Task<SurveyStatusDto?> GetSurveyStatusAsync(
        SurveyId surveyId, 
        CancellationToken cancellationToken = default)
    {
        var survey = await _context.Surveys
            .FirstOrDefaultAsync(s => s.Id == surveyId.Value, cancellationToken);

        if (survey == null)
            return null;

        var options = await _context.Options
            .Where(o => EF.Property<Guid>(o, "SurveyId") == surveyId.Value)
            .ToListAsync(cancellationToken);

        var questionsWithOptions = options
            .GroupBy(o => o.QuestionId.Value) 
            .ToDictionary(
                g => QuestionId.Reconstitute(g.Key),
                g => g.Select(o => OptionId.Reconstitute(o.Id)) // Use o .Id (Guid) para criar o OptionId
                    .ToList());

        return new SurveyStatusDto
        {
            SurveyId = surveyId,
            IsActive = survey.Status.IsActive && survey.CollectionPeriod.IsActive(DateTime.UtcNow),
            StartDate = survey.CollectionPeriod.StartDate,
            EndDate = survey.CollectionPeriod.EndDate,
            QuestionsWithOptions = questionsWithOptions
        };
    }
}