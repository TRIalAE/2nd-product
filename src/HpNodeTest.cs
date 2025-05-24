using Godot;
using GdUnit4;

namespace HpNodeTest;

using System.Threading.Tasks;
using HpNode;
using static Assertions;
using static Utils;

[TestSuite]
public partial class HpNodeTest
{
	// テスト用のシグナルを発行するクラス
	private sealed partial class TestEmitter : Node
	{
		[Signal]
		public delegate void HitEventHandler(int damage);

		public void EmitHit(int damage)
		{
			EmitSignal(SignalName.Hit, damage);
		}
	}

	[TestCase]
	public void Ready_ShouldInitializeWithMaxHp()
	{
		// GIVEN
		var hpNode = AutoFree(new HpNode());
		hpNode.MaxHp = 100;

		// WHEN
		hpNode._Ready();

		// THEN
		AssertInt(hpNode.CurrentHp).IsEqual(100);
	}

	[TestCase]
	public async Task OnHit_ShouldDecreaseHp()
	{
		// GIVEN
		var hpNode = AutoFree(new HpNode());
		hpNode.MaxHp = 100;
		hpNode._Ready();

		// シグナルを待機
		var waitEmit = AssertSignal(hpNode).IsEmitted(HpNode.SignalName.HpChanged, 80, 100).WithTimeout(1000);

		// WHEN
		hpNode.OnHit(20);

		// THEN
		AssertInt(hpNode.CurrentHp).IsEqual(80);

		await waitEmit;
	}

	[TestCase]
	public async Task OnHit_ShouldNotDecreaseHpBelowZero()
	{
		// GIVEN
		var hpNode = AutoFree(new HpNode());
		hpNode.MaxHp = 100;
		hpNode._Ready();

		// シグナルを待機
		var waitEmit = AssertSignal(hpNode).IsEmitted(HpNode.SignalName.HpChanged, 0, 100).WithTimeout(1000);

		// WHEN
		hpNode.OnHit(120);

		// THEN
		AssertInt(hpNode.CurrentHp).IsEqual(0);
		await waitEmit;
	}

	[TestCase]
	public async Task OnHit_ShouldNotDecreaseHpIfAlreadyZero()
	{
		// GIVEN
		var hpNode = AutoFree(new HpNode());
		hpNode.MaxHp = 100;
		hpNode._Ready();
		hpNode.OnHit(100); // HPを0にする

		// シグナルを待機
		var waitEmit = AssertSignal(hpNode).IsNotEmitted(HpNode.SignalName.HpChanged).WithTimeout(1000);

		// WHEN
		hpNode.OnHit(20);

		// THEN
		AssertInt(hpNode.CurrentHp).IsEqual(0);
		await waitEmit;
	}

	[TestCase]
	public void OnHit_ShouldDecreaseHp_WhenHitSignalReceived()
	{
		// GIVEN
		var hpNode = AutoFree(new HpNode());
		hpNode.MaxHp = 100;
		hpNode._Ready();

		var testEmitter = AutoFree(new TestEmitter());
		hpNode.AddChild(testEmitter);
		testEmitter.Hit += hpNode.OnHit;

		// WHEN
		// シグナルを発信(ダメージ:20)
		testEmitter.EmitHit(20);

		// THEN
		AssertInt(hpNode.CurrentHp).IsEqual(80);
	}
}
