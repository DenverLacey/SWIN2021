using System;
using SplashKitSDK;
using Drawing_Program;

public class Program
{
    public static void Main()
    {
        new Window("Shape Drawer", 800, 600);
        Drawing drawing = new Drawing();

        do
        {
            SplashKit.ProcessEvents();
            drawing.Draw();

            if (SplashKit.MouseClicked(MouseButton.LeftButton))
            {
                Shape s = new Shape
                {
                    X = SplashKit.MouseX(),
                    Y = SplashKit.MouseY(),
                    Width = 10,
                    Height = 10,
                    Color = Color.Black
                };
                drawing.AddShape(s);
            }

            if (SplashKit.MouseClicked(MouseButton.RightButton))
            {
                drawing.SelectShapesAt(SplashKit.MousePosition());
            }

            if (SplashKit.KeyTyped(KeyCode.DeleteKey) ||
                SplashKit.KeyTyped(KeyCode.BackspaceKey))
            {
                foreach (Shape s in drawing.SelectedShapes)
                {
                    drawing.RemoveShape(s);
                }
            }

            if (SplashKit.KeyTyped(KeyCode.SpaceKey))
            {
                Color bg = SplashKit.RandomRGBColor(255);
                drawing.Background = bg;
            }

            if (SplashKit.KeyTyped(KeyCode.EscapeKey))
            {
                SplashKit.CloseAllWindows();
            }

            SplashKit.RefreshScreen();

        } while (!SplashKit.WindowCloseRequested("Shape Drawer"));
    }
}
