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

	[Before]
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

		var spawnedEnemiesBefore = GetEnemyCount();
		var costBefore = _spawner.GetCurrentCost();

		var timer = _spawner.GetChild(0) as Timer;

		// Act (実行)
		await timer.ToSignal(timer, Timer.SignalName.Timeout);

		// Assert (検証)
		// 敵機が1体出現しているか
		var spawnedEnemiesAfter = GetEnemyCount();
		AssertThat(spawnedEnemiesAfter - spawnedEnemiesBefore).IsEqual(1);
		// スポナーのコストが1になっているか
		var costAfter = _spawner.GetCurrentCost();
		AssertThat(costAfter - costBefore).IsEqual(1);
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

		var spawnedEnemiesBefore = GetEnemyCount();
		var costBefore = _spawner.GetCurrentCost();

		var timer = _spawner.GetChild(0) as Timer;

		// Act (実行)
		await timer.ToSignal(timer, Timer.SignalName.Timeout);

		// Assert (検証)
		var spawnedEnemiesAfter = GetEnemyCount();
		AssertThat(spawnedEnemiesAfter - spawnedEnemiesBefore).IsEqual(0);

		var costAfter = _spawner.GetCurrentCost();
		AssertThat(costAfter).IsEqual(costBefore);
	}

	[TestCase]
	public async Task Spawn_WhenCostIsMax_ShouldNotSpawnEnemy()
	{
		// Arrange (準備)
		_spawner.SpawnAttemptProbability = 1.0; // 確率100%
		_spawner.MaxSpawnCostPerPhase = _spawner.GetCurrentCost(); // 現在のコストを最大値に設定

		_spawner.CurrentSpawnPerPhaseData = new SpawnPerPhaseData
		{
			SpawnTable = new Array<EnemySpawnEntry>
			{
				new EnemySpawnEntry { EnemyScene = _dummyEnemyScene, Cost = 1, Rate = 1.0f }
			}
		};

		var spawnedEnemiesBefore = GetEnemyCount();
		var costBefore = _spawner.GetCurrentCost();

		var timer = _spawner.GetChild(0) as Timer;

		// Act (実行)
		await timer.ToSignal(timer, Timer.SignalName.Timeout);

		// Assert (検証)
		var spawnedEnemiesAfter = GetEnemyCount();
		AssertThat(spawnedEnemiesAfter - spawnedEnemiesBefore).IsEqual(0);

		var costAfter = _spawner.GetCurrentCost();
		AssertThat(costAfter).IsEqual(costBefore); // コストは変わらない
	}

	[TestCase]
	public async Task Spawn_WhenEnemyDestroyed_ShouldDecreaseCost()
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

		await timer.ToSignal(timer, Timer.SignalName.Timeout);

		var spawnedEnemiesBefore = GetEnemyCount();
		var costBefore = _spawner.GetCurrentCost();

		var enemyInstance = _spawner.GetParent().GetChildren().FirstOrDefault(node => node is Area2D) as Area2D;

		if (enemyInstance != null)
		{
			enemyInstance.QueueFree(); // 敵機を削除
			await Task.Delay(100); // 少し待機して削除が反映されるのを待つ
		}

		var spawnedEnemiesAfter = GetEnemyCount();
		var costAfter = _spawner.GetCurrentCost();

		AssertThat(spawnedEnemiesAfter).IsEqual(spawnedEnemiesBefore - 1);
		AssertThat(costAfter).IsNotEqual(costBefore); // コストが減少していること
	}

	// helpers
	private int GetEnemyCount()
	{
		return _spawner.GetParent().GetChildren().Count(node => node is Area2D);
	}
}
