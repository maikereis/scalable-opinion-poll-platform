using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Parrhesia.Domain.SurveyManagement.Aggregates;
using Parrhesia.Domain.SurveyManagement.Entities;
using Parrhesia.Domain.Voting.Aggregates;

namespace Parrhesia.Infrastructure.Tests.Schema;

public class DatabaseSchemaTests : IntegrationTestBase
{
    [Fact]
    public void Model_ShouldContainAllExpectedEntities()
    {
        var model = Context.Model;

        model.FindEntityType(typeof(Survey)).Should().NotBeNull();
        model.FindEntityType(typeof(Question)).Should().NotBeNull();
        model.FindEntityType(typeof(Option)).Should().NotBeNull();
        model.FindEntityType(typeof(Ballot)).Should().NotBeNull();
    }

    [Fact]
    public void Surveys_Table_ShouldHaveExpectedColumns()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey))!;

        // Get all properties including from owned types
        var allProperties = new List<string>();

        // Direct properties
        allProperties.AddRange(entityType.GetProperties().Select(p => p.GetColumnName()));

        // Owned type properties (CollectionPeriod)
        var collectionPeriodNav = entityType.FindNavigation("CollectionPeriod");
        if (collectionPeriodNav != null)
        {
            allProperties.AddRange(
                collectionPeriodNav.TargetEntityType.GetProperties()
                    .Select(p => p.GetColumnName()));
        }

        allProperties.Should().Contain("Id");
        allProperties.Should().Contain("Title");
        allProperties.Should().Contain("Description");
        allProperties.Should().Contain("Status");
        allProperties.Should().Contain("StartDate");
        allProperties.Should().Contain("EndDate");
        allProperties.Should().Contain("CreatedAt");
        allProperties.Should().Contain("UpdatedAt");
    }

    [Fact]
    public void Questions_Table_ShouldHaveExpectedColumns()
    {
        var entityType = Context.Model.FindEntityType(typeof(Question))!;
        var properties = entityType.GetProperties().Select(p => p.GetColumnName()).ToList();

        properties.Should().Contain("Id");
        properties.Should().Contain("SurveyId");
        properties.Should().Contain("Text");
        properties.Should().Contain("Order");
    }

    [Fact]
    public void Options_Table_ShouldHaveExpectedColumns()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option))!;
        var properties = entityType.GetProperties().Select(p => p.GetColumnName()).ToList();

        properties.Should().Contain("Id");
        properties.Should().Contain("SurveyId");
        properties.Should().Contain("QuestionId");
        properties.Should().Contain("Text");
        properties.Should().Contain("Order");
    }

    [Fact]
    public void Ballots_Table_ShouldHaveExpectedColumns()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot))!;
        var properties = entityType.GetProperties().Select(p => p.GetColumnName()).ToList();

        properties.Should().Contain("Id");
        properties.Should().Contain("SurveyId");
        properties.Should().Contain("QuestionId");
        properties.Should().Contain("SelectedOptionId");
        properties.Should().Contain("VoterFingerprint");
        properties.Should().Contain("CastedAt");
    }

    [Fact]
    public void Ballots_Table_ShouldHaveDeviceInfoColumns()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot))!;
        var ownedType = entityType.FindNavigation("DeviceInfo")!.TargetEntityType;
        var properties = ownedType.GetProperties().Select(p => p.GetColumnName()).ToList();

        properties.Should().Contain("DeviceId");
        properties.Should().Contain("UserAgent");
        properties.Should().Contain("IpHash");
    }

    [Fact]
    public void Model_ShouldNotHaveUnexpectedTables()
    {
        var entityTypes = Context.Model.GetEntityTypes()
            .Where(e => !e.IsOwned())
            .Select(e => e.GetTableName())
            .Distinct()
            .ToList();

        entityTypes.Should().HaveCount(4);
        entityTypes.Should().Contain("Surveys");
        entityTypes.Should().Contain("Questions");
        entityTypes.Should().Contain("Options");
        entityTypes.Should().Contain("Ballots");
    }

    [Fact]
    public void Surveys_ShouldHaveExpectedIndexCount()
    {
        var entityType = Context.Model.FindEntityType(typeof(Survey))!;
        var indexes = entityType.GetIndexes().ToList();

        // Status index
        indexes.Should().HaveCount(1);
    }

    [Fact]
    public void Questions_ShouldHaveExpectedIndexCount()
    {
        var entityType = Context.Model.FindEntityType(typeof(Question))!;
        var indexes = entityType.GetIndexes().ToList();

        // SurveyId index
        indexes.Should().HaveCount(1);
    }

    [Fact]
    public void Options_ShouldHaveExpectedIndexCount()
    {
        var entityType = Context.Model.FindEntityType(typeof(Option))!;
        var indexes = entityType.GetIndexes().ToList();

        // SurveyId + QuestionId indexes
        indexes.Should().HaveCount(2);
    }

    [Fact]
    public void Ballots_ShouldHaveExpectedIndexCount()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot))!;
        var indexes = entityType.GetIndexes().ToList();

        // SurveyId, SelectedOptionId, CastedAt, VoterFingerprint+SurveyId (unique)
        indexes.Should().HaveCount(4);
    }

    [Fact]
    public void AllPrimaryKeys_ShouldBeGuid()
    {
        var entityTypes = Context.Model.GetEntityTypes().Where(e => !e.IsOwned());

        foreach (var entityType in entityTypes)
        {
            var primaryKey = entityType.FindPrimaryKey();
            primaryKey.Should().NotBeNull($"{entityType.Name} should have a primary key");

            var keyProperty = primaryKey!.Properties.Single();
            keyProperty.ClrType.Should().Be(typeof(Guid), $"{entityType.Name} primary key should be Guid");
        }
    }

    [Fact]
    public void AllPrimaryKeys_ShouldNotBeAutoGenerated()
    {
        var entityTypes = Context.Model.GetEntityTypes().Where(e => !e.IsOwned());

        foreach (var entityType in entityTypes)
        {
            var primaryKey = entityType.FindPrimaryKey();
            var keyProperty = primaryKey!.Properties.Single();

            keyProperty.ValueGenerated.Should().Be(
                Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never,
                $"{entityType.Name} primary key should not be auto-generated");
        }
    }
}
