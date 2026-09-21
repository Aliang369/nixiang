using System;
using System.Collections.Generic;

[Serializable]
public class RoomRef : AssetPreDataRef<RoomInfoData, DungeonRoom>
{
	protected override IList<RoomInfoData> GetAllAssetPreData(AssetCatalog catalog)
	{
		return catalog.roomInfoDataCatalog.ValidAssets;
	}

	private RoomRef()
	{
	}

	public RoomRef(RoomInfoData roomInfoData)
	{
		CTAssetId = roomInfoData.FullId;
	}
}
