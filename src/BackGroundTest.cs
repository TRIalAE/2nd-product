using Godot;
using GdUnit4;

namespace BackGroundTest;

using BackGround;
using static Assertions;
using static Utils;

[TestSuite]
public class BackGroundTest
{
	[TestCase]
	public void Process_ShouldUpdatePosition()
	{
		// GIVEN
		var background = new BackGround();
		background.ScrollOffset = Vector2.Zero;
		background.ScrollSpeed = 200f;

		// WHEN
		background._Process(1.0); // delta = 1秒と仮定

		// THEN
		AssertVector(background.ScrollOffset).IsEqual(new Vector2(-200f, 0f)); // 初期位置から200f左に移動
	}
}
