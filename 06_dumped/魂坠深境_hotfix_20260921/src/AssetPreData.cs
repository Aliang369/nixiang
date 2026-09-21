using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class AssetPreData<T> : ILoadable, ICTAsset, IChildren<ICTAsset> where T : UnityEngine.Object, ICTAsset
{
	public int index;

	public string address;

	private static Dictionary<string, AssetReference> addressToRefDic = new Dictionary<string, AssetReference>();

	public virtual AssetReference TargetRef => GetGlobalAssetRef(address);

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
			return address;
		}
		set
		{
			address = value;
		}
	}

	public abstract string CTAssetIDPrepend { get; }

	public virtual Type CTAssetType => GetType();

	public string FullId => ((ICTAsset)this).GetFullID();

	public virtual T loadedAsset
	{
		get
		{
			GameObject obj = TargetRef.Asset as GameObject;
			if ((object)obj == null)
			{
				return null;
			}
			return obj.GetComponent<T>();
		}
	}

	public virtual bool isLoaded => TargetRef.Asset != null;

	public string ResourceId => TargetRef?.AssetGUID;

	protected static AssetReference GetGlobalAssetRef(string address)
	{
		if (address.IsNullOrEmpty())
		{
			return null;
		}
		if (addressToRefDic.TryGetValue(address, out var value))
		{
			return value;
		}
		AssetReference assetReference = new AssetReference(address);
		addressToRefDic.Add(address, assetReference);
		return assetReference;
	}

	public virtual AsyncOperationHandle<GameObject> LoadAsync()
	{
		return LoadAsync(TargetRef);
	}

	public T GetOrSyncLoadAsset()
	{
		if (!isLoaded)
		{
			LoadSync();
		}
		return loadedAsset;
	}

	public virtual void LoadSync()
	{
		LoadSync(TargetRef);
	}

	protected static void LoadSync(AssetReference assetRef)
	{
		if (!assetRef.IsValid())
		{
			assetRef.LoadAssetAsync<GameObject>().WaitForCompletion();
			if (ScriptableObjectSingleton<LocalizationManager>.instance.EnableLocalization)
			{
				LocalizeAsset(assetRef);
			}
			TraceLog.Warn($"同步加载资源{assetRef}");
		}
	}

	protected static AsyncOperationHandle<GameObject> LoadAsync(AssetReference assetRef)
	{
		if (assetRef.IsValid())
		{
			return assetRef.OperationHandle.Convert<GameObject>();
		}
		AsyncOperationHandle<GameObject> result = default(AsyncOperationHandle<GameObject>);
		try
		{
			result = assetRef.LoadAssetAsync<GameObject>();
			result.Completed += delegate
			{
				if (ScriptableObjectSingleton<LocalizationManager>.instance.EnableLocalization)
				{
					LocalizeAsset(assetRef);
				}
			};
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, $"资源加载错误：{assetRef}");
		}
		return result;
	}

	private static void LocalizeAsset(AssetReference assetRef)
	{
		GameObject obj = assetRef.Asset as GameObject;
		LocalizationManager.LocalizeTarget(((object)obj != null) ? obj.GetComponent<T>() : null, new TraverseLocalizeTermState(TraverseLocalizeTermState.EType.Translate));
	}

	public void Unload()
	{
		if (TargetRef.IsValid())
		{
			TargetRef.ReleaseAsset();
		}
	}

	public abstract bool IsValidAsset();

	public List<ICTAsset> GetChildren()
	{
		return null;
	}
}
