using FluentAssertions;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Tests.Voting.ValueObjects;

public class DeviceInfoTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        var deviceInfo = DeviceInfo.Create("device-123", "Mozilla/5.0", "192.168.1.1");

        deviceInfo.DeviceId.Should().Be("device-123");
        deviceInfo.UserAgent.Should().Be("Mozilla/5.0");
        deviceInfo.IpHash.Should().NotBeEmpty();
        deviceInfo.IpHash.Should().HaveLength(64);
    }

    [Fact]
    public void Create_WithNullUserAgent_ShouldUseDefault()
    {
        var deviceInfo = DeviceInfo.Create("device-123", null!, "192.168.1.1");

        deviceInfo.UserAgent.Should().Be("Unknown");
    }

    [Fact]
    public void Create_WithNullIp_ShouldUseUnknown()
    {
        var deviceInfo = DeviceInfo.Create("device-123", "Mozilla", null!);

        deviceInfo.IpHash.Should().Be("Unknown");
    }

    [Fact]
    public void Create_WithEmptyDeviceId_ShouldThrow()
    {
        Action act = () => DeviceInfo.Create("", "Mozilla", "192.168.1.1");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*DeviceId is required*");
    }

    [Fact]
    public void Create_SameIp_ShouldProduceSameHash()
    {
        var info1 = DeviceInfo.Create("dev1", "Mozilla", "192.168.1.1");
        var info2 = DeviceInfo.Create("dev2", "Chrome", "192.168.1.1");

        info1.IpHash.Should().Be(info2.IpHash);
    }

    [Fact]
    public void TwoDeviceInfos_WithSameValues_ShouldBeEqual()
    {
        var info1 = DeviceInfo.Create("device-123", "Mozilla", "192.168.1.1");
        var info2 = DeviceInfo.Create("device-123", "Mozilla", "192.168.1.1");

        info1.Should().Be(info2);
    }
}
