using System;
using Newtonsoft.Json;
using UnityEngine;

public class zbTableLoader
{
	private static zbTableLoader sInstance;

	public static zbTableLoader Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbTableLoader();
			}
			return sInstance;
		}
	}

	public T LoadConfig<T>(string filePath) where T : class
	{
		TextAsset val = zbAssetLoader.Instance.LoadResouce<TextAsset>(filePath);
		if (Object.op_Implicit((Object)(object)val))
		{
			return JsonConvert.DeserializeObject<T>(val.text);
		}
		return null;
	}

	public T LoadBinConfig<T>(string filePath, Func<byte[], T> readFunc) where T : class
	{
		TextAsset val = zbAssetLoader.Instance.LoadResouce<TextAsset>(filePath);
		if (Object.op_Implicit((Object)(object)val))
		{
			return readFunc(val.bytes);
		}
		return null;
	}

	public T LoadTableConfig<T>(string pathNoExt, Func<byte[], T> readFunc) where T : class
	{
		TextAsset val = zbAssetLoader.Instance.LoadResouce<TextAsset>(pathNoExt + ".bytes");
		if (Object.op_Implicit((Object)(object)val))
		{
			return readFunc(val.bytes);
		}
		return LoadConfig<T>(pathNoExt + ".json");
	}
}
