using Godot;
using GdUnit4;

namespace SampleTest;

using Sample;
using static Assertions;
using static Utils;

[TestSuite]
public class SampleTest
{
	[TestCase]
	public void Process_ShouldUpdatePositionAndRotation()
	{
		// GIVEN
		var sample = new Sample();
		sample.Position = Vector2.Zero;
		sample.Rotation = 0f;

		// WHEN
		sample._Process(1.0); // delta = 1秒と仮定

		// THEN
		AssertFloat(sample.Rotation).IsEqual(Mathf.Pi);
		AssertVector(sample.Position).IsNotEqual(Vector2.Zero);
	}

	[TestCase]
	public void Return5_ShouldReturnNumberFive()
	{
		// GIVEN
		var sample = AutoFree(new Sample());

		// WHEN
		var result = sample.Return5();

		// THEN
		AssertInt(result).IsEqual(5);
	}
}
