using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ScriptableObjectSingleton<T> : SerializedScriptableObject, ICTAsset, IChildren<ICTAsset> where T : ScriptableObjectSingleton<T>
{
	public int index;

	public string resourceId;

	private static T _instance;

	public int AssetIndex
	{
		get
		{
			return index;
		}
		set
		{
			index = value;
		}
	}

	public string AssetGuid
	{
		get
		{
			return resourceId;
		}
		set
		{
			resourceId = value;
		}
	}

	public static T instance
	{
		get
		{
			if (_instance == null && Bootstrap.instance?.assetCatalog != null)
			{
				_instance = Bootstrap.instance.assetCatalog.singletons.Find((ScriptableObject t) => t is T) as T;
			}
			return _instance;
		}
	}

	public string CTAssetIDPrepend => "SINGELTON";

	public Type CTAssetType => typeof(ScriptableObjectSingleton<>);

	public List<ICTAsset> GetChildren()
	{
		return null;
	}

	public bool IsValidAsset()
	{
		return true;
	}
}
