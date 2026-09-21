using System.Collections.Generic;
using UnityEngine;

public class IdolDef : CTScriptableObject
{
	[CTSOAssetName]
	[LocalizeText("神像名称")]
	public string idolName;

	[SerializeField]
	[LocalizeText("神像描述")]
	public string idolDescription;

	[SerializeField]
	public CharacterClassType attachClass;

	[SerializeField]
	public Sprite idolIcon;

	[SerializeField]
	public Sprite classStatueSprite;

	[SerializeField]
	public List<IdolLevel> levels;

	public override string CTAssetIDPrepend => "IDOL";

	public int maxLevel => levels.Count;

	public string SoulSacrificeName => $"{attachClass}·魂祭";

	public IdolLevel GetLevel(int level)
	{
		int num = level - 1;
		if (num < 0 || num >= levels.Count)
		{
			return null;
		}
		return levels[num];
	}

	public List<GlobalRewardInfo> GetLevelReward(int level)
	{
		List<GlobalRewardSetup> reward = GetLevel(level).reward;
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		foreach (GlobalRewardSetup item2 in reward)
		{
			GlobalRewardInfo item = item2.GenerateReward(Bootstrap.applicationState, GlobalRewardSourceType.IdolUpgrade, base.FullID);
			list.Add(item);
		}
		return list;
	}

	public List<PlayerCharacterDef> CharacterDefs()
	{
		return Bootstrap.applicationState.assetCatalog.characterDefCatalog.ValidAssets.FindAll((PlayerCharacterDef c) => c.attachClass.mClass == attachClass);
	}

	public List<PlayerCharacterState> CharacterStates()
	{
		return Bootstrap.applicationState.characterManager.characterStates.FindAll((PlayerCharacterState c) => c.def.attachClass.mClass == attachClass);
	}
}
