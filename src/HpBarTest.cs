using Godot;
using GdUnit4;

namespace HpBarTest;

using System.Diagnostics;
using HpBar;
using static Assertions;
using static Utils;

[TestSuite]
public partial class HpBarTest
{
	// テスト用のシグナルを発行するクラス
	private sealed partial class TestEmitter : Node
	{
		[Signal]
		public delegate void HpChangedEventHandler(int currentHp, int maxHp);

		public void EmitHpChanged(int currentHp, int maxHp)
		{
			EmitSignal(SignalName.HpChanged, currentHp, maxHp);
		}
	}

	private ISceneRunner _sceneRunner;

	[BeforeTest]
	public void SetUp()
	{
		_sceneRunner = ISceneRunner.Load("res://assets/hp_bar/hp_bar.tscn");
	}

	[TestCase]
	public void OnHpChanged_ShouldUpdateHpBar()
	{
		// GIVEN
		var hpBar = _sceneRunner.Scene() as HpBar;
		var textureProgressBar = _sceneRunner.FindChild("TextureProgressBar") as TextureProgressBar;
		hpBar._Ready();

		// WHEN
		// HPバーの値を更新(最大値:100 → 現在値:30)
		hpBar.OnHpChanged(30, 100);

		// THEN
		AssertFloat(textureProgressBar.Value).IsEqual(30);
	}

	[TestCase]
	public void OnHpChanged_ShouldNotUpdateHpBar_WhenMaxHpIsZero()
	{
		// GIVEN
		var hpBar = _sceneRunner.Scene() as HpBar;
		var textureProgressBar = _sceneRunner.FindChild("TextureProgressBar") as TextureProgressBar;
		hpBar._Ready();

		// WHEN
		// HPバーの値を更新(最大値:0)
		hpBar.OnHpChanged(0, 0);

		// THEN
		AssertFloat(textureProgressBar.Value).IsEqual(100);
	}

	[TestCase]
	public void OnHpChanged_ShouldUpdateHpBar_WhenReceivedSignal()
	{
		// GIVEN
		var hpBar = _sceneRunner.Scene() as HpBar;
		var textureProgressBar = _sceneRunner.FindChild("TextureProgressBar") as TextureProgressBar;
		hpBar._Ready();

		var testEmitter = AutoFree(new TestEmitter());
		hpBar.AddChild(testEmitter);
		testEmitter.HpChanged += hpBar.OnHpChanged;

		// WHEN
		// シグナルを送信
		testEmitter.EmitHpChanged(50, 100);

		// THEN
		AssertFloat(textureProgressBar.Value).IsEqual(50);
		testEmitter.HpChanged -= hpBar.OnHpChanged;
	}
}
