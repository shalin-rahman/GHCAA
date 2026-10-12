using System;
using System.Text.Json;
using static GHCAA.Domain.Constants;

namespace GHCAA.Infrastructure.Services;

// 37.1r. A small substring parser, not a full user agent database. It names the device class, the
// OS and the browser well enough for an audit row. An unknown agent gives "unknown" in each field.
public static class UserAgentParser
{
    public sealed record Result(string Device, string Os, string Browser);

    public static Result Parse(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return new Result(UserAgentDevices.Unknown, UserAgentDevices.Unknown, UserAgentDevices.Unknown);

        var ua = userAgent;
        var os = Has(ua, "iPhone") || Has(ua, "iPad") || Has(ua, "iPod") ? UserAgentDevices.IOs
            : Has(ua, "Android") ? UserAgentDevices.Android
            : Has(ua, "Windows") ? UserAgentDevices.Windows
            : Has(ua, "Mac OS X") || Has(ua, "Macintosh") ? UserAgentDevices.MacOs
            : Has(ua, "CrOS") ? UserAgentDevices.ChromeOs
            : Has(ua, "Linux") ? UserAgentDevices.Linux
            : Has(ua, "Dart/") ? UserAgentDevices.Flutter
            : UserAgentDevices.Unknown;

        // Edge, Opera and Samsung Internet all carry "Chrome", and Chrome carries "Safari". Test in that order.
        var browser = Has(ua, "Edg/") || Has(ua, "EdgA/") || Has(ua, "EdgiOS/") ? UserAgentDevices.Edge
            : Has(ua, "OPR/") || Has(ua, "Opera") ? UserAgentDevices.Opera
            : Has(ua, "SamsungBrowser/") ? UserAgentDevices.SamsungInternet
            : Has(ua, "Firefox/") || Has(ua, "FxiOS/") ? UserAgentDevices.Firefox
            : Has(ua, "Chrome/") || Has(ua, "CriOS/") ? UserAgentDevices.Chrome
            : Has(ua, "Safari/") ? UserAgentDevices.Safari
            : Has(ua, "Dart/") ? UserAgentDevices.FlutterApp
            : UserAgentDevices.Unknown;

        var device = Has(ua, "iPad") || (Has(ua, "Android") && !Has(ua, "Mobile")) || Has(ua, "Tablet") ? UserAgentDevices.Tablet
            : Has(ua, "Mobile") || Has(ua, "iPhone") || Has(ua, "iPod") || Has(ua, "Dart/") ? UserAgentDevices.Mobile
            : os == UserAgentDevices.Unknown ? UserAgentDevices.Unknown
            : UserAgentDevices.Desktop;

        return new Result(device, os, browser);
    }

    // The JSON written into ActivityLog.Metadata.
    public static string ToMetadata(string? userAgent)
    {
        var r = Parse(userAgent);
        return JsonSerializer.Serialize(new { device = r.Device, os = r.Os, browser = r.Browser });
    }

    private static bool Has(string ua, string token) => ua.Contains(token, StringComparison.OrdinalIgnoreCase);
}
