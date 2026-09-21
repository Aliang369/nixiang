using System;
using System.Collections.Generic;

namespace GameRandom;

public class DynamicRandomSelector<T>
{
	private static readonly Lazy<Random> staticRandom = new Lazy<Random>(() => new Random());

	public List<T> history = new List<T>();

	public List<RandomItem<T>> items;

	public Func<double> randomValueGetter;

	public List<Action<DynamicRandomSelector<T>>> selectionAdjustors = new List<Action<DynamicRandomSelector<T>>>();

	public DynamicRandomSelector(IList<T> items, IList<float> initWeights)
	{
		this.items = new List<RandomItem<T>>();
		for (int i = 0; i < items.Count; i++)
		{
			this.items.Add(new RandomItem<T>
			{
				value = items[i],
				weight = initWeights[i]
			});
		}
	}

	public DynamicRandomSelector(IList<T> items)
	{
		this.items = new List<RandomItem<T>>();
		foreach (T item in items)
		{
			this.items.Add(new RandomItem<T>
			{
				value = item,
				weight = 1f
			});
		}
	}

	public DynamicRandomSelector(DynamicRandomSelector<T> t)
	{
		items = new List<RandomItem<T>>(t.items);
		history = new List<T>(t.history);
		if (t.selectionAdjustors != null)
		{
			selectionAdjustors = new List<Action<DynamicRandomSelector<T>>>(t.selectionAdjustors);
		}
	}

	private void ResetDynamicWeight()
	{
		for (int i = 0; i < items.Count; i++)
		{
			RandomItem<T> value = items[i];
			value.ResetDynamicValue();
			items[i] = value;
		}
		foreach (Action<DynamicRandomSelector<T>> selectionAdjustor in selectionAdjustors)
		{
			selectionAdjustor?.Invoke(this);
		}
	}

	public T SelectRandom()
	{
		ResetDynamicWeight();
		foreach (RandomItem<T> item in items)
		{
			if (item.must)
			{
				return SelectItemOut(item.value);
			}
		}
		return SelectItemOut(items.SelectRandomWeight((RandomItem<T> t) => t.dynamicWeight, randomValueGetter ?? new Func<double>(staticRandom.Value.NextDouble)).value);
	}

	public List<T> SelectRandomCount(int count)
	{
		List<T> list = new List<T>();
		for (int i = 0; i < count; i++)
		{
			list.Add(SelectRandom());
		}
		return list;
	}

	private T SelectItemOut(T item)
	{
		history.Add(item);
		return item;
	}

	public void ResetHistory()
	{
		history.Clear();
	}

	public T ForceSelectValueOut(T value)
	{
		return SelectItemOut(value);
	}
}
