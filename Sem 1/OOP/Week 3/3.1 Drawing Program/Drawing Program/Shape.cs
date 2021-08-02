using System;
using SplashKitSDK;

namespace Drawing_Program
{
    abstract class Shape
    {
        private Color color;
        private float x;
        private float y;
        private bool selected;

        public Color Color { get => color; set => color = value; }
        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
        public bool Selected { get => selected; set => selected = value; }

        public Shape()
        {
            x = 0;
            y = 0;
            selected = false;
        }

        public Shape(Color color)
            : this()
        {
            this.color = color;
        }

        public abstract void Draw();
        public abstract void DrawOutline();
        public abstract bool IsAt(Point2D pt);
    }
}
