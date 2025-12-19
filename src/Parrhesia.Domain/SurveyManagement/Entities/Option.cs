using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.SurveyManagement.Entities;

public class Option : Entity
{
    public OptionId OptionId { get; private init; } = null!;
    public QuestionId QuestionId { get; private init; } = null!;
    public OptionText Text { get; private set; } = null!;
    public int Order { get; private set; }

#pragma warning disable CS8618
    private Option() : base() { }
#pragma warning restore CS8618

    private Option(QuestionId questionId, OptionText text, int order)
    {
        OptionId = OptionId.Create(Id);
        QuestionId = questionId;
        Text = text;
        Order = order;
    }

    private Option(OptionId id, QuestionId questionId, OptionText text, int order) : base(id.Value)
    {
        OptionId = id;
        QuestionId = questionId;
        Text = text;
        Order = order;
    }

    public static Option Create(QuestionId questionId, OptionText text, int order)
    {
        if (order < 0)
            throw new ArgumentException("Order must be non-negative", nameof(order));

        return new Option(questionId, text, order);
    }

    public static Option Create(OptionId id, QuestionId questionId, OptionText text, int order)
    {
        if (order < 0)
            throw new ArgumentException("Order must be non-negative", nameof(order));

        return new Option(id, questionId, text, order);
    }

    public void UpdateText(OptionText text)
    {
        Text = text;
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order must be non-negative", nameof(order));

        Order = order;
    }
}