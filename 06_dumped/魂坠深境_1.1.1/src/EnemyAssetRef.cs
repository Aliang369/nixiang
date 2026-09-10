using System;
using System.Collections.Generic;

[Serializable]
public class EnemyAssetRef : AssetPreDataRef<EnemyInfoData, CharacterAIComponent>
{
	protected override IList<EnemyInfoData> GetAllAssetPreData(AssetCatalog catalog)
	{
		return catalog.enemyInfoDataCatalog.assets;
	}

	private EnemyAssetRef()
	{
	}

	public EnemyAssetRef(EnemyInfoData enemyInfoData)
	{
		CTAssetId = enemyInfoData.FullId;
	}
}
