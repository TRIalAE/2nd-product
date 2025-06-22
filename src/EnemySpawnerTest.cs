using Godot;
using GdUnit4;
using System.Threading.Tasks;
using Godot.Collections;
using System.Linq;

namespace EnemySpawnerTest;

using System;
using EnemySpawner;
using static Assertions;
using static Utils;

[TestSuite]
public class EnemySpawnerTest
{
	private ISceneRunner _sceneRunner;
	private EnemySpawner _spawner;
	private PackedScene _dummyEnemyScene;

	[BeforeTest]
	public void SetUp()
	{
		_sceneRunner = ISceneRunner.Load("res://assets/enemy_spawner/enemy_spawner.tscn");

		_spawner = _sceneRunner.Scene() as EnemySpawner;

		_dummyEnemyScene = ResourceLoader.Load<PackedScene>("res://assets/enemy_spawner/test/test_enemy.tscn");
	}


	[TestCase]
	public async Task Spawn_WhenProbabilityIsOne_ShouldSpawnEnemy()
	{
		// Arrange (準備)
		_spawner.SpawnAttemptProbability = 1.0; // 確率100%
		_spawner.MaxSpawnCostPerPhase = 10;
		_spawner.CurrentSpawnPerPhaseData = new SpawnPerPhaseData
		{
			SpawnTable = new Array<EnemySpawnEntry>
			{
				new EnemySpawnEntry { EnemyScene = _dummyEnemyScene, Cost = 1, Rate = 1.0f }
			}
		};

		var timer = _spawner.GetChild(0) as Timer;

		// Act (実行)
		await timer.ToSignal(timer, Timer.SignalName.Timeout);

		// Assert (検証)
		// 敵機が1体出現しているか
		var spawnedEnemies = _spawner.GetParent().GetChildren().Where(node => node is Area2D).ToArray();
		AssertThat(spawnedEnemies.Length).IsEqual(1);
		// スポナーのコストが1になっているか
		AssertThat(_spawner.GetCurrentCost()).IsEqual(1);
	}

	[TestCase]
	public async Task Spawn_WhenProbabilityIsZero_ShouldNotSpawnEnemy()
	{
		// Arrange (準備)
		_spawner.SpawnAttemptProbability = 0.0; // 確率0%
		_spawner.MaxSpawnCostPerPhase = 10;
		_spawner.CurrentSpawnPerPhaseData = new SpawnPerPhaseData
		{
			SpawnTable = new Array<EnemySpawnEntry>
			{
				new EnemySpawnEntry { EnemyScene = _dummyEnemyScene, Cost = 1, Rate = 1.0f }
			}
		};

		var timer = _spawner.GetChild(0) as Timer;

		// Act (実行)
		await timer.ToSignal(timer, Timer.SignalName.Timeout);

		// Assert (検証)
		var spawnedEnemies = _spawner.GetParent().GetChildren().Where(node => node is Area2D).ToArray();
		AssertThat(spawnedEnemies.Length).IsEqual(0);
		AssertThat(_spawner.GetCurrentCost()).IsEqual(0);
	}
}
