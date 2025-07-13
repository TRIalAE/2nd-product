namespace Enemy;

using Godot;
using HpNode;
using System;

public partial class EnemyBase : Node
{
    [Export]
    public int score { get; set; }
    
    protected HpNode hpNode;

    public override void _Ready()
    {
        hpNode = GetNode<HpNode>("HpNode");

        if (hpNode != null)
            hpNode.Connect(HpNode.SignalName.HpChanged, new Callable(this, nameof(OnHpChanged)));
    }

    public void OnHpChanged(int currentHp, int maxHp)
    {
        if (currentHp <= 0)
            Defeat(score);
    }



    public void Defeat(int score)
    {
        GD.Print(score);
        // スコア増加処理が入る？
    }
}


/* AddScore（Player）
 * ↑
 * Defeat（Enemyの基底クラス）
 * ↑
 * OnDefeat（Enemyの派生クラス）
 * ↑
 * OnEnemyHpChanged
 * ↑
 * OnHit（HpNode）
 * ↑
 * 何かがこれを呼ぶ？
 */