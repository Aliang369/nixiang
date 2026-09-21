using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRandom;

public static class RandomExtension
{
	private static readonly Lazy<Random> staticRandom = new Lazy<Random>(() => new Random());

	public static T SelectRandom<T>(this IList<T> pool)
	{
		return pool.SelectRandom(staticRandom.Value.NextDouble);
	}

	public static T SelectRandom<T>(this IList<T> pool, Func<double> nextRandomGetter)
	{
		if (pool == null || pool.Count == 0)
		{
			return default(T);
		}
		return pool[(int)(nextRandomGetter() * 2147483647.0) % pool.Count];
	}

	public static List<T> SelectRandomCount<T>(this IList<T> pool, int count)
	{
		return pool.SelectRandomCount(count, staticRandom.Value.NextDouble);
	}

	public static List<T> SelectRandomCount<T>(this IList<T> pool, int count, Func<double> nextRandomGetter)
	{
		List<T> list = new List<T>();
		for (int i = 0; i < count; i++)
		{
			list.Add(pool.SelectRandom(nextRandomGetter));
		}
		return list;
	}

	public static List<T> SelectRandomCountUnique<T>(this IList<T> pool, int count)
	{
		return pool.SelectRandomCountUnique(count, staticRandom.Value.NextDouble);
	}

	public static List<T> SelectRandomCountUnique<T>(this IList<T> pool, int count, Func<double> nextRandomGetter)
	{
		if (count > pool.Count)
		{
			throw new Exception("count Greater than pool size");
		}
		if (count == pool.Count)
		{
			return pool.Shuffle();
		}
		List<T> list = new List<T>();
		List<T> list2 = new List<T>(pool);
		while (list.Count < count)
		{
			int index = (int)(nextRandomGetter() * 2147483647.0) % list2.Count;
			list.Add(list2[index]);
			list2.RemoveAt(index);
		}
		return list;
	}

	public static T SelectRandomWeight<T>(this IList<T> pool, Func<T, float> weightGenerator)
	{
		return pool.SelectRandomWeight(weightGenerator, staticRandom.Value.NextDouble);
	}

	public static T SelectRandomWeight<T>(this IList<T> pool, Func<T, float> weightGenerator, Func<double> nextRandomGetter)
	{
		List<float> list = new List<float>();
		foreach (T item in pool)
		{
			list.Add(weightGenerator(item));
		}
		return pool.SelectRandomWeight(list, nextRandomGetter);
	}

	public static T SelectRandomWeight<T>(this Dictionary<T, float> pool)
	{
		return pool.Keys.ToList().SelectRandomWeight((T l) => pool[l]);
	}

	public static T SelectRandomWeight<T>(this Dictionary<T, int> pool)
	{
		return pool.Keys.ToList().SelectRandomWeight((T l) => pool[l]);
	}

	public static T SelectRandomWeight<T>(this IList<T> pool, IList<float> weights, Func<double> nextRandomGetter)
	{
		if (pool.Count != weights.Count)
		{
			throw new Exception("pool count not equal to weights count");
		}
		float num = weights.Sum();
		float num2 = (float)nextRandomGetter() * num;
		float num3 = 0f;
		for (int i = 0; i < pool.Count; i++)
		{
			num3 += weights[i];
			if (num3 > num2)
			{
				return pool[i];
			}
		}
		return pool.Last();
	}

	public static List<T> SelectRandomWeightCount<T>(this IList<T> pool, Func<T, float> weightGenerator, int count)
	{
		return pool.SelectRandomWeightCount(weightGenerator, count, staticRandom.Value.NextDouble);
	}

	public static List<T> SelectRandomWeightCount<T>(this IList<T> pool, Func<T, float> weightGenerator, int count, Func<double> nextRandomGetter)
	{
		List<float> list = new List<float>();
		foreach (T item in pool)
		{
			list.Add(weightGenerator(item));
		}
		return pool.SelectRandomWeightCount(list, count, nextRandomGetter);
	}

	public static List<T> SelectRandomWeightCount<T>(this IList<T> pool, Func<int, float> weightGenerator, int count)
	{
		return pool.SelectRandomWeightCount(weightGenerator, count, staticRandom.Value.NextDouble);
	}

	public static List<T> SelectRandomWeightCount<T>(this IList<T> pool, Func<int, float> weightGenerator, int count, Func<double> nextRandomGetter)
	{
		List<float> list = new List<float>();
		for (int i = 0; i < pool.Count; i++)
		{
			list.Add(weightGenerator(i));
		}
		return pool.SelectRandomWeightCount(list, count, nextRandomGetter);
	}

	public static List<T> SelectRandomWeightCount<T>(this IList<T> pool, IList<float> weights, int count)
	{
		return pool.SelectRandomWeightCount(weights, count, staticRandom.Value.NextDouble);
	}

	public static List<T> SelectRandomWeightCount<T>(this IList<T> pool, IList<float> weights, int count, Func<double> nextRandomGetter)
	{
		if (pool.Count != weights.Count)
		{
			throw new Exception("pool count not equal to weights count");
		}
		float num = weights.Sum();
		List<T> list = new List<T>();
		for (int i = 0; i < count; i++)
		{
			float num2 = (float)nextRandomGetter() * num;
			float num3 = 0f;
			for (int j = 0; j < pool.Count; j++)
			{
				num3 += weights[j];
				if (num3 > num2)
				{
					list.Add(pool[j]);
					break;
				}
			}
		}
		return list;
	}

	public static List<T> SelectRandomWeightCountUnique<T>(this IList<T> pool, Func<T, float> weightGenerator, int count)
	{
		return pool.SelectRandomWeightCountUnique(weightGenerator, count, staticRandom.Value.NextDouble);
	}

	public static List<T> SelectRandomWeightCountUnique<T>(this IList<T> pool, Func<T, float> weightGenerator, int count, Func<double> nextRandomGetter)
	{
		List<float> list = new List<float>();
		foreach (T item in pool)
		{
			list.Add(weightGenerator(item));
		}
		return pool.SelectRandomWeightCountUnique(list, count, nextRandomGetter);
	}

	public static List<T> SelectRandomWeightCountUnique<T>(this IList<T> pool, Func<int, float> weightGenerator, int count)
	{
		return pool.SelectRandomWeightCountUnique(weightGenerator, count, staticRandom.Value.NextDouble);
	}

	public static List<T> SelectRandomWeightCountUnique<T>(this IList<T> pool, Func<int, float> weightGenerator, int count, Func<double> nextRandomGetter)
	{
		List<float> list = new List<float>();
		for (int i = 0; i < pool.Count; i++)
		{
			list.Add(weightGenerator(i));
		}
		return pool.SelectRandomWeightCountUnique(list, count, nextRandomGetter);
	}

	public static List<T> SelectRandomWeightCountUnique<T>(this IList<T> pool, IList<float> weights, int count)
	{
		return pool.SelectRandomWeightCountUnique(weights, count, staticRandom.Value.NextDouble);
	}

	public static List<T> SelectRandomWeightCountUnique<T>(this IList<T> pool, IList<float> weights, int count, Func<double> nextRandomGetter)
	{
		if (pool.Count != weights.Count)
		{
			throw new Exception("pool count not equal to weights count");
		}
		if (count >= pool.Count)
		{
			return new List<T>(pool);
		}
		List<(T, float)> list = new List<(T, float)>();
		for (int i = 0; i < pool.Count; i++)
		{
			list.Add((pool[i], weights[i]));
		}
		List<T> list2 = new List<T>();
		while (list2.Count < count)
		{
			float num = list.Sum(((T, float) t) => t.Item2);
			float num2 = (float)nextRandomGetter() * num;
			float num3 = 0f;
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				num3 += list[num4].Item2;
				if (num3 > num2)
				{
					list2.Add(list[num4].Item1);
					list.RemoveAt(num4);
					break;
				}
			}
		}
		return list2;
	}

	public static List<T> Shuffle<T>(this IList<T> pool, Func<double> nextRandomGetter = null)
	{
		if (nextRandomGetter == null)
		{
			nextRandomGetter = staticRandom.Value.NextDouble;
		}
		List<T> list = new List<T>(pool);
		List<T> list2 = new List<T>();
		while (list.Count > 0)
		{
			int index = (int)Math.Floor(nextRandomGetter() * (double)list.Count);
			list2.Add(list[index]);
			list.RemoveAt(index);
		}
		return list2;
	}

	public static List<int> DistributeSumToValues(int sum, int count, int min, int grad = 1, Func<double> nextRandomGetter = null)
	{
		if (nextRandomGetter == null)
		{
			nextRandomGetter = staticRandom.Value.NextDouble;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < count; i++)
		{
			list.Add(min);
		}
		for (sum -= min * count; sum > 0; sum -= grad)
		{
			list[(int)Math.Floor(nextRandomGetter() * (double)count)] += grad;
		}
		return list;
	}

	public static List<int> DistributeDeltaToValues(IReadOnlyList<int> values, int delta, Func<int, int> maxGetter, Func<double> nextRandomGetter)
	{
		if (nextRandomGetter == null)
		{
			nextRandomGetter = staticRandom.Value.NextDouble;
		}
		if (maxGetter == null)
		{
			maxGetter = (int i) => int.MaxValue;
		}
		if (MathHelper.SumOfDiffToMax(values, maxGetter) < delta)
		{
			throw new Exception("sum greater than deltaSum");
		}
		List<int> list = new List<int>(values);
		List<int> list2 = new List<int>();
		for (int num = 0; num < values.Count; num++)
		{
			if (values[num] < maxGetter(num))
			{
				list2.Add(num);
			}
		}
		while (delta > 0)
		{
			int num2 = list2[(int)Math.Floor(nextRandomGetter() * (double)list2.Count)];
			list[num2]++;
			delta--;
			if (list[num2] >= maxGetter(num2))
			{
				list2.Remove(num2);
			}
		}
		return list;
	}
}
