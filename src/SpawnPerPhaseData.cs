using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SpawnPerPhaseData : Resource
{
	[Export]
	public Array<EnemySpawnEntry> SpawnTable { get; set; } // このフェーズの出現テーブル
}
