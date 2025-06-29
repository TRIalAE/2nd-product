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

		// Act (実行)
		if (enemyInstance != null)
		{
			enemyInstance.QueueFree(); // 敵機を削除
			await Task.Delay(100); // 少し待機して削除が反映されるのを待つ
		}

		// Assert (検証)
		var spawnedEnemiesAfter = GetEnemyCount();
		var costAfter = _spawner.GetCurrentCost();

		AssertThat(spawnedEnemiesAfter).IsEqual(spawnedEnemiesBefore - 1);
		AssertThat(costAfter).IsLess(costBefore); // コストが減少していること
	}

	[TestCase]
	// 抽選で適切な敵が選ばれるかを確認するテスト
	public async Task Spawn_WhenMultipleEnemiesAvailable_ShouldSelectBasedOnRate()
	{
		// Arrange (準備)
		_spawner.SpawnAttemptProbability = 0.0; // 確率0%にして自動的なスポーンを防ぐ
		_spawner.MaxSpawnCostPerPhase = 10;

		// コストが1, 2, 3の敵を用意することで、コストの減少量でどの敵が選ばれたかを判別できるようにする
		_spawner.CurrentSpawnPerPhaseData = new SpawnPerPhaseData
		{
			SpawnTable = new Array<EnemySpawnEntry>
			{
				new EnemySpawnEntry { EnemyScene = _dummyEnemyScene, Cost = 1, Rate = 1.0f },
				new EnemySpawnEntry { EnemyScene = _dummyEnemyScene, Cost = 2, Rate = 1.0f },
				new EnemySpawnEntry { EnemyScene = _dummyEnemyScene, Cost = 3, Rate = 1.0f }
			}
		};

		// 現在出現している敵を全て削除
		foreach (var enemy in _spawner.GetParent().GetChildren().OfType<Area2D>())
		{
			enemy.QueueFree();
		}

		await Task.Delay(100); // 少し待機して削除が反映されるのを待つ

		var spawnedEnemiesBefore = GetEnemyCount();
		var costBefore = _spawner.GetCurrentCost();

		// Act (実行)
		// ここではランダム値を固定して、特定の敵が選ばれることを確認する
		// 1.0f以下のランダム値を使用すると、最初の敵が選ばれる
		_spawner.TrySpawnEnemy(1.0f);

		await Task.Delay(100); // 少し待機してスポーンが反映されるのを待つ

		// Assert (検証)
		var spawnedEnemies = _spawner.GetParent().GetChildren().OfType<Area2D>().ToList();
		var spawnedEnemiesAfter = GetEnemyCount();
		var costAfter = _spawner.GetCurrentCost();

		AssertThat(spawnedEnemiesAfter - spawnedEnemiesBefore).IsEqual(1);
		AssertThat(costAfter - costBefore).IsEqual(1); // コストが1減っていること

		spawnedEnemiesBefore = spawnedEnemiesAfter;
		costBefore = costAfter;

		// Act (実行) - 追加の敵をスポーンして、選ばれる敵が正しいか確認
		// ここではランダム値を2.0fに設定して、2番目の敵が選ばれることを確認する
		_spawner.TrySpawnEnemy(2.0f);

		await Task.Delay(100); // 少し待機してスポーンが反映されるのを待つ

		// Assert (検証)
		spawnedEnemiesAfter = GetEnemyCount();
		costAfter = _spawner.GetCurrentCost();
		spawnedEnemies = _spawner.GetParent().GetChildren().OfType<Area2D>().ToList();

		AssertThat(spawnedEnemiesAfter - spawnedEnemiesBefore).IsEqual(1);
		AssertThat(costAfter - costBefore).IsEqual(2); // コストが2減っていること

		spawnedEnemiesBefore = spawnedEnemiesAfter;
		costBefore = costAfter;

		// Act (実行) - さらに3番目の敵をスポーンして、選ばれる敵が正しいか確認
		_spawner.TrySpawnEnemy(3.0f);

		await Task.Delay(100); // 少し待機してスポーンが反映されるのを待つ

		// Assert (検証)
		spawnedEnemiesAfter = GetEnemyCount();
		costAfter = _spawner.GetCurrentCost();
		spawnedEnemies = _spawner.GetParent().GetChildren().OfType<Area2D>().ToList();

		AssertThat(spawnedEnemiesAfter - spawnedEnemiesBefore).IsEqual(1);
		AssertThat(costAfter - costBefore).IsEqual(3); // コストが3減っていること
	}

	// helpers
	private int GetEnemyCount()
	{
		return _spawner.GetParent().GetChildren().Count(node => node is Area2D);
	}
}
