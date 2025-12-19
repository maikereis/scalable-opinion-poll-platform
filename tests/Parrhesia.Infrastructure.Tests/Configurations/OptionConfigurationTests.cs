using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Parrhesia.Domain.SurveyManagement.Entities;

namespace Parrhesia.Infrastructure.Tests.Configurations;

public class OptionConfigurationTests : IntegrationTestBase
{
    [Fact]
    public void Option_ShouldMapToCorrectTable()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option));

        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Options");
    }

    [Fact]
    public void Option_Id_ShouldBePrimaryKey()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option));
        var primaryKey = entityType!.FindPrimaryKey();

        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void Option_Id_ShouldNotBeValueGenerated()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option));
        var idProperty = entityType!.FindProperty("Id");

        idProperty!.ValueGenerated.Should().Be(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
    }

    [Fact]
    public void Option_Text_ShouldHaveMaxLength200()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option));
        var textProperty = entityType!.FindProperty("Text");

        textProperty!.GetMaxLength().Should().Be(200);
    }

    [Fact]
    public void Option_Text_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option));
        var textProperty = entityType!.FindProperty("Text");

        textProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Option_QuestionId_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option));
        var questionIdProperty = entityType!.FindProperty("QuestionId");

        questionIdProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Option_Order_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option));
        var orderProperty = entityType!.FindProperty("Order");

        orderProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Option_ShouldHaveSurveyIdShadowProperty()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option));
        var surveyIdProperty = entityType!.FindProperty("SurveyId");

        surveyIdProperty.Should().NotBeNull();
        surveyIdProperty!.IsShadowProperty().Should().BeTrue();
    }

    [Fact]
    public void Option_SurveyId_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option));
        var surveyIdProperty = entityType!.FindProperty("SurveyId");

        surveyIdProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Option_ShouldHaveSurveyIdIndex()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option));
        var indexes = entityType!.GetIndexes();

        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "SurveyId"));
    }

    [Fact]
    public void Option_ShouldHaveQuestionIdIndex()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option));
        var indexes = entityType!.GetIndexes();

        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "QuestionId"));
    }

    [Fact]
    public void Option_OptionId_ShouldBeIgnored()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option));
        var optionIdProperty = entityType!.FindProperty("OptionId");

        optionIdProperty.Should().BeNull();
    }
}
