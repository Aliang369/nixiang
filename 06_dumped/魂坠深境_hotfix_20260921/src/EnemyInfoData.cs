using System.Collections.Generic;
using UnityEngine;

public class EnemyInfoData : AssetPreData<CharacterAIComponent>
{
	[LocalizeText("敌人名字-图鉴")]
	public string name;

	[LocalizeText("敌人描述-图鉴")]
	public string description;

	public bool globalSpawn;

	public bool boss;

	public bool obsolete;

	public bool developingFlag;

	public bool isEliteVariant;

	public DifficultyLevel unlockDifficulty;

	public bool removeForBanHao;

	public bool hideInIndex;

	public bool isPvpEnemy;

	public List<ItemDefinition> refItems;

	public List<CharacterAIComponent.EnemySpawnSetup> setups;

	public PVPEnemyAvatarConfig pvpEnemyAvatarConfig;

	public Sprite thumbnail;

	public Sprite avatar;

	public override string CTAssetIDPrepend => "ENEMYINFO";

	public override bool IsValidAsset()
	{
		if (obsolete)
		{
			return false;
		}
		if (!CSCommon.developingAssetFilter)
		{
			return true;
		}
		return !developingFlag;
	}
}
