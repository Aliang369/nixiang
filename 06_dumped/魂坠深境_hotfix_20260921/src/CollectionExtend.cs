using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class CollectionExtend
{
	public static T Find<T>(this T[][] tDoubleArray, Predicate<T> predicate)
	{
		foreach (T[] array in tDoubleArray)
		{
			foreach (T val in array)
			{
				if (predicate(val))
				{
					return val;
				}
			}
		}
		return default(T);
	}

	public static T Find<T>(this T[] tArray, Predicate<T> predicate)
	{
		foreach (T val in tArray)
		{
			if (predicate(val))
			{
				return val;
			}
		}
		return default(T);
	}

	public static void Swap(this IList list, int indexA, int indexB)
	{
		object obj = list[indexB];
		object obj2 = list[indexA];
		object obj3 = (list[indexA] = obj);
		obj3 = (list[indexB] = obj2);
	}

	public static bool Contains<T>(this T[,] array, T item)
	{
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		foreach (T x in array)
		{
			if (equalityComparer.Equals(x, item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains<T>(this T[] array, T item)
	{
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		foreach (T x in array)
		{
			if (equalityComparer.Equals(x, item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Contains<T>(this List<T> list, Func<T, bool> pref)
	{
		if (list == null || list.Count == 0 || pref == null)
		{
			return false;
		}
		foreach (T item in list)
		{
			if (pref(item))
			{
				return true;
			}
		}
		return false;
	}

	public static T Find<T>(this T[,] array, Predicate<T> match)
	{
		if (match == null)
		{
			throw new Exception("match is null");
		}
		foreach (T val in array)
		{
			if (match(val))
			{
				return val;
			}
		}
		return default(T);
	}

	public static T Find<T>(this IReadOnlyCollection<T> collection, Predicate<T> match)
	{
		if (match == null)
		{
			throw new Exception("match is null");
		}
		foreach (T item in collection)
		{
			if (match(item))
			{
				return item;
			}
		}
		return default(T);
	}

	public static T[] FindAll<T>(this T[] array, Predicate<T> match)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		int num = 0;
		T[] array2 = Array.Empty<T>();
		for (int i = 0; i < array.Length; i++)
		{
			if (match(array[i]))
			{
				if (num == array2.Length)
				{
					Array.Resize(ref array2, Math.Min((num == 0) ? 4 : (num * 2), array.Length));
				}
				array2[num++] = array[i];
			}
		}
		if (num != array2.Length)
		{
			Array.Resize(ref array2, num);
		}
		return array2;
	}

	public static int IndexOf<T>(this T[] array, T item)
	{
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		for (int i = 0; i < array.Length; i++)
		{
			if (equalityComparer.Equals(array[i], item))
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOf<T>(this LinkedList<T> list, T item)
	{
		int num = 0;
		foreach (T item2 in list)
		{
			if (item2.Equals(item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static void AddUnique<T>(this List<T> list, T item)
	{
		if (list != null && !list.Contains(item))
		{
			list.Add(item);
		}
	}

	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source)
	{
		return source.Where((T x) => x != null);
	}

	public static TVALUE TryGetValue<TKEY, TVALUE>(this Dictionary<TKEY, TVALUE> dic, TKEY key, TVALUE defaultValue = default(TVALUE))
	{
		return dic.GetValueOrDefault(key, defaultValue);
	}

	public static KeyValuePair<K, V> TryFindPair<K, V>(this Dictionary<K, V> dictionary, Predicate<(K, V)> match)
	{
		foreach (var (val3, val4) in dictionary)
		{
			if (match((val3, val4)))
			{
				return new KeyValuePair<K, V>(val3, val4);
			}
		}
		return default(KeyValuePair<K, V>);
	}

	public static List<T> GetEnumValues<T>() where T : Enum
	{
		return Enum.GetValues(typeof(T)).Cast<T>().ToList();
	}

	public static T[] Remove<T>(this T[] ts, T obj)
	{
		List<T> list = ts.ToList();
		list.Remove(obj);
		return list.ToArray();
	}

	public static TOutput[] ConvertAll<TInput, TOutput>(this TInput[] array, Converter<TInput, TOutput> converter)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (converter == null)
		{
			throw new ArgumentNullException("converter");
		}
		TOutput[] array2 = new TOutput[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = converter(array[i]);
		}
		return array2;
	}

	public static bool IsEmptyOrNull<T>(this IReadOnlyCollection<T> list)
	{
		if (list != null)
		{
			return list.Count == 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty<T>(this ICollection<T> source)
	{
		if (source != null)
		{
			return source.Count == 0;
		}
		return true;
	}

	public static List<T> Copy<T>(this List<T> source)
	{
		return new List<T>(source);
	}

	public static T FindOrAdd<T>(this List<T> list, Predicate<T> match, Func<T> add)
	{
		if (list == null)
		{
			return default(T);
		}
		int num = list.FindIndex(match);
		if (num == -1)
		{
			T val = add();
			list.Add(val);
			return val;
		}
		return list[num];
	}

	public static string ToString<T>(this List<T> list, Func<T, string> fun = null)
	{
		string text = "";
		if (list.IsNullOrEmpty())
		{
			return text;
		}
		foreach (T item in list)
		{
			text = ((fun == null) ? (text + "[" + item.ToString() + "]") : (text + "[" + fun(item) + "]"));
		}
		return text;
	}

	public static void Change<T>(this Dictionary<T, int> dictionary, T key, int delta)
	{
		if (!dictionary.ContainsKey(key))
		{
			dictionary.Add(key, 0);
		}
		dictionary[key] += delta;
		if (dictionary[key] == 0)
		{
			dictionary.Remove(key);
		}
	}
}
