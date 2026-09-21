using System;
using System.Collections.Generic;

[Serializable]
public class DraftMapRef : AssetPreDataRef<DraftMapInfoData, DraftMap>
{
	protected override IList<DraftMapInfoData> GetAllAssetPreData(AssetCatalog catalog)
	{
		return catalog.draftMapCatalog.assets;
	}

	public DraftMapRef()
	{
	}

	public DraftMapRef(DraftMapInfoData info)
	{
		CTAssetId = info.FullId;
	}
}
