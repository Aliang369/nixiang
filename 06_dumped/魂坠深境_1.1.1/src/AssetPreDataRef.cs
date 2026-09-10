using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public abstract class AssetPreDataRef<T, P> where T : AssetPreData<P> where P : UnityEngine.Object, ICTAsset
{
	[SaveField(null, null)]
	public string CTAssetId;

	public override string ToString()
	{
		return CTAssetId;
	}

	public T GetAssetPreData(AssetCatalog catalog)
	{
		return catalog.FullIdToCTAsset<T>(CTAssetId);
	}

	public bool IsValidRef(AssetCatalog catalog)
	{
		if (!CTAssetId.IsNullOrEmpty())
		{
			return GetAssetPreData(catalog)?.IsValidAsset() ?? false;
		}
		return false;
	}

	public AsyncOperationHandle StartLoad(AssetCatalog catalog)
	{
		T assetPreData = GetAssetPreData(catalog);
		if (assetPreData == null)
		{
			return default(AsyncOperationHandle);
		}
		return assetPreData.LoadAsync();
	}

	public void LoadSync(AssetCatalog catalog)
	{
		GetAssetPreData(catalog).LoadSync();
	}

	public bool IsLoaded(AssetCatalog catalog)
	{
		return GetAssetPreData(catalog)?.isLoaded ?? false;
	}

	public P GetLoadedAsset(AssetCatalog catalog)
	{
		return GetAssetPreData(catalog).loadedAsset;
	}

	public void Unload(AssetCatalog catalog)
	{
		GetAssetPreData(catalog).Unload();
	}

	protected abstract IList<T> GetAllAssetPreData(AssetCatalog catalog);
}
