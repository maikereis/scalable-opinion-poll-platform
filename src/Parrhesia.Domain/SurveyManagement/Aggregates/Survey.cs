using Parrhesia.Domain.Common;
using Parrhesia.Domain.Common.Exceptions;
using Parrhesia.Domain.SurveyManagement.Entities;
using Parrhesia.Domain.SurveyManagement.Events;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.SurveyManagement.Aggregates;

public class Survey : AggregateRoot
{
    private readonly List<Question> _questions = [];
    private readonly List<Option> _options = [];

    public SurveyId SurveyId { get; private init; } = null!;
    public SurveyTitle Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public SurveyStatus Status { get; private set; } = null!;
    public CollectionPeriod CollectionPeriod { get; private set; } = null!;
    public IReadOnlyList<Question> Questions => _questions.AsReadOnly();
    public IReadOnlyList<Option> Options => _options.AsReadOnly();
    public DateTime CreatedAt { get; private init; }
    public DateTime UpdatedAt { get; private set; }

#pragma warning disable CS8618
    private Survey() : base() { }
#pragma warning restore CS8618

    private Survey(SurveyTitle title, string description, CollectionPeriod period)
    {
        SurveyId = SurveyId.Create(Id);
        Title = title;
        Description = description;
        Status = SurveyStatus.Draft;
        CollectionPeriod = period;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private Survey(SurveyId id, SurveyTitle title, string description,
                   SurveyStatus status, CollectionPeriod period,
                   DateTime createdAt) : base(id.Value)
    {
        SurveyId = id;
        Title = title;
        Description = description;
        Status = status;
        CollectionPeriod = period;
        CreatedAt = createdAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Survey Create(string title, string description,
                                DateTime startDate, DateTime endDate)
    {
        var surveyTitle = SurveyTitle.Create(title);
        var period = CollectionPeriod.Create(startDate, endDate);

        var survey = new Survey(surveyTitle, description, period);
        survey.AddDomainEvent(new SurveyCreatedEvent(survey.SurveyId, surveyTitle, period));

        return survey;
    }

    public static Survey Reconstitute(SurveyId id, SurveyTitle title, string description,
                                      SurveyStatus status, CollectionPeriod period,
                                      DateTime createdAt,
                                      IEnumerable<Question> questions,
                                      IEnumerable<Option> options)
    {
        var survey = new Survey(id, title, description, status, period, createdAt);
        survey._questions.AddRange(questions);
        survey._options.AddRange(options);
        return survey;
    }

    public void AddQuestion(string text, int order)
    {
        EnsureDraft();

        var questionText = QuestionText.Create(text);
        var question = Question.Create(questionText, order);
        _questions.Add(question);
        Touch();
    }

    public void AddOption(QuestionId questionId, string text, int order)
    {
        EnsureDraft();

        var question = _questions.FirstOrDefault(q => q.QuestionId == questionId)
            ?? throw new DomainException("Question not found");

        var optionText = OptionText.Create(text);
        var option = Option.Create(questionId, optionText, order);
        _options.Add(option);
        Touch();
    }

    public void RemoveQuestion(QuestionId questionId)
    {
        EnsureDraft();

        var question = _questions.FirstOrDefault(q => q.QuestionId == questionId);
        if (question == null) return;

        _questions.Remove(question);
        _options.RemoveAll(o => o.QuestionId == questionId);
        Touch();
    }

    public void RemoveOption(OptionId optionId)
    {
        EnsureDraft();

        var option = _options.FirstOrDefault(o => o.OptionId == optionId);
        if (option == null) return;

        _options.Remove(option);
        Touch();
    }

    public void UpdateTitle(string title)
    {
        EnsureDraft();

        Title = SurveyTitle.Create(title);
        Touch();
    }

    public void UpdateDescription(string description)
    {
        EnsureDraft();

        Description = description;
        Touch();
    }

    public void UpdateCollectionPeriod(DateTime startDate, DateTime endDate)
    {
        EnsureDraft();

        CollectionPeriod = CollectionPeriod.Create(startDate, endDate);
        Touch();
    }

    public void Activate()
    {
        if (Status.IsActive)
            throw new DomainException("Survey is already active");

        if (Status.IsClosed)
            throw new DomainException("Cannot activate a closed survey");

        ValidateReadyForActivation();

        Status = SurveyStatus.Active;
        Touch();

        AddDomainEvent(new SurveyActivatedEvent(SurveyId, DateTime.UtcNow));
    }

    public void Close(string reason = "")
    {
        if (Status.IsClosed)
            throw new DomainException("Survey is already closed");

        Status = SurveyStatus.Closed;
        Touch();

        AddDomainEvent(new SurveyClosedEvent(SurveyId, DateTime.UtcNow, reason));
    }

    public bool IsAcceptingVotes()
    {
        return Status.IsActive && CollectionPeriod.IsActive(DateTime.UtcNow);
    }

    public bool HasOption(QuestionId questionId, OptionId optionId)
    {
        return _options.Any(o => o.QuestionId == questionId && o.OptionId == optionId);
    }

    public IReadOnlyList<Option> GetOptionsForQuestion(QuestionId questionId)
    {
        return _options.Where(o => o.QuestionId == questionId)
                       .OrderBy(o => o.Order)
                       .ToList()
                       .AsReadOnly();
    }

    private void ValidateReadyForActivation()
    {
        if (!_questions.Any())
            throw new DomainException("Survey must have at least one question");

        foreach (var question in _questions)
        {
            var optionCount = _options.Count(o => o.QuestionId == question.QuestionId);
            if (optionCount < 2)
                throw new DomainException($"Question must have at least 2 options");
            if (optionCount > 10)
                throw new DomainException($"Question cannot have more than 10 options");
        }
    }

    private void EnsureDraft()
    {
        if (!Status.IsDraft)
            throw new DomainException("Cannot modify survey after activation");
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}