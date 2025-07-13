using Godot;
using GdUnit4;

namespace EnemyTest;

using System.Threading.Tasks;
using HpNode;
using Enemy;
using static Assertions;

[TestSuite]
public partial class EnemyBaseTest
{
    // テスト用のシグナルを発行するクラス
    private sealed partial class TestEmitter : Node
    {[TestCase]
public async Task OnHit_Test()
{
	// GIVEN
	var hpNode = AutoFree(new HpNode());
	hpNode.MaxHp = 100;
	hpNode._Ready(); // ここは Root.AddChild(...) でも可

	// シグナルを待機
	var waitEmit = AssertSignal(hpNode).IsEmitted(HpNode.SignalName.HpChanged, 80, 100).WithTimeout(1000);

	// WHEN
	hpNode.OnHit(20);

	// THEN
	AssertInt(hpNode.CurrentHp).IsEqual(80);

	await waitEmit;
}
        public delegate void HitEventHandler(int damage);

        public void EmitHit(int damage)
        {
            EmitSignal(SignalName.Hit, damage);
        }
    }

    [TestCase]
    public async Task OnHit_Test()
    {
        // GIVEN
        var hpNode = AutoFree(new HpNode());
        hpNode.MaxHp = 100;
        hpNode._Ready(); // ここは Root.AddChild(...) でも可

        // シグナルを待機
        var waitEmit = AssertSignal(hpNode).IsEmitted(HpNode.SignalName.HpChanged, 80, 100).WithTimeout(1000);

        // WHEN
        hpNode.OnHit(20);

        // THEN
        AssertInt(hpNode.CurrentHp).IsEqual(80);

        await waitEmit;
    }
}