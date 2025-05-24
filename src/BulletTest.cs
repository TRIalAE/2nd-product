using Godot;
using GdUnit4;

namespace BulletTest;

using Bullet;
using static Assertions;
using static Utils;

[TestSuite]
public class BulletTest
{
	private ISceneRunner _sceneRunner;
	[BeforeTest]
	public void SetUp()
	{
		_sceneRunner = ISceneRunner.Load("res://assets/bullet/player/bullet.tscn");
	}

	[TestCase]
	public void Direction_ShouldBeNormalized_WhenSet()
	{
		// GIVEN
		var bullet = AutoFree(new Bullet());

		// WHEN
		bullet.Direction = new Vector2(10f, 0f); // Right direction

		// THEN
		AssertVector(bullet.Direction).IsEqual(Vector2.Right);
	}

	[TestCase]
	public void Process_ShouldUpdatePosition()
	{
		// GIVEN
		var bullet = AutoFree(new Bullet());
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
		var bullet = AutoFree(new Bullet());
		bullet.Position = Vector2.Zero;
		bullet.Speed = 200f;
		bullet.Direction = new Vector2(0f, 1f); // Downward direction

		// WHEN
		bullet._Process(1.0);

		// THEN
		AssertVector(bullet.Position).IsEqual(new Vector2(0f, 200f));
	}

	[TestCase]
	public void Ready_ShouldPlaySpecificAudio_WhenCalled()
	{
		// GIVEN
		var bullet = _sceneRunner.Scene() as Bullet;
		var audioStream = GD.Load<AudioStream>("res://assets/bullet/player/bullet_shotA.wav");
		bullet.SoundEffect = audioStream;

		// WHEN
		bullet._Ready();

		// THEN
		var node = bullet as Node;
		var audioPlayer = node.GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		AssertThat(audioPlayer).IsNotNull();
		AssertThat(audioPlayer.Playing).IsTrue();
		AssertThat(audioPlayer.Stream).IsNotNull();
		AssertThat(audioPlayer.Stream.ResourcePath).IsEqual("res://assets/bullet/player/bullet_shotA.wav");
	}
}
