using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public abstract class CTPrefabAsset : SerializedMonoBehaviour, ICTAsset, IChildren<ICTAsset>
{
	public int assetIndex;

	public string assetGuid;

	public bool obsolete;

	public bool developingFlag;

	public bool removeForBanHao;

	public string FullId => ((ICTAsset)this).GetFullID();

	public string GetPaddingID => ((ICTAsset)this).GetPaddingID();

	public abstract string CTAssetIDPrepend { get; }

	public virtual Type CTAssetType => GetType();

	int ICTAsset.AssetIndex
	{
		get
		{
			return assetIndex;
		}
		set
		{
			assetIndex = value;
		}
	}

	string ICTAsset.AssetGuid
	{
		get
		{
			return assetGuid;
		}
		set
		{
			assetGuid = value;
		}
	}

	public virtual List<ICTAsset> GetChildren()
	{
		return null;
	}

	public bool IsValidAsset()
	{
		if (!obsolete)
		{
			if (CSCommon.developingAssetFilter)
			{
				return !developingFlag;
			}
			return true;
		}
		return false;
	}
}
