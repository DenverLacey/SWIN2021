using System;
using System.Collections.Generic;
using SplashKitSDK;

namespace Drawing_Program
{
    class Drawing
    {
        private readonly List<Shape> shapes;
        private Color background;

        public Color Background { get => background; set => background = value; }
        public int ShapeCount { get => shapes.Count; }

        public List<Shape> SelectedShapes
        {
            get
            {
                var result = new List<Shape>();

                foreach (Shape s in shapes)
                {
                    if (s.Selected)
                    {
                        result.Add(s);
                    }
                }

                return result;
            }
        }

        public Drawing() : this(Color.White) { }
        public Drawing(Color background)
        {
            this.background = background;
            shapes = new List<Shape>();
        }

        public void Draw()
        {
            SplashKit.ClearScreen(background);

            foreach (Shape s in shapes)
            {
                s.Draw();
            }
        }

        public void SelectShapesAt(Point2D pt)
        {
            foreach (Shape s in shapes)
            {
                s.Selected = s.IsAt(pt);
            }
        }

        public void AddShape(Shape s)
        {
            shapes.Add(s);
        }

        public void RemoveShape(Shape s)
        {
            shapes.Remove(s);
        }
    }
}
