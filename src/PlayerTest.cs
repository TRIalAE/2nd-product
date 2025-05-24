using Godot;
using GdUnit4;

namespace PlayerTest;

using System.Threading;
using System.Threading.Tasks;
using Player;
using static Assertions;
using static Utils;
using Bullet;

[TestSuite]
public class PlayerTest
{
	private ISceneRunner _sceneRunner;
	private const float Speed = 200f;
	private const float DeltaTime = 1f / 60f; // 待機時間(60FPSを想定)

	[BeforeTest]
	public void SetUp()
	{
		_sceneRunner = ISceneRunner.Load("res://assets/player/Player.tscn");
	}

	[TestCase]
	public async Task MovesRight_WhenRightKeyPressed()
	{
		var player = _sceneRunner.Scene() as Player;
		player.Position = Vector2.Zero;
		player.Speed = Speed;

		// キーを押した状態で待機
		_sceneRunner.SimulateActionPress("player_right");
		await Task.Delay((int)(DeltaTime * 1000));

		AssertFloat(player.Position.X).IsGreater(0f);
		AssertFloat(player.Position.Y).IsEqual(0f);

		// 最後にキーを離す
		_sceneRunner.SimulateActionRelease("player_right");
	}

	[TestCase]
	public async Task MovesLeft_WhenLeftKeyPressed()
	{
		var player = _sceneRunner.Scene() as Player;
		player.Position = Vector2.Zero;
		player.Speed = Speed;

		// キーを押した状態で待機
		_sceneRunner.SimulateActionPress("player_left");
		await Task.Delay((int)(DeltaTime * 1000));

		AssertFloat(player.Position.X).IsLess(0f);
		AssertFloat(player.Position.Y).IsEqual(0f);

		// 最後にキーを離す
		_sceneRunner.SimulateActionRelease("player_left");
	}

	[TestCase]
	public async Task MovesUp_WhenUpKeyPressed()
	{
		var player = _sceneRunner.Scene() as Player;
		player.Position = Vector2.Zero;
		player.Speed = Speed;

		// キーを押した状態で待機
		_sceneRunner.SimulateActionPress("player_up");
		await Task.Delay((int)(DeltaTime * 1000));

		AssertFloat(player.Position.Y).IsLess(0f);
		AssertFloat(player.Position.X).IsEqual(0f);

		// 最後にキーを離す
		_sceneRunner.SimulateActionRelease("player_up");
	}

	[TestCase]
	public async Task MovesDown_WhenDownKeyPressed()
	{
		var player = _sceneRunner.Scene() as Player;
		player.Position = Vector2.Zero;
		player.Speed = Speed;

		// キーを押した状態で待機
		_sceneRunner.SimulateActionPress("player_down");
		await Task.Delay((int)(DeltaTime * 1000));

		AssertFloat(player.Position.Y).IsGreater(0f);
		AssertFloat(player.Position.X).IsEqual(0f);

		// 最後にキーを離す
		_sceneRunner.SimulateActionRelease("player_down");
	}

	[TestCase]
	public void ResetsCooldown_AfterShooting()
	{
		// GIVEN
		var player = _sceneRunner.Scene() as Player;
		player.Position = Vector2.Zero;
		player.FireCooldown = 0.5;

		player.TimeSinceLastShot = 10.0;

		// WHEN
		_sceneRunner.SimulateActionPress("shoot");
		player._Process(0.5);
		_sceneRunner.SimulateActionRelease("shoot");

		// THEN
		AssertFloat(player.TimeSinceLastShot).IsEqual(0.0);
		AssertInt(player.GetChildren().Count).IsGreaterEqual(1);
	}
}
