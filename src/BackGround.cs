namespace BackGround;

using Godot;
using System;

public partial class BackGround : ParallaxBackground
{
    [Export]
    public float ScrollSpeed { get; set; } // スクロール速度

    public override void _Process(double delta)
    {
        ScrollOffset += new Vector2((float)(ScrollSpeed * delta * -1), 0); 
    }
}
