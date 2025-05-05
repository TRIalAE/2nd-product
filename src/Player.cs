namespace Player;

using Godot;
using System;

public partial class Player : Area2D
{
	[Export]
	public float Speed { get; set; } // プレイヤーの移動速度

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 direction = Vector2.Zero;

		if (Input.IsActionPressed("player_right"))
			direction.X += 1;
		if (Input.IsActionPressed("player_left"))
			direction.X -= 1;
		if (Input.IsActionPressed("player_down"))
			direction.Y += 1;
		if (Input.IsActionPressed("player_up"))
			direction.Y -= 1;

		direction = direction.Normalized();

		Position += direction * Speed * (float)delta;
	}
}
