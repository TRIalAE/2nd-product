namespace Sample;

using Godot;
using System;

public partial class Sample : Sprite2D
{
	private const int _speed = 400;
	private const float _angularSpeed = Mathf.Pi;

	public override void _Process(double delta)
	{
		Rotation += _angularSpeed * (float)delta;
		var velocity = Vector2.Up.Rotated(Rotation) * _speed;

		Position += velocity * (float)delta;
	}

	public int Return5()
	{
		return 5;
	}
}
