using System.Collections.Generic;

public class DraftMapInfoData : AssetPreData<DraftMap>
{
	[LocalizeText("地图开发者名字")]
	public string mapName;

	[LocalizeText("地图显示名字")]
	public string mapDisplayName;

	[LocalizeText("地图描述")]
	public string mapDescription;

	public bool isExtraMap;

	public DifficultyLevel minimumDifficultyLevel;

	public bool obsolete;

	public bool developingFlag;

	public bool removeForBanHao;

	public List<RoomRef> allRoomRefs;

	public override string CTAssetIDPrepend => "DRMAP";

	public override bool IsValidAsset()
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
