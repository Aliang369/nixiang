using System;
using System.Collections.Generic;

namespace GameRandom;

public static class DynamicRandomSelectorExtension
{
	public static DynamicRandomSelector<T> SetWeight<T>(this DynamicRandomSelector<T> source, Func<T, float> weightGenerator)
	{
		DynamicRandomSelector<T> dynamicRandomSelector = new DynamicRandomSelector<T>(source);
		List<float> list = dynamicRandomSelector.items.ConvertAll((RandomItem<T> t) => weightGenerator(t.value));
		for (int num = 0; num < dynamicRandomSelector.items.Count; num++)
		{
			RandomItem<T> value = dynamicRandomSelector.items[num];
			value.weight = list[num];
			dynamicRandomSelector.items[num] = value;
		}
		return dynamicRandomSelector;
	}

	public static DynamicRandomSelector<T> MinRepeatInterval<T>(this DynamicRandomSelector<T> source, Func<T, int> minIntervalGenerator)
	{
		return new DynamicRandomSelector<T>(source)
		{
			selectionAdjustors = { (Action<DynamicRandomSelector<T>>)delegate(DynamicRandomSelector<T> t)
			{
				RandomItem<T> item = default(RandomItem<T>);
				for (int i = 0; i < t.items.Count; i++)
				{
					item = t.items[i];
					int num = minIntervalGenerator(item.value);
					if (num >= 0)
					{
						int num2 = t.history.FindLastIndex((T b) => b.Equals(item.value));
						if (num2 != -1)
						{
							if (t.history.Count - num2 <= num)
							{
								item.dynamicWeight = 0f;
							}
							t.items[i] = item;
						}
					}
				}
			} }
		};
	}

	public static DynamicRandomSelector<T> MaxIntervalGurantee<T>(this DynamicRandomSelector<T> source, Func<T, int> maxIntervalGenerator)
	{
		return new DynamicRandomSelector<T>(source)
		{
			selectionAdjustors = { (Action<DynamicRandomSelector<T>>)delegate(DynamicRandomSelector<T> t)
			{
				RandomItem<T> item = default(RandomItem<T>);
				for (int i = 0; i < t.items.Count; i++)
				{
					item = t.items[i];
					int num = maxIntervalGenerator(item.value);
					if (num >= 0)
					{
						int num2 = t.history.FindLastIndex((T b) => b.Equals(item.value));
						if (t.history.Count - num2 >= num)
						{
							item.must = true;
							t.items[i] = item;
						}
					}
				}
			} }
		};
	}

	public static DynamicRandomSelector<T> MinSelectIndex<T>(this DynamicRandomSelector<T> source, Func<T, int> minIndexGenerator)
	{
		return new DynamicRandomSelector<T>(source)
		{
			selectionAdjustors = { (Action<DynamicRandomSelector<T>>)delegate(DynamicRandomSelector<T> t)
			{
				for (int i = 0; i < t.items.Count; i++)
				{
					RandomItem<T> value = t.items[i];
					int num = minIndexGenerator(value.value);
					if (num != -1)
					{
						if (t.history.Count < num)
						{
							value.dynamicWeight = 0f;
						}
						t.items[i] = value;
					}
				}
			} }
		};
	}

	public static DynamicRandomSelector<T> MaxSelectIndex<T>(this DynamicRandomSelector<T> source, Func<T, int> maxIndexGenerator)
	{
		return new DynamicRandomSelector<T>(source)
		{
			selectionAdjustors = { (Action<DynamicRandomSelector<T>>)delegate(DynamicRandomSelector<T> t)
			{
				for (int i = 0; i < t.items.Count; i++)
				{
					RandomItem<T> value = t.items[i];
					int num = maxIndexGenerator(value.value);
					if (num != -1)
					{
						if (t.history.Count >= num)
						{
							value.dynamicWeight = 0f;
						}
						t.items[i] = value;
					}
				}
			} }
		};
	}

	public static DynamicRandomSelector<T> MaxRepeatSequence<T>(this DynamicRandomSelector<T> source, Func<T, int> repeatCountGenerator)
	{
		return new DynamicRandomSelector<T>(source)
		{
			selectionAdjustors = { (Action<DynamicRandomSelector<T>>)delegate(DynamicRandomSelector<T> t)
			{
				for (int i = 0; i < t.items.Count; i++)
				{
					RandomItem<T> value = t.items[i];
					int num = repeatCountGenerator(value.value);
					bool flag = true;
					if (num < 0)
					{
						flag = false;
					}
					if (t.history.Count < num)
					{
						flag = false;
					}
					else
					{
						for (int j = 0; j < num; j++)
						{
							if (!t.history[t.history.Count - j - 1].Equals(value.value))
							{
								flag = false;
							}
						}
					}
					if (flag)
					{
						value.dynamicWeight = 0f;
					}
					t.items[i] = value;
				}
			} }
		};
	}
}
