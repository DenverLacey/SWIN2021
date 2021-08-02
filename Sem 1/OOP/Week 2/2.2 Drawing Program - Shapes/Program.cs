using System;
using SplashKitSDK;

public class Program
{
    public static void Main()
    {
        new Window("Shape Drawer", 800, 600);
        var shape = new Shape(Color.Red, 300, 200, 100, 100);

        do
        {
            SplashKit.ProcessEvents();
            SplashKit.ClearScreen();

            shape.Draw();

            if (SplashKit.MouseClicked(MouseButton.LeftButton))
            {
                shape.X = SplashKit.MouseX();
                shape.Y = SplashKit.MouseY();
            }

            if (SplashKit.KeyTyped(KeyCode.SpaceKey) && shape.IsAt(SplashKit.MousePosition()))
            {
                Color newColor = SplashKit.RandomRGBColor(255);
                shape.Color = newColor;
            }

            if (SplashKit.KeyTyped(KeyCode.EscapeKey))
            {
                SplashKit.CloseAllWindows();
            }

            SplashKit.RefreshScreen();

        } while (!SplashKit.WindowCloseRequested("Shape Drawer"));
    }
}
