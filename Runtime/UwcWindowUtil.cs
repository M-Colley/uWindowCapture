using UnityEngine;

namespace uWindowCapture
{

public static class UwcWindowUtil
{
    public static Vector3 ConvertDesktopCoordToUnityPosition(int x, int y, int width, int height, float basePixel)
    {
        var screenMetrics = new DesktopScreenMetrics(
            Lib.GetScreenX(),
            Lib.GetScreenY(),
            Lib.GetScreenWidth(),
            Lib.GetScreenHeight());

        var windowRectangle = new DesktopWindowRectangle(x, y, width, height);
        var unityCoords = DesktopCoordinateConverter.ConvertToUnityCoordinates(windowRectangle, screenMetrics, basePixel);
        return new Vector3(unityCoords.X, unityCoords.Y, 0f);
    }

    public static Vector3 ConvertDesktopCoordToUnityPosition(UwcWindow window, float basePixel)
    {
        return ConvertDesktopCoordToUnityPosition(window.x, window.y, window.width, window.height, basePixel);
    }
}

}