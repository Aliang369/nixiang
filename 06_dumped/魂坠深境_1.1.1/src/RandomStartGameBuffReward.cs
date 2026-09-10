using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class RandomStartGameBuffReward : GlobalRewardSetup
{
	public int count;

	public int lifetimeDays = 7;

	public QualityLevel qualityLevel = QualityLevel.None;

	private List<StartGameBuffDef> StartGameBuffRandomPool => Bootstrap.instance.assetCatalog.startGameBuffCatalog.ValidAssets.Where((StartGameBuffDef b) => qualityLevel == QualityLevel.None || b.quality == qualityLevel).ToList();

	public override Sprite GlobalItemCellIcon => ScriptableObjectSingleton<VFXDataPreset>.instance.icon_global_item_random_start_game_buff;

	public override int GlobalItemCount => count;

	public override QualityLevel GlobalItemQuality
	{
		get
		{
			if (qualityLevel == QualityLevel.None)
			{
				if (StartGameBuffRandomPool.Count <= 0)
				{
					return QualityLevel.Common;
				}
				return StartGameBuffRandomPool.Max((StartGameBuffDef b) => b.quality);
			}
			return qualityLevel;
		}
	}

	public override List<IGlobalItemDisplaySetup> RandomStartGameBuffs => StartGameBuffRandomPool.ConvertAll((Converter<StartGameBuffDef, IGlobalItemDisplaySetup>)((StartGameBuffDef b) => new GlobalRewardInfo
	{
		getStartGameBuff = StartGameBuff.CreateBuffOfLifeTime(b, lifetimeDays)
	}));

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		List<GlobalRewardInfo> nestedRewards = StartGameBuffRandomPool.SelectRandomCount(count).ConvertAll((StartGameBuffDef buffDef) => StartGameBuff.CreateBuffOfLifeTime(buffDef, lifetimeDays)).ConvertAll((StartGameBuff buff) => new GlobalRewardInfo
		{
			getStartGameBuff = buff
		});
		return new GlobalRewardInfo(GlobalRewardInfo.RewardNestedType.RandomChest, nestedRewards);
	}

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		string text = ((qualityLevel == QualityLevel.None) ? "" : ScriptableObjectSingleton<TextDataPreset>.instance.GetQualityLevelText(qualityLevel, withColor: true));
		return base.GlobalItemStringConfig.GetGlobalItemDescription.FormatWith(text, count);
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief((qualityLevel == QualityLevel.None) ? "" : ((object)qualityLevel), count);
	}
}
