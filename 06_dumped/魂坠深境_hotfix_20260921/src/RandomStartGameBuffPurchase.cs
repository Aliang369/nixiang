using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class RandomStartGameBuffPurchase : PurchaseInfo
{
	public QualityLevel filterQuality;

	public int lifeTimeDays = 7;

	public override bool SupportBatchPurchase => true;

	public List<StartGameBuffDef> StartGameBuffRandomPool => Bootstrap.instance.assetCatalog.startGameBuffCatalog.ValidAssets.Where((StartGameBuffDef b) => filterQuality == QualityLevel.None || b.quality == filterQuality).ToList();

	public override Sprite GlobalItemCellIcon => ScriptableObjectSingleton<VFXDataPreset>.instance.icon_global_item_random_start_game_buff;

	public override string PurchaseTitleText => PurchaseInfo.textData.term_random_startGameBuff;

	public override QualityLevel GlobalItemQuality
	{
		get
		{
			if (filterQuality == QualityLevel.None)
			{
				if (StartGameBuffRandomPool.Count <= 0)
				{
					return QualityLevel.Common;
				}
				return StartGameBuffRandomPool.Max((StartGameBuffDef b) => b.quality);
			}
			return filterQuality;
		}
	}

	public override List<IGlobalItemDisplaySetup> RandomStartGameBuffs => StartGameBuffRandomPool.ConvertAll((Converter<StartGameBuffDef, IGlobalItemDisplaySetup>)((StartGameBuffDef b) => new GlobalRewardInfo
	{
		getStartGameBuff = StartGameBuff.CreateBuffOfLifeTime(b, lifeTimeDays)
	}));

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return PurchaseInfo.textData.term_get_random_startGameBuff_can_uase_and_within_arg0_days.FormatWith(lifeTimeDays);
	}

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		for (int i = 0; i < purchaseTimes; i++)
		{
			StartGameBuff getStartGameBuff = StartGameBuff.CreateBuffOfLifeTime(StartGameBuffRandomPool.SelectRandom(), lifeTimeDays);
			list.Add(new GlobalRewardInfo
			{
				getStartGameBuff = getStartGameBuff
			});
		}
		return list;
	}
}
