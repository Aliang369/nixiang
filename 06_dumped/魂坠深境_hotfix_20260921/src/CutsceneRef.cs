using System;
using System.Collections.Generic;

[Serializable]
public class CutsceneRef : AssetPreDataRef<CutsceneInfoData, Cutscene>
{
	protected override IList<CutsceneInfoData> GetAllAssetPreData(AssetCatalog catalog)
	{
		return catalog.cutsceneCatalog.assets;
	}

	private CutsceneRef()
	{
	}

	public CutsceneRef(CutsceneInfoData cutsceneInfoData)
	{
		CTAssetId = cutsceneInfoData.FullId;
	}
}
