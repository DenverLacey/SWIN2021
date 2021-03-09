using System;
using SplashKitSDK;

public class Shape
{
	private Color color;
	private float x;
	private float y;
	private int width;
	private int height;

	public Color Color { get => color; set => color = value; }
	public float X { get => x; set => x = value; }
	public float Y { get => y; set => y = value; }
	public int Width { get => width; set => width = value; }
	public int Height { get => height; set => height = value; }

	public Shape(Color color, float x, float y, int width, int height)
	{
		this.color = color;
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}

	public void Draw()
	{
		SplashKit.FillRectangle(color, x, y, width, height);
    }

	public bool IsAt(Point2D pt)
    {
		return pt.X >= x && pt.X <= x + width &&
			pt.Y >= y && pt.Y <= y + height;
    }
}
