using System;
using SplashKitSDK;

namespace Drawing_Program 
{
    class MyRectangle : Shape
    {
        private int width;
        private int height;

        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }

        public MyRectangle()
            : base()
        {
            width = 0;
            height = 0;
        }

        public MyRectangle(Color color, float x, float y, int width, int height)
            : base(color)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override void Draw()
        {
            SplashKit.FillRectangle(Color, X, Y, width, height);

            if (Selected)
            {
                DrawOutline();
            }
        }

        public override void DrawOutline()
        {
            SplashKit.DrawRectangle(Color.Black, X - 2, Y - 2, width + 4, height + 4);
        }

        public override bool IsAt(Point2D pt)
        {
            return pt.X >= X && pt.X <= X + width &&
                pt.Y >= Y && pt.Y <= Y + height;
        }
    }
}
