namespace Player;

using Godot;
using System;

public partial class Player : Area2D
{
	[Export]
	public float Speed { get; set; } // プレイヤーの移動速度

	[Export]
	public PackedScene BulletScene { get; set; }

	[Export]
	public double FireCooldown { get; set; }

	[Export]
	public Vector2 BulletOffset { get; set; }

	public double TimeSinceLastShot { get; set; } = 0;

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

		TimeSinceLastShot += delta;

		if (Input.IsActionPressed("shoot") && TimeSinceLastShot >= FireCooldown)
		{
			Shoot();
			TimeSinceLastShot = 0;
		}
	}

	public void Shoot()
	{
		if (BulletScene == null)
		{
			GD.PrintErr("BulletScene is not set.");
			return;
		}

		var bullet = BulletScene.Instantiate() as Bullet.Bullet;

		bullet.Position = Position + BulletOffset;
		bullet.ZIndex = -1;

		bullet.Direction = Vector2.Right;

		GetTree().Root.AddChild(bullet);
	}
}
