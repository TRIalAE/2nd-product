namespace EnemySpawner;

using Godot;
using System;
using Godot.Collections;
using System.Linq;

public partial class EnemySpawner : Node2D
{
    [ExportGroup("出現設定")]
    [Export]
    public double SpawnAttemptInterval { get; set; } = 1.0; // 敵機出現試行間隔（秒）

    [Export(PropertyHint.Range, "0.0,1.0")]
    public double SpawnAttemptProbability { get; set; } = 0.5; // 敵機出現試行確率（0.0 から 1.0 の範囲）

    [ExportGroup("コスト管理")]
    [Export]
    public int MaxSpawnCostPerPhase { get; set; } = 10; // フェーズごとの最大出現コスト

    [ExportGroup("フェーズ設定")]
    [Export]
    public SpawnPerPhaseData CurrentSpawnPerPhaseData { get; set; } // 現在のフェーズデータ

    private int _currentSpawnCost = 0;

	/// <summary>
    /// GdUnit4でのテスト用に、現在のコストを外部から取得できるようにするメソッド。
    /// </summary>
    public int GetCurrentCost() => _currentSpawnCost;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        var timer = new Timer();
        timer.WaitTime = SpawnAttemptInterval;
        timer.Timeout += OnSpawnTimerTimeout;
        AddChild(timer);
        timer.Start();
    }

    private void OnSpawnTimerTimeout()
    {
        if (_rng.Randf() < SpawnAttemptProbability)
        {
            TrySpawnEnemy();
        }
    }

	/// <summary>
	/// 敵機を出現させる試行を行う。
	/// 現在のフェーズデータに基づいて出現可能な敵を選び、
	/// 重み付き抽選で1体を選択して出現させる。
	/// </summary>
	/// <remarks>
	/// 出現コストが最大値に達している場合は何もしない。
	/// 出現可能な敵がいない場合も何もしない。
	/// </remarks>
	/// <returns>なし</returns>
	private void TrySpawnEnemy()
	{
		// 現在のフェーズデータが設定されていなければ何もしない
		if (CurrentSpawnPerPhaseData == null || CurrentSpawnPerPhaseData.SpawnTable == null)
		{
			return;
		}

		// 現在のコストで出現可能な敵をテーブルから絞り込む
		var availableEnemies = CurrentSpawnPerPhaseData.SpawnTable
			.Where(entry => entry.Cost <= MaxSpawnCostPerPhase - _currentSpawnCost)
			.ToList();

		if (availableEnemies.Count == 0)
		{
			return;
		}

		// 重み付き抽選で出現させる敵を1体選ぶ
		var totalRate = availableEnemies.Sum(entry => entry.Rate);
		var randomValue = _rng.Randf() * totalRate;

		EnemySpawnEntry selectedEntry = null;
		foreach (var entry in availableEnemies)
		{
			if (randomValue < entry.Rate)
			{
				selectedEntry = entry;
				break;
			}
			randomValue -= entry.Rate;
		}

		if (selectedEntry != null)
		{
			InstanceEnemy(selectedEntry);
		}
	}

	/// <summary>
	/// 敵機を出現させる。
	/// 出現位置は画面外のランダムな位置に設定される。
	/// 出現した敵機は、ツリーから削除されたときにコストを減算するイベントを登録する。
	/// </summary>
	/// <param name="entry">出現させる敵機のエントリ</param>
	/// <returns>なし</returns>
	private void InstanceEnemy(EnemySpawnEntry entry)
	{
		if (entry.EnemyScene == null) return;

		var newEnemy = entry.EnemyScene.Instantiate<Area2D>();

		// (出現位置の決定ロジックは変更なし)
		var viewportRect = GetViewportRect();
		var spawnPosition = new Vector2();
		var side = _rng.RandiRange(0, 3);
		switch (side)
		{
			case 0: spawnPosition.X = _rng.RandfRange(0, viewportRect.Size.X); spawnPosition.Y = -50; break;
			case 1: spawnPosition.X = viewportRect.Size.X + 50; spawnPosition.Y = _rng.RandfRange(0, viewportRect.Size.Y); break;
			case 2: spawnPosition.X = _rng.RandfRange(0, viewportRect.Size.X); spawnPosition.Y = viewportRect.Size.Y + 50; break;
			case 3: spawnPosition.X = -50; spawnPosition.Y = _rng.RandfRange(0, viewportRect.Size.Y); break;
		}
		newEnemy.Position = spawnPosition;

		newEnemy.TreeExiting += () => OnEnemyDestroyed(entry.Cost);

		GetParent().AddChild(newEnemy);
		_currentSpawnCost += entry.Cost;
	}

    private void OnEnemyDestroyed(int cost)
    {
        _currentSpawnCost -= cost;
    }
}
