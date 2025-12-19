using Microsoft.EntityFrameworkCore;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.Entities;
using Parrhesia.Domain.SurveyManagement.Repositories;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Infrastructure.Persistence.Repositories;

public class SurveyRepository : ISurveyRepository
{
    private readonly ParrhesiaDbContext _context;

    public SurveyRepository(ParrhesiaDbContext context)
    {
        _context = context;
    }

    public async Task<Survey?> GetByIdAsync(SurveyId id, CancellationToken cancellationToken = default)
    {
        var surveyId = id.Value;

        var survey = await _context.Surveys
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == surveyId, cancellationToken);

        if (survey == null)
            return null;

        var questionsWithSurveyId = await _context.Questions
            .AsNoTracking()
            .Where(q => EF.Property<Guid>(q, "SurveyId") == surveyId)
            .Select(q => new { Question = q, SurveyId = EF.Property<Guid>(q, "SurveyId") })
            .OrderBy(x => x.Question.Order)
            .ToListAsync(cancellationToken);

        var questions = questionsWithSurveyId.Select(x => x.Question).ToList();

        var optionsWithSurveyId = await _context.Options
            .AsNoTracking()
            .Where(o => EF.Property<Guid>(o, "SurveyId") == surveyId)
            .Select(o => new { Option = o, SurveyId = EF.Property<Guid>(o, "SurveyId") })
            .OrderBy(x => x.Option.Order)
            .ToListAsync(cancellationToken);

        var options = optionsWithSurveyId.Select(x => x.Option).ToList();

        return ReconstituteSurvey(survey, questions, options);
    }

    public async Task<IReadOnlyList<Survey>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var surveyIds = await _context.Surveys
            .AsNoTracking()
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (surveyIds.Count == 0)
            return Array.Empty<Survey>();

        return await LoadSurveysWithDetails(surveyIds, cancellationToken);
    }

    public async Task<IReadOnlyList<Survey>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var activeSurveyIds = await _context.Surveys
            .AsNoTracking()
            .Where(s => s.Status == SurveyStatus.Active)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (activeSurveyIds.Count == 0)
            return Array.Empty<Survey>();

        return await LoadSurveysWithDetails(activeSurveyIds, cancellationToken);
    }

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .CountAsync(s => s.Status == SurveyStatus.Active, cancellationToken);
    }

    public async Task AddAsync(Survey survey, CancellationToken cancellationToken = default)
    {
        _context.Surveys.Add(survey);

        foreach (var question in survey.Questions)
        {
            _context.Entry(question).Property("SurveyId").CurrentValue = survey.Id;
            _context.Questions.Add(question);
        }

        foreach (var option in survey.Options)
        {
            _context.Entry(option).Property("SurveyId").CurrentValue = survey.Id;
            _context.Options.Add(option);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Survey survey, CancellationToken cancellationToken = default)
    {
        var surveyId = survey.Id;

        var existingQuestionIds = await _context.Questions
            .Where(q => EF.Property<Guid>(q, "SurveyId") == surveyId)
            .Select(q => q.Id)
            .ToListAsync(cancellationToken);

        var existingOptionIds = await _context.Options
            .Where(o => EF.Property<Guid>(o, "SurveyId") == surveyId)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        var currentQuestionIds = survey.Questions.Select(q => q.Id).ToHashSet();
        var currentOptionIds = survey.Options.Select(o => o.Id).ToHashSet();

        var questionsToRemove = existingQuestionIds.Where(id => !currentQuestionIds.Contains(id)).ToList();
        if (questionsToRemove.Count > 0)
        {
            var questionsToDelete = await _context.Questions
                .Where(q => questionsToRemove.Contains(q.Id))
                .ToListAsync(cancellationToken);
            _context.Questions.RemoveRange(questionsToDelete);
        }

        var optionsToRemove = existingOptionIds.Where(id => !currentOptionIds.Contains(id)).ToList();
        if (optionsToRemove.Count > 0)
        {
            var optionsToDelete = await _context.Options
                .Where(o => optionsToRemove.Contains(o.Id))
                .ToListAsync(cancellationToken);
            _context.Options.RemoveRange(optionsToDelete);
        }

        var newQuestions = survey.Questions.Where(q => !existingQuestionIds.Contains(q.Id));
        foreach (var question in newQuestions)
        {
            _context.Entry(question).Property("SurveyId").CurrentValue = surveyId;
            _context.Questions.Add(question);
        }

        var newOptions = survey.Options.Where(o => !existingOptionIds.Contains(o.Id));
        foreach (var option in newOptions)
        {
            _context.Entry(option).Property("SurveyId").CurrentValue = surveyId;
            _context.Options.Add(option);
        }

        var existingQuestionsToUpdate = survey.Questions.Where(q => existingQuestionIds.Contains(q.Id));
        foreach (var question in existingQuestionsToUpdate)
        {
            _context.Entry(question).Property("SurveyId").CurrentValue = surveyId;
            _context.Questions.Update(question);
        }

        var existingOptionsToUpdate = survey.Options.Where(o => existingOptionIds.Contains(o.Id));
        foreach (var option in existingOptionsToUpdate)
        {
            _context.Entry(option).Property("SurveyId").CurrentValue = surveyId;
            _context.Options.Update(option);
        }

        _context.Surveys.Update(survey);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(SurveyId id, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .AnyAsync(s => s.Id == id.Value, cancellationToken);
    }

    private async Task<IReadOnlyList<Survey>> LoadSurveysWithDetails(
        List<Guid> surveyIds, 
        CancellationToken cancellationToken)
    {
        var surveys = await _context.Surveys
            .AsNoTracking()
            .Where(s => surveyIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var questionsWithSurveyId = await _context.Questions
            .AsNoTracking()
            .Where(q => surveyIds.Contains(EF.Property<Guid>(q, "SurveyId")))
            .Select(q => new { Question = q, SurveyId = EF.Property<Guid>(q, "SurveyId") })
            .OrderBy(x => x.Question.Order)
            .ToListAsync(cancellationToken);

        var optionsWithSurveyId = await _context.Options
            .AsNoTracking()
            .Where(o => surveyIds.Contains(EF.Property<Guid>(o, "SurveyId")))
            .Select(o => new { Option = o, SurveyId = EF.Property<Guid>(o, "SurveyId") })
            .OrderBy(x => x.Option.Order)
            .ToListAsync(cancellationToken);

        var questionsBySurvey = questionsWithSurveyId
            .GroupBy(x => x.SurveyId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Question).ToList());

        var optionsBySurvey = optionsWithSurveyId
            .GroupBy(x => x.SurveyId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Option).ToList());

        var result = surveys.Select(survey =>
        {
            var questions = questionsBySurvey.GetValueOrDefault(survey.Id, []);
            var options = optionsBySurvey.GetValueOrDefault(survey.Id, []);
            return ReconstituteSurvey(survey, questions, options);
        }).ToList();

        return result.AsReadOnly();
    }

    private static Survey ReconstituteSurvey(Survey survey, List<Question> questions, List<Option> options)
    {
        var reconstructedQuestions = questions.Select(q =>
            Question.Create(
                QuestionId.Reconstitute(q.Id),
                q.Text,
                q.Order)).ToList();

        var reconstructedOptions = options.Select(o =>
            Option.Create(
                OptionId.Reconstitute(o.Id),
                o.QuestionId,
                o.Text,
                o.Order)).ToList();

        return Survey.Reconstitute(
            SurveyId.Create(survey.Id),
            survey.Title,
            survey.Description,
            survey.Status,
            survey.CollectionPeriod,
            survey.CreatedAt,
            reconstructedQuestions,
            reconstructedOptions);
    }
}