using Godot;
using GdUnit4;

namespace EnemyTest;

using Enemy;
using HpNode;
using System.Threading.Tasks;
using static Assertions;

[TestSuite]
public partial class EnemyBaseTest
{
	// 仮想敵
	private sealed partial class TestEnemy : EnemyBase
	{
		public bool DefeatedCalled { get; private set; } = false;

		public override void Defeat(int score)
		{
			// 親クラスではQueueFreeしてるが、即時破棄するとシグナルの検証ができなくなるためオーバーライド
			DefeatedCalled = true;
			EmitSignal(SignalName.Defeated, score);
		}

	}

	[TestCase]
	public void Ready_ShouldConnectHpChangedSignal()
	{
		// GIVEN
		var enemy = AutoFree(new TestEnemy());
		var hp = AutoFree(new HpNode());
		hp.Name = "HpNode";
		enemy.AddChild(hp);

		// WHEN
		enemy._Ready();

		// THEN
		AssertBool(hp.IsConnected("HpChanged", new Callable(enemy, nameof(enemy.OnEnemyHpChanged)))).IsTrue();
	}

	// [TestCase]
	// // attributeでテストできるはずだがなぜ名前解決できない？？？？？？？
	// [ThrowsException(typeof(Exception))]
	// public void Ready_ShouldThrowException_WhenHpNodeIsMissing()
	// {
	// 	// GIVEN
	// 	var enemy = AutoFree(new TestEnemy());

	// 	// WHEN
	// 	enemy._Ready();

	// 	// WHEN THEN
	// 	// こいつも名前解決できないのが原因。仕事しろ
	// 	// AssertThat(() => enemy._Ready()).ThrowsException<Exception>();
	// }

	[TestCase]
	public void OnEnemyHpChanged_ShouldCallDefeat_WhenHpIsZero()
	{
		// GIVEN
		var enemy = AutoFree(new TestEnemy());
		var hp = AutoFree(new HpNode());
		hp.Name = "HpNode";
		enemy.AddChild(hp);

		// WHEN
		enemy._Ready();
		enemy.OnEnemyHpChanged(0, 100);

		// THEN
		AssertBool(enemy.DefeatedCalled).IsTrue();
	}

	[TestCase]
	public async Task Defeat_ShouldEmitDefeatedSignal()
	{
		// GIVEN
		var enemy = AutoFree(new TestEnemy());
		var hp = AutoFree(new HpNode());
		hp.Name = "HpNode";
		enemy.AddChild(hp);

		enemy.score = 123;

		// シグナルを待機
		var waitEmit = AssertSignal(enemy).IsEmitted(EnemyBase.SignalName.Defeated, 123).WithTimeout(1000);

		// WHEN
		enemy.Defeat(enemy.score);

		// THEN
		await waitEmit;
	}

	[TestCase]
	// note: EnemySampleを削除する際にはこのテストも削除する
	public void Score_ShouldBeInheritedFromDerivedClass()
	{
		// GIVEN
		var enemy = AutoFree(new EnemySample());
		var hp = AutoFree(new HpNode());
		hp.Name = "HpNode";
		enemy.AddChild(hp);

		// WHEN
		enemy._Ready();

		// THEN
		AssertInt(enemy.score).IsEqual(100);
	}
}
