namespace GameRandom;

public struct RandomItem<T>
{
	public T value;

	public float weight;

	public float dynamicWeight;

	public bool must;

	public void ResetDynamicValue()
	{
		dynamicWeight = weight;
		must = false;
	}
}
