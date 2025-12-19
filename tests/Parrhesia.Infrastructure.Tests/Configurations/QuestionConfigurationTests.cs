using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Parrhesia.Domain.SurveyManagement.Entities;

namespace Parrhesia.Infrastructure.Tests.Configurations;

public class QuestionConfigurationTests : IntegrationTestBase
{
    [Fact]
    public void Question_ShouldMapToCorrectTable()
    {
        var entityType = Context.Model.FindEntityType(typeof(Question));

        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Questions");
    }

    [Fact]
    public void Question_Id_ShouldBePrimaryKey()
    {
        var entityType = Context.Model.FindEntityType(typeof(Question));
        var primaryKey = entityType!.FindPrimaryKey();

        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void Question_Id_ShouldNotBeValueGenerated()
    {
        var entityType = Context.Model.FindEntityType(typeof(Question));
        var idProperty = entityType!.FindProperty("Id");

        idProperty!.ValueGenerated.Should().Be(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
    }

    [Fact]
    public void Question_Text_ShouldHaveMaxLength500()
    {
        var entityType = Context.Model.FindEntityType(typeof(Question));
        var textProperty = entityType!.FindProperty("Text");

        textProperty!.GetMaxLength().Should().Be(500);
    }

    [Fact]
    public void Question_Text_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Question));
        var textProperty = entityType!.FindProperty("Text");

        textProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Question_Order_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Question));
        var orderProperty = entityType!.FindProperty("Order");

        orderProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Question_ShouldHaveSurveyIdShadowProperty()
    {
        var entityType = Context.Model.FindEntityType(typeof(Question));
        var surveyIdProperty = entityType!.FindProperty("SurveyId");

        surveyIdProperty.Should().NotBeNull();
        surveyIdProperty!.IsShadowProperty().Should().BeTrue();
    }

    [Fact]
    public void Question_SurveyId_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Question));
        var surveyIdProperty = entityType!.FindProperty("SurveyId");

        surveyIdProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Question_ShouldHaveSurveyIdIndex()
    {
        var entityType = Context.Model.FindEntityType(typeof(Question));
        var indexes = entityType!.GetIndexes();

        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "SurveyId"));
    }

    [Fact]
    public void Question_QuestionId_ShouldBeIgnored()
    {
        var entityType = Context.Model.FindEntityType(typeof(Question));
        var questionIdProperty = entityType!.FindProperty("QuestionId");

        questionIdProperty.Should().BeNull();
    }
}
