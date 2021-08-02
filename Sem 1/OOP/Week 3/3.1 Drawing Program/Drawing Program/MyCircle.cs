using SplashKitSDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drawing_Program
{
    class MyCircle : Shape
    {
        private int radius;

        public int Radius { get => radius; set => radius = value; }

        public MyCircle()
            : base()
        {
            radius = 0;
        }

        public MyCircle(Color color, int radius)
            : base(color)
        {
            Radius = radius;
        }

        public override void Draw()
        {
            if (Selected)
                DrawOutline();
            SplashKit.FillCircle(Color, X, Y, radius);
        }

        public override void DrawOutline()
        {
            SplashKit.DrawCircle(Color, X, Y, radius + 4);
        }

        public override bool IsAt(Point2D pt)
        {
            Vector2D diff = new Vector2D
            {
                X = pt.X - X,
                Y = pt.Y - Y
            };
            return SplashKit.VectorMagnitude(diff) <= radius;
        }
    }
}
