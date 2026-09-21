using System;
using Unity.Mathematics;

public class PredictableRandom : ISaveCallback
{
	public Random random;

	[SaveField(null, null)]
	public uint state;

	public PredictableRandom()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		state = (uint)DateTime.Now.Ticks;
		random = new Random(state);
	}

	public PredictableRandom(uint state)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		this.state = state;
		random = new Random(state);
	}

	public int Next()
	{
		return ((Random)(ref random)).NextInt();
	}

	public int Next(int max)
	{
		return ((Random)(ref random)).NextInt(max);
	}

	public int Next(int min, int max)
	{
		return ((Random)(ref random)).NextInt(min, max);
	}

	public double NextDouble()
	{
		return ((Random)(ref random)).NextDouble();
	}

	public double NextDouble(double max)
	{
		return ((Random)(ref random)).NextDouble() * max;
	}

	public double NextDouble(double min, double max)
	{
		return ((Random)(ref random)).NextDouble() * (max - min) + min;
	}

	public float NextFloat()
	{
		return (float)((Random)(ref random)).NextDouble();
	}

	public float NextFloat(float max)
	{
		return NextFloat() * max;
	}

	public float NextFloat(float min, float max)
	{
		return NextFloat() * (max - min) + min;
	}

	public void BeforeSave()
	{
		state = random.state;
	}

	public void AfterLoad()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		random = default(Random);
		random.state = state;
	}
}
