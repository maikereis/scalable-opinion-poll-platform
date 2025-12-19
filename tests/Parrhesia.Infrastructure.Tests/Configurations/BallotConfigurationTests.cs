using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Parrhesia.Domain.Voting.Aggregates;

namespace Parrhesia.Infrastructure.Tests.Configurations;

public class BallotConfigurationTests : IntegrationTestBase
{
    [Fact]
    public void Ballot_ShouldMapToCorrectTable()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));

        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Ballots");
    }

    [Fact]
    public void Ballot_Id_ShouldBePrimaryKey()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var primaryKey = entityType!.FindPrimaryKey();

        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void Ballot_Id_ShouldNotBeValueGenerated()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var idProperty = entityType!.FindProperty("Id");

        idProperty!.ValueGenerated.Should().Be(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
    }

    [Fact]
    public void Ballot_SurveyId_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var surveyIdProperty = entityType!.FindProperty("SurveyId");

        surveyIdProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Ballot_QuestionId_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var questionIdProperty = entityType!.FindProperty("QuestionId");

        questionIdProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Ballot_SelectedOptionId_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var optionIdProperty = entityType!.FindProperty("SelectedOptionId");

        optionIdProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Ballot_VoterFingerprint_ShouldHaveMaxLength64()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var fingerprintProperty = entityType!.FindProperty("VoterFingerprint");

        fingerprintProperty!.GetMaxLength().Should().Be(64);
    }

    [Fact]
    public void Ballot_VoterFingerprint_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var fingerprintProperty = entityType!.FindProperty("VoterFingerprint");

        fingerprintProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Ballot_CastedAt_ShouldBeRequired()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var castedAtProperty = entityType!.FindProperty("CastedAt");

        castedAtProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Ballot_DeviceInfo_ShouldBeOwnedType()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var navigation = entityType!.FindNavigation("DeviceInfo");

        navigation.Should().NotBeNull();
        navigation!.TargetEntityType.IsOwned().Should().BeTrue();
    }

    [Fact]
    public void Ballot_DeviceInfo_DeviceId_ShouldMapToColumn()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var ownedType = entityType!.FindNavigation("DeviceInfo")!.TargetEntityType;
        var deviceIdProperty = ownedType.FindProperty("DeviceId");

        deviceIdProperty!.GetColumnName().Should().Be("DeviceId");
    }

    [Fact]
    public void Ballot_DeviceInfo_DeviceId_ShouldHaveMaxLength255()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var ownedType = entityType!.FindNavigation("DeviceInfo")!.TargetEntityType;
        var deviceIdProperty = ownedType.FindProperty("DeviceId");

        deviceIdProperty!.GetMaxLength().Should().Be(255);
    }

    [Fact]
    public void Ballot_DeviceInfo_UserAgent_ShouldMapToColumn()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var ownedType = entityType!.FindNavigation("DeviceInfo")!.TargetEntityType;
        var userAgentProperty = ownedType.FindProperty("UserAgent");

        userAgentProperty!.GetColumnName().Should().Be("UserAgent");
    }

    [Fact]
    public void Ballot_DeviceInfo_UserAgent_ShouldHaveMaxLength500()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var ownedType = entityType!.FindNavigation("DeviceInfo")!.TargetEntityType;
        var userAgentProperty = ownedType.FindProperty("UserAgent");

        userAgentProperty!.GetMaxLength().Should().Be(500);
    }

    [Fact]
    public void Ballot_DeviceInfo_IpHash_ShouldMapToColumn()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var ownedType = entityType!.FindNavigation("DeviceInfo")!.TargetEntityType;
        var ipHashProperty = ownedType.FindProperty("IpHash");

        ipHashProperty!.GetColumnName().Should().Be("IpHash");
    }

    [Fact]
    public void Ballot_DeviceInfo_IpHash_ShouldHaveMaxLength64()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var ownedType = entityType!.FindNavigation("DeviceInfo")!.TargetEntityType;
        var ipHashProperty = ownedType.FindProperty("IpHash");

        ipHashProperty!.GetMaxLength().Should().Be(64);
    }

    [Fact]
    public void Ballot_ShouldHaveSurveyIdIndex()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var indexes = entityType!.GetIndexes();

        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "SurveyId"));
    }

    [Fact]
    public void Ballot_ShouldHaveSelectedOptionIdIndex()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var indexes = entityType!.GetIndexes();

        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "SelectedOptionId"));
    }

    [Fact]
    public void Ballot_ShouldHaveCastedAtIndex()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var indexes = entityType!.GetIndexes();

        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "CastedAt"));
    }

    [Fact]
    public void Ballot_ShouldHaveUniqueIndexOnFingerprintAndSurveyId()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var indexes = entityType!.GetIndexes();

        var uniqueIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "VoterFingerprint") &&
            i.Properties.Any(p => p.Name == "SurveyId"));

        uniqueIndex.Should().NotBeNull();
        uniqueIndex!.IsUnique.Should().BeTrue();
    }

    [Fact]
    public void Ballot_BallotId_ShouldBeIgnored()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var ballotIdProperty = entityType!.FindProperty("BallotId");

        ballotIdProperty.Should().BeNull();
    }

    [Fact]
    public void Ballot_DomainEvents_ShouldBeIgnored()
    {
        var entityType = Context.Model.FindEntityType(typeof(Ballot));
        var domainEventsProperty = entityType!.FindProperty("DomainEvents");

        domainEventsProperty.Should().BeNull();
    }
}
