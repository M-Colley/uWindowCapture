using System;

namespace uWindowCapture
{
    internal readonly struct DesktopScreenMetrics
    {
        public DesktopScreenMetrics(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public int CenterX => X + Width / 2;
        public int CenterY => Y + Height / 2;
    }

    internal readonly struct DesktopWindowRectangle
    {
        public DesktopWindowRectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public int CenterX => X + Width / 2;
        public int CenterY => Y + Height / 2;
    }

    internal readonly struct UnityDesktopCoordinates
    {
        public UnityDesktopCoordinates(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }
        public float Y { get; }
    }

    internal static class DesktopCoordinateConverter
    {
        public static UnityDesktopCoordinates ConvertToUnityCoordinates(
            DesktopWindowRectangle window,
            DesktopScreenMetrics screen,
            float basePixel)
        {
            if (!float.IsFinite(basePixel))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(basePixel),
                    basePixel,
                    "basePixel must be a finite, positive value.");
            }

            if (basePixel <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(basePixel),
                    basePixel,
                    "basePixel must be a finite, positive value.");
            }

            var unityX = (window.CenterX - screen.CenterX) / basePixel;
            var unityY = (-window.CenterY + screen.CenterY) / basePixel;
            return new UnityDesktopCoordinates(unityX, unityY);
        }
    }
}
