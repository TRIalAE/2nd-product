using Godot;
using GdUnit4;

namespace BulletTest;

using Bullet;
using static Assertions;
using static Utils;

[TestSuite]
public class BulletTest
{
	[TestCase]
	public void Direction_ShouldBeNormalized_WhenSet()
	{
		// GIVEN
		var bullet = new Bullet();

		// WHEN
		bullet.Direction = new Vector2(10f, 0f); // Right direction

		// THEN
		AssertVector(bullet.Direction).IsEqual(Vector2.Right);
	}

	[TestCase]
	public void Process_ShouldUpdatePosition()
	{
		// GIVEN
		var bullet = new Bullet();
		bullet.Position = Vector2.Zero;
		bullet.Speed = 200f;
		bullet.Direction = Vector2.Right;

		// WHEN
		bullet._Process(1.0);

		// THEN
		AssertVector(bullet.Position).IsEqual(new Vector2(200f, 0f));
	}

	[TestCase]
	public void Process_ShouldMoveInDirectionOfVector()
	{
		// GIVEN
		var bullet = new Bullet();
		bullet.Position = Vector2.Zero;
		bullet.Speed = 200f;
		bullet.Direction = new Vector2(0f, 1f); // Downward direction

		// WHEN
		bullet._Process(1.0);

		// THEN
		AssertVector(bullet.Position).IsEqual(new Vector2(0f, 200f));
	}
}
