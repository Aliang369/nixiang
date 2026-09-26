using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Rand
{
	private static int _state;

	public static float value => (float)(uint)(NextInt() & 0xFFFFFF) / 16777215f;

	public static Vector2 insideUnitCircle
	{
		get
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			float num = value * ((float)Math.PI * 2f);
			float num2 = Mathf.Sqrt(value);
			float num3 = Mathf.Cos(num) * num2;
			float num4 = Mathf.Sin(num) * num2;
			return new Vector2(num3, num4);
		}
	}

	static Rand()
	{
		_state = 123456789;
		int num = (int)(DateTime.UtcNow.Ticks & 0xFFFFFFFFu);
		if (num == 0)
		{
			num = 1;
		}
		_state = num;
	}

	public static void InitState(int seed)
	{
		if (seed == 0)
		{
			seed = 1;
		}
		_state = seed;
	}

	public static int NextInt()
	{
		int state = _state;
		int num = state ^ (state << 13);
		int num2 = num ^ (num >> 17);
		return _state = num2 ^ (num2 << 5);
	}

	public static long NextLong()
	{
		long num = (uint)NextInt();
		ulong num2 = (uint)NextInt();
		return (num << 32) | (long)num2;
	}

	public static int Range(int min, int max)
	{
		if (max <= min)
		{
			return min;
		}
		return (int)((uint)NextInt() % (uint)(max - min)) + min;
	}

	public static float Range(float min, float max)
	{
		return min + value * (max - min);
	}

	public static int Next(int count)
	{
		if (count <= 1)
		{
			return 0;
		}
		return (int)((uint)NextInt() % (uint)count);
	}

	public static List<T> GetRandomItems<T>(T[] source, int count)
	{
		if (source == null)
		{
			return new List<T>();
		}
		if (count > source.Length)
		{
			return new List<T>();
		}
		if (count < 0)
		{
			return new List<T>();
		}
		T[] array = new T[source.Length];
		Array.Copy(source, array, source.Length);
		for (int num = array.Length - 1; num > array.Length - 1 - count; num--)
		{
			int num2 = Range(0, num + 1);
			T val = array[num];
			array[num] = array[num2];
			array[num2] = val;
		}
		List<T> list = new List<T>();
		for (int i = array.Length - count; i < array.Length; i++)
		{
			list.Add(array[i]);
		}
		return list;
	}

	public static string CaptureScreenshot()
	{
		string path = $"share_screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
		string text = Path.Combine(Application.temporaryCachePath, path);
		Texture2D val = null;
		try
		{
			val = ScreenCapture.CaptureScreenshotAsTexture();
			byte[] bytes = ImageConversion.EncodeToPNG(val);
			File.WriteAllBytes(text, bytes);
			return text;
		}
		finally
		{
			if ((Object)(object)val != (Object)null)
			{
				Object.Destroy((Object)(object)val);
			}
		}
	}
}
