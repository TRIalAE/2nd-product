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
	public int GetCurrentCost() { return _currentSpawnCost; }
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
	/// <param name="fixedRandomValue">テスト用の固定ランダム値。nullの場合はランダムに選択。</param>
	/// <remarks>
	/// 出現コストが最大値に達している場合は何もしない。
	/// 出現可能な敵がいない場合も何もしない。
	/// </remarks>
	/// <returns>なし</returns>
	public void TrySpawnEnemy(float? fixedRandomValue = null)
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

		// テスト用に固定のランダム値が指定されている場合はそれを使用
		// 0からtotalRateの範囲に収める
		if (fixedRandomValue.HasValue)
		{
			randomValue = Mathf.Clamp(fixedRandomValue.Value, 0, totalRate);
		}

		EnemySpawnEntry selectedEntry = null;

		float cumulativeRate = 0.0f;
		foreach (var entry in availableEnemies)
		{
			cumulativeRate += entry.Rate;
			if (randomValue <= cumulativeRate)
			{
				selectedEntry = entry;
				break;
			}
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

		// 出現位置を画面外のランダムな位置に設定
		// 画面の右端から50ピクセル外に出現させる
		// Y座標は画面の高さに基づいてランダムに設定する
		var viewportRect = GetViewportRect();
		var spawnPosition = new Vector2();
		spawnPosition.X = viewportRect.Size.X + 50;
		spawnPosition.Y = _rng.RandfRange(0, viewportRect.Size.Y);
		newEnemy.Position = spawnPosition;

		newEnemy.TreeExiting += () => OnEnemyDestroyed(entry.Cost);

		GetParent().AddChild(newEnemy);
		_currentSpawnCost += entry.Cost;

		GD.Print($"Enemy spawned: {entry.EnemyScene.ResourceName}, cost: {entry.Cost}, current total cost: {GetCurrentCost()}");
	}

	private void OnEnemyDestroyed(int cost)
	{
		_currentSpawnCost -= cost;
		GD.Print($"Enemy destroyed, current cost: {GetCurrentCost()}");
	}
}
