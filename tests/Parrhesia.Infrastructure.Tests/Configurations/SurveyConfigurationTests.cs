using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Parrhesia.Domain.SurveyManagement.Aggregates;

namespace Parrhesia.Infrastructure.Tests.Configurations;

public class SurveyConfigurationTests : IntegrationTestBase
{
    [Fact]
    public void Survey_ShouldMapToCorrectTable()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));

        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Surveys");
    }

    [Fact]
    public void Survey_Id_ShouldBePrimaryKey()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var primaryKey = entityType!.FindPrimaryKey();

        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void Survey_Id_ShouldNotBeValueGenerated()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var idProperty = entityType!.FindProperty("Id");

        idProperty!.ValueGenerated.Should().Be(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
    }

    [Fact]
    public void Survey_Title_ShouldHaveMaxLength500()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var titleProperty = entityType!.FindProperty("Title");

        titleProperty!.GetMaxLength().Should().Be(500);
    }

    [Fact]
    public void Survey_Title_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var titleProperty = entityType!.FindProperty("Title");

        titleProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Survey_Description_ShouldHaveMaxLength2000()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var descProperty = entityType!.FindProperty("Description");

        descProperty!.GetMaxLength().Should().Be(2000);
    }

    [Fact]
    public void Survey_Status_ShouldHaveMaxLength20()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var statusProperty = entityType!.FindProperty("Status");

        statusProperty!.GetMaxLength().Should().Be(20);
    }

    [Fact]
    public void Survey_Status_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var statusProperty = entityType!.FindProperty("Status");

        statusProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Survey_CollectionPeriod_ShouldBeOwnedType()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var navigation = entityType!.FindNavigation("CollectionPeriod");

        navigation.Should().NotBeNull();
        navigation!.TargetEntityType.IsOwned().Should().BeTrue();
    }

    [Fact]
    public void Survey_CollectionPeriod_StartDate_ShouldMapToColumn()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var ownedType = entityType!.FindNavigation("CollectionPeriod")!.TargetEntityType;
        var startDateProperty = ownedType.FindProperty("StartDate");

        startDateProperty!.GetColumnName().Should().Be("StartDate");
    }

    [Fact]
    public void Survey_CollectionPeriod_EndDate_ShouldMapToColumn()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var ownedType = entityType!.FindNavigation("CollectionPeriod")!.TargetEntityType;
        var endDateProperty = ownedType.FindProperty("EndDate");

        endDateProperty!.GetColumnName().Should().Be("EndDate");
    }

    [Fact]
    public void Survey_CreatedAt_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var createdAtProperty = entityType!.FindProperty("CreatedAt");

        createdAtProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Survey_UpdatedAt_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var updatedAtProperty = entityType!.FindProperty("UpdatedAt");

        updatedAtProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Survey_ShouldHaveStatusIndex()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var indexes = entityType!.GetIndexes();

        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "Status"));
    }

    [Fact]
    public void Survey_DomainEvents_ShouldBeIgnored()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var domainEventsProperty = entityType!.FindProperty("DomainEvents");

        domainEventsProperty.Should().BeNull();
    }

    [Fact]
    public void Survey_SurveyId_ShouldBeIgnored()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var surveyIdProperty = entityType!.FindProperty("SurveyId");

        surveyIdProperty.Should().BeNull();
    }

    [Fact]
    public void Survey_Questions_ShouldBeIgnored()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var questionsNavigation = entityType!.FindNavigation("Questions");

        questionsNavigation.Should().BeNull();
    }

    [Fact]
    public void Survey_Options_ShouldBeIgnored()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey));
        var optionsNavigation = entityType!.FindNavigation("Options");

        optionsNavigation.Should().BeNull();
    }
}
