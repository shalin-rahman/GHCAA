using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Infrastructure.Services;
using NUnit.Framework;

namespace GHCAA.Tests.Services;

[TestFixture]
[Category("FR-39")]
public sealed class UserAgentParserTests
{
    [Test]
    public void Parse_IPhoneSafari_IsMobileIosSafari()
    {
        var r = UserAgentParser.Parse("Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1");

        r.Device.Should().Be(Constants.UserAgentDevices.Mobile);
        r.Os.Should().Be(Constants.UserAgentDevices.IOs);
        r.Browser.Should().Be(Constants.UserAgentDevices.Safari);
    }

    [Test]
    public void Parse_AndroidChrome_IsMobileAndroidChrome()
    {
        var r = UserAgentParser.Parse("Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Mobile Safari/537.36");

        r.Device.Should().Be(Constants.UserAgentDevices.Mobile);
        r.Os.Should().Be(Constants.UserAgentDevices.Android);
        r.Browser.Should().Be(Constants.UserAgentDevices.Chrome);
    }

    [Test]
    public void Parse_AndroidWithoutMobile_IsTablet()
    {
        UserAgentParser.Parse("Mozilla/5.0 (Linux; Android 13; SM-X700) AppleWebKit/537.36 Chrome/120.0 Safari/537.36")
            .Device.Should().Be(Constants.UserAgentDevices.Tablet);
    }

    [Test]
    public void Parse_WindowsEdge_IsDesktopEdge()
    {
        var r = UserAgentParser.Parse("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36 Edg/120.0");

        r.Device.Should().Be(Constants.UserAgentDevices.Desktop);
        r.Os.Should().Be(Constants.UserAgentDevices.Windows);
        r.Browser.Should().Be(Constants.UserAgentDevices.Edge);
    }

    [Test]
    public void Parse_FlutterApp_IsMobileApp()
    {
        var r = UserAgentParser.Parse("Dart/3.5 (dart:io)");

        r.Device.Should().Be(Constants.UserAgentDevices.Mobile);
        r.Browser.Should().Be(Constants.UserAgentDevices.FlutterApp);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Parse_EmptyInput_IsAllUnknown(string? agent)
    {
        var r = UserAgentParser.Parse(agent);

        r.Should().Be(new UserAgentParser.Result(Constants.UserAgentDevices.Unknown, Constants.UserAgentDevices.Unknown, Constants.UserAgentDevices.Unknown));
    }

    [Test]
    public void ToMetadata_HasDeviceOsBrowserAndNoTime()
    {
        var json = UserAgentParser.ToMetadata("Mozilla/5.0 (Windows NT 10.0) Chrome/120.0 Safari/537.36");

        json.Should().Contain("\"device\"").And.Contain("\"os\"").And.Contain("\"browser\"");
        json.Should().NotContain("time");
    }
}
