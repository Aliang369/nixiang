using System.Collections.Generic;
using UnityEngine;

public sealed class AssetPreDataCatalog<INFO, TYPE> : IAssetCatalog where INFO : AssetPreData<TYPE>, new() where TYPE : Object, ICTAsset
{
	public List<INFO> assets;

	public List<INFO> ValidAssets => assets.FindAll((INFO a) => a.IsValidAsset());

	public INFO GetInfo(TYPE obj)
	{
		foreach (INFO asset in assets)
		{
			if (asset.ResourceId == obj.AssetGuid)
			{
				return asset;
			}
		}
		return null;
	}

	public INFO GetAssetInfo(TYPE asset)
	{
		return assets.Find((INFO t) => t.ResourceId == asset.AssetGuid);
	}

	public INFO GetAssetInfo(string assetId)
	{
		return assets.Find((INFO t) => t.ResourceId == assetId);
	}

	public void UnloadAll()
	{
		foreach (INFO asset in assets)
		{
			asset.Unload();
		}
	}
}
