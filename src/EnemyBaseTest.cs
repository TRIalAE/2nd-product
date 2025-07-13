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
		var enemy = AutoFree(new EnemyBase());
		var hp = AutoFree(new HpNode());
		hp.Name = "HpNode";
		enemy.AddChild(hp);

		// WHEN
		enemy._Ready();

		// THEN
		AssertBool(hp.IsConnected("HpChanged", new Callable(enemy, nameof(enemy.OnEnemyHpChanged)))).IsTrue();
	}

	[TestCase]
	public void OnEnemyHpChanged_ShouldCallDefeat_WhenHpIsZero()
	{
		// GIVEN
		var enemy = AutoFree(new TestEnemy());
		var hp = AutoFree(new HpNode());
		enemy.AddChild(hp);
		enemy._Ready();

		// WHEN
		enemy.OnEnemyHpChanged(0, 100);

		// THEN
		AssertBool(enemy.DefeatedCalled).IsTrue();
	}

	[TestCase]
	public async Task Defeat_ShouldEmitDefeatedSignal()
	{
		// GIVEN
		var enemy = AutoFree(new TestEnemy());
		enemy.score = 123;

		// シグナルを待機
		// 苦肉の策で文字列直渡し
		var waitEmit = AssertSignal(enemy).IsEmitted("Defeated", 123).WithTimeout(1000);

		// WHEN
		enemy.Defeat(enemy.score);

		// THEN
		await waitEmit;
	}

	[TestCase]
	public void Score_ShouldBeInheritedFromDerivedClass()
	{
		// GIVEN
		var enemy = AutoFree(new EnemySample());

		// WHEN
		enemy._Ready();

		// THEN
		AssertInt(enemy.score).IsEqual(100);
	}
}
