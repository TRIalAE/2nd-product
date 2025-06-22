using Godot;

[GlobalClass]
public partial class EnemySpawnEntry : Resource
{
    [Export]
    public PackedScene EnemyScene { get; set; } // 出現させる敵機シーン

    [Export(PropertyHint.Range, "0.0,1.0")]
    public float Rate { get; set; } // 出現率 (0.0 から 1.0 の範囲)

    [Export]
    public int Cost { get; set; } // 敵機を出現させるためのコスト
}