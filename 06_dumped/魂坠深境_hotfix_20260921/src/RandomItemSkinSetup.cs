using System.Collections.Generic;
using UnityEngine;

public class RandomItemSkinSetup : GlobalRewardSetup
{
	public int count = 1;

	public QualityLevel filterQuality = QualityLevel.None;

	public override Sprite GlobalItemCellIcon => ScriptableObjectSingleton<VFXDataPreset>.instance.icon_global_item_random_skin;

	public override int GlobalItemCount => count;

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return base.GlobalItemStringConfig.GetGlobalItemDescription.FormatWith(count);
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		for (int i = 0; i < count; i++)
		{
			ItemSkinDef unlockItemSkin = applicationState.itemSkinManager.DrawRandomSkin(filterQuality);
			GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo();
			globalRewardInfo.unlockItemSkin = unlockItemSkin;
			list.Add(globalRewardInfo);
		}
		return new GlobalRewardInfo(GlobalRewardInfo.RewardNestedType.None, list);
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(count);
	}

	public RandomItemSkinSetup()
	{
		count = 1;
		filterQuality = QualityLevel.None;
	}
}
