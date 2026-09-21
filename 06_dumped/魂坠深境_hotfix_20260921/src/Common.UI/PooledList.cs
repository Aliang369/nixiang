using System;

namespace Common.UI;

public class PooledList<T> where T : new()
{
	public T[] array;

	public int size;

	public PooledList(int capacity = 10)
	{
		array = Array.Empty<T>();
		Resize(capacity);
		size = 0;
	}

	public void Resize(int capacity)
	{
		if (array.Length >= capacity)
		{
			size = capacity;
			return;
		}
		Array.Resize(ref array, capacity);
		for (int i = size; i < capacity; i++)
		{
			array[i] = new T();
		}
		size = capacity;
	}

	public void Clear()
	{
		size = 0;
	}
}
