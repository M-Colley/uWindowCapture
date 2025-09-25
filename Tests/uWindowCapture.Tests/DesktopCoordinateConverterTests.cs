using System;
using NUnit.Framework;

namespace uWindowCapture.Tests;

public class DesktopCoordinateConverterTests
{
    [Test]
    public void ConvertToUnityCoordinates_WindowCenteredOnScreen_ReturnsOrigin()
    {
        var window = new DesktopWindowRectangle(0, 0, 1920, 1080);
        var screen = new DesktopScreenMetrics(0, 0, 1920, 1080);

        var result = DesktopCoordinateConverter.ConvertToUnityCoordinates(window, screen, 1f);

        Assert.That(result.X, Is.EqualTo(0f));
        Assert.That(result.Y, Is.EqualTo(0f));
    }

    [Test]
    public void ConvertToUnityCoordinates_WindowOffsetOnSameScreen_ReturnsTranslatedCoordinates()
    {
        var window = new DesktopWindowRectangle(960, -540, 1920, 1080);
        var screen = new DesktopScreenMetrics(0, 0, 1920, 1080);

        var result = DesktopCoordinateConverter.ConvertToUnityCoordinates(window, screen, 1f);

        Assert.That(result.X, Is.EqualTo(960f));
        Assert.That(result.Y, Is.EqualTo(540f));
    }

    [Test]
    public void ConvertToUnityCoordinates_WindowOnSecondaryMonitor_AccountsForScreenOffset()
    {
        var window = new DesktopWindowRectangle(-1920, 0, 1920, 1080);
        var screen = new DesktopScreenMetrics(-1920, 0, 1920, 1080);

        var result = DesktopCoordinateConverter.ConvertToUnityCoordinates(window, screen, 1f);

        Assert.That(result.X, Is.EqualTo(0f));
        Assert.That(result.Y, Is.EqualTo(0f));
    }

    [Test]
    public void ConvertToUnityCoordinates_UsesBasePixelToScaleCoordinates()
    {
        var window = new DesktopWindowRectangle(0, 0, 1920, 1080);
        var screen = new DesktopScreenMetrics(0, 0, 3840, 2160);

        var result = DesktopCoordinateConverter.ConvertToUnityCoordinates(window, screen, 2f);

        Assert.That(result.X, Is.EqualTo(-480f));
        Assert.That(result.Y, Is.EqualTo(270f));
    }

    [Test]
    public void ConvertToUnityCoordinates_OddSizedWindowMatchesIntegerDivisionBehaviour()
    {
        var window = new DesktopWindowRectangle(5, 5, 3, 3);
        var screen = new DesktopScreenMetrics(0, 0, 10, 10);

        var result = DesktopCoordinateConverter.ConvertToUnityCoordinates(window, screen, 1f);

        Assert.That(result.X, Is.EqualTo(1f));
        Assert.That(result.Y, Is.EqualTo(-1f));
    }

    [Test]
    public void ConvertToUnityCoordinates_ZeroBasePixel_ThrowsArgumentOutOfRangeException()
    {
        var window = new DesktopWindowRectangle(0, 0, 100, 100);
        var screen = new DesktopScreenMetrics(0, 0, 200, 200);

        Assert.That(
            () => DesktopCoordinateConverter.ConvertToUnityCoordinates(window, screen, 0f),
            Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("basePixel"));
    }

    [Test]
    public void ConvertToUnityCoordinates_NegativeBasePixel_ThrowsArgumentOutOfRangeException()
    {
        var window = new DesktopWindowRectangle(0, 0, 100, 100);
        var screen = new DesktopScreenMetrics(0, 0, 200, 200);

        Assert.That(
            () => DesktopCoordinateConverter.ConvertToUnityCoordinates(window, screen, -1f),
            Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("basePixel"));
    }

    [Test]
    public void ConvertToUnityCoordinates_NonFiniteBasePixel_ThrowsArgumentOutOfRangeException()
    {
        var window = new DesktopWindowRectangle(0, 0, 100, 100);
        var screen = new DesktopScreenMetrics(0, 0, 200, 200);

        Assert.That(
            () => DesktopCoordinateConverter.ConvertToUnityCoordinates(window, screen, float.PositiveInfinity),
            Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("basePixel"));
    }
}
