using System;
using System.Collections.Generic;
using UnityEngine;

public class AssetDataCatalog<T> : IAssetCatalog where T : UnityEngine.Object, ICTAsset
{
	public List<T> assets;

	private Dictionary<int, T> dics;

	public List<T> ValidAssets
	{
		get
		{
			if (assets == null)
			{
				return new List<T>();
			}
			return assets.FindAll(Validate);
			static bool Validate(T asset)
			{
				if (asset == null)
				{
					return false;
				}
				try
				{
					return asset.IsValidAsset();
				}
				catch (Exception e)
				{
					TraceLog.Exception(e);
					return false;
				}
			}
		}
	}

	public T GetAsset(string guid)
	{
		List<T> list = assets;
		if (list == null)
		{
			return null;
		}
		return list.Find((T t) => t != null && t.AssetGuid == guid);
	}
}
