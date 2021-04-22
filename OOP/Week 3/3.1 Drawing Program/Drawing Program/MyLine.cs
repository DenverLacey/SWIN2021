using System;
using SplashKitSDK;

namespace Drawing_Program
{
    class MyLine : Shape
    {
        float endX;
        float endY;

        public float EndX { get => endX; set => endX = value; }
        public float EndY { get => endY; set => endY = value; }

        public MyLine()
            : base()
        {
            endX = 0;
            endY = 0;
        }

        public MyLine(Color color, float startX, float startY, float endX, float endY)
            : base(color)
        {
            X = startX;
            Y = startY;
        }

        public override void Draw()
        {
            if (Selected)
                DrawOutline();
            SplashKit.DrawLine(Color, X, Y, EndX, EndY);
        }

        public override void DrawOutline()
        {
            SplashKit.DrawCircle(Color, X, Y, 10);
            SplashKit.DrawCircle(Color, EndX, EndY, 10);
        }

        public override bool IsAt(Point2D pt)
        {
            Vector2D start = new Vector2D
            {
                X = X,
                Y = Y
            };

            Vector2D end = new Vector2D
            {
                X = EndX,
                Y = EndY
            };

            Vector2D p = new Vector2D 
            { 
                X = pt.X, 
                Y = pt.Y 
            };

            double distance;
            double l2 = SplashKit.VectorMagnitudeSqared(SplashKit.VectorSubtract(end, start));
            if (l2 == 0.0)
            {
                distance = SplashKit.VectorMagnitude(SplashKit.VectorSubtract(p, start));
            }
            else
            {
                double t = Math.Max(0, Math.Min(1, SplashKit.DotProduct(SplashKit.VectorSubtract(p, start), SplashKit.VectorSubtract(end, start)) / l2));
                Vector2D proj = SplashKit.VectorAdd(start, SplashKit.VectorMultiply(SplashKit.VectorSubtract(end, start), t));
                distance = SplashKit.VectorMagnitude(SplashKit.VectorSubtract(p, proj));
            }

            return distance <= 10;
        }
    }
}
