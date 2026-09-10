using System;
using System.Collections.Generic;

public abstract class CTUIBaseAsset : UIBase, ICTAsset, IChildren<ICTAsset>
{
	public int assetIndex;

	public string assetGuid;

	public bool developingFlag;

	public bool obsolete;

	public bool removeForBanHao;

	public int AssetIndex
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

	public string AssetGuid
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

	public string FullId => ((ICTAsset)this).GetFullID();

	public abstract string CTAssetIDPrepend { get; }

	public virtual Type CTAssetType => GetType();

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

	public List<ICTAsset> GetChildren()
	{
		return null;
	}
}
