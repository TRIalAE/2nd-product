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

    private Vector2 _direction = Vector2.Right;

    public override void _Ready()
    {
        var notifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
        notifier.ScreenExited += OnScreenExited;
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
