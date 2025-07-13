namespace Enemy;

using Godot;
using HpNode;
using System;

public partial class EnemyBase : Node
{
    [Signal]
    public delegate void DefeatedEventHandler(int score);

    [Export]
    public int score { get; set; }
    
    protected HpNode hpNode;

    public override void _Ready()
    {
        hpNode = GetNode<HpNode>("HpNode");

        if (hpNode != null)
            hpNode.Connect(HpNode.SignalName.HpChanged, new Callable(this, nameof(OnEnemyHpChanged)));
    }

    public void OnEnemyHpChanged(int currentHp, int maxHp)
    {
        if (currentHp <= 0)
            Defeat(score);
    }

    public virtual void Defeat(int score)
    {
        GD.Print($"撃破されました: {score}");
        QueueFree();
        // スコア増加処理が入る（別issueと結合）
    }
}