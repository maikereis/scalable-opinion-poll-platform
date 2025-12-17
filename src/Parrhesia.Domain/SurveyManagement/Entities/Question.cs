using Parrhesia.Domain.Common;
using Parrhesia.Domain.SurveyManagement.ValueObjects;

namespace Parrhesia.Domain.SurveyManagement.Entities;

public class Question : Entity
{
    public QuestionId QuestionId { get; }
    public QuestionText Text { get; private set; }
    public int Order { get; private set; }

    private Question(QuestionText text, int order)
    {
        QuestionId = QuestionId.NewId();
        Text = text;
        Order = order;
    }

    private Question(QuestionId id, QuestionText text, int order) : base(id.Value)
    {
        QuestionId = id;
        Text = text;
        Order = order;
    }

    public static Question Create(QuestionText text, int order)
    {
        if (order < 0)
            throw new ArgumentException("Order must be non-negative", nameof(order));

        return new Question(text, order);
    }

    public static Question Create(QuestionId id, QuestionText text, int order)
    {
        if (order < 0)
            throw new ArgumentException("Order must be non-negative", nameof(order));

        return new Question(id, text, order);
    }

    public void UpdateText(QuestionText text)
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
