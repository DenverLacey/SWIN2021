using System;
using SplashKitSDK;
using Drawing_Program;

public class Program
{
    private enum ShapeKind
    {
        Rectangle,
        Circle,
        Line
    }

    public static void Main()
    {
        new Window("Shape Drawer", 800, 600);
        Drawing drawing = new Drawing();
        ShapeKind kindToAdd = ShapeKind.Rectangle;

        bool makingLine = false;
        MyLine line = null;

        do
        {
            SplashKit.ProcessEvents();
            drawing.Draw();

            if (SplashKit.MouseClicked(MouseButton.LeftButton))
            {
                if (makingLine && line != null)
                {
                    line.EndX = SplashKit.MouseX();
                    line.EndY = SplashKit.MouseY();

                    drawing.AddShape(line);
                    line = null;
                    makingLine = false;
                } else {
                    switch (kindToAdd)
                    {
                        case ShapeKind.Rectangle:
                            Shape newRect = new MyRectangle
                            {
                                X = SplashKit.MouseX(),
                                Y = SplashKit.MouseY(),
                                Width = 50,
                                Height = 50,
                                Color = Color.Black
                            };
                            drawing.AddShape(newRect);
                            break;

                        case ShapeKind.Circle:
                            Shape newCircle = new MyCircle
                            {
                                X = SplashKit.MouseX(),
                                Y = SplashKit.MouseY(),
                                Radius = 50,
                                Color = Color.Black
                            };
                            drawing.AddShape(newCircle);
                            break;

                        case ShapeKind.Line:
                            makingLine = true;
                            line = new MyLine
                            {
                                X = SplashKit.MouseX(),
                                Y = SplashKit.MouseY(),
                                Color = Color.Black
                            };
                            break;
                    }
                }
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

            if (SplashKit.KeyTyped(KeyCode.RKey))
            {
                kindToAdd = ShapeKind.Rectangle;
            } 
            else if (SplashKit.KeyTyped(KeyCode.CKey))
            {
                kindToAdd = ShapeKind.Circle;
            }
            else if (SplashKit.KeyTyped(KeyCode.LKey))
            {
                kindToAdd = ShapeKind.Line;
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
