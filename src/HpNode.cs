namespace HpNode;

using Godot;
using System;

public partial class HpNode : Node
{
	[Signal]
	public delegate void HpChangedEventHandler(int currentHp, int maxHp);

	[Export]
	public int MaxHp { get; set; } = 100;

	public int CurrentHp { get; private set; } = 100;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CurrentHp = MaxHp;
		EmitSignal(SignalName.HpChanged, CurrentHp, MaxHp);
	}

	public void OnHit(int damage)
	{
		if (CurrentHp <= 0)
			return;

		// Hitシグナルを受け取ったときにHPを減少させる
		CurrentHp = Mathf.Max(CurrentHp - damage, 0);

		// HpChangedシグナルを発行
		// 現在HPと最大HPを引数に渡す
		EmitSignal(SignalName.HpChanged, CurrentHp, MaxHp);
	}
}
