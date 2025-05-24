namespace Bullet;

using Godot;
using System;

public partial class Bullet : Node2D
{
	[Export]
	public float Speed { get; set; }
	[Export]
	public Vector2 Direction
	{
		get => _direction;
		set => _direction = value.Normalized();
	}
	[Export]
	public AudioStream SoundEffect { get; set; }

	private Vector2 _direction = Vector2.Right;
	private AudioStreamPlayer2D _audioPlayer;

	public override void _Ready()
	{
		var notifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
		notifier.ScreenExited += OnScreenExited;

		_audioPlayer = new AudioStreamPlayer2D();
		_audioPlayer.Name = "AudioStreamPlayer2D";
		AddChild(_audioPlayer);

		if (SoundEffect != null)
		{
			_audioPlayer.Stream = SoundEffect;
		}
		else
		{
			GD.PrintErr("SoundEffect is not set. Please assign a sound effect in the editor.");
			return;
		}

		_audioPlayer.Play();
	}

	private void OnScreenExited()
	{
		QueueFree();
	}

	public override void _Process(double delta)
	{
		Position += _direction * Speed * (float)delta;
	}
}
