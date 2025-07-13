namespace Enemy;

using Godot;
using System;

public partial class EnemySample : EnemyBase
{
    public override void _Ready()
    {
        base._Ready();
        score = 100;
    }

    public override void _PhysicsProcess(double delta)
    {
        // 敵ごとの移動ロジック（別issueと結合）
    }
}
