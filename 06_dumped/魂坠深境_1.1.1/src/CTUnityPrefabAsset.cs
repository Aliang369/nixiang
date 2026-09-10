using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CTUnityPrefabAsset : MonoBehaviour, ICTAsset, IChildren<ICTAsset>
{
	public int assetIndex;

	public string assetGuid;

	public bool obsolete;

	[TextArea]
	public string developerTip;

	public abstract string CTAssetIDPrepend { get; }

	public virtual Type CTAssetType => GetType();

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

	public virtual List<ICTAsset> GetChildren()
	{
		return null;
	}

	public bool IsValidAsset()
	{
		return !obsolete;
	}
}
