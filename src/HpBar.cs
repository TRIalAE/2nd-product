namespace HpBar;

using Godot;
using System;

public partial class HpBar : Control
{
	private TextureProgressBar _hpBar;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_hpBar = GetNode<TextureProgressBar>("TextureProgressBar");

		// HPバーの初期値を設定
		_hpBar.MaxValue = 100;
		_hpBar.Value = 100;
	}

	public void OnHpChanged(int currentHp, int maxHp)
	{
		if (maxHp <= 0)
			return;

		// HpChangedシグナルを受け取った際にHPバーの値を更新
		_hpBar.Value = (float)currentHp / maxHp * _hpBar.MaxValue;
	}
}
