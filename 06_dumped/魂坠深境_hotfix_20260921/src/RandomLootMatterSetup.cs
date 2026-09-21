using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomLootMatterSetup : GlobalRewardSetup
{
	[SaveField(null, null)]
	public int count;

	public override int GlobalItemCount => count;

	public override Sprite GlobalItemCellIcon => ScriptableObjectSingleton<VFXDataPreset>.instance.icon_global_item_random_loot_matter;

	public override List<IGlobalItemDisplaySetup> RandomMatterDefs => Bootstrap.instance.assetCatalog.matterDefCatalog.ValidAssets.FindAll((MatterDef m) => m.IsMatterTypeEnemyLoot).ToList().ConvertAll((Converter<MatterDef, IGlobalItemDisplaySetup>)((MatterDef m) => new GlobalRewardInfo
	{
		matter = new MatterInfo(m, count)
	}));

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return base.GlobalItemStringConfig.GetGlobalItemDescription.FormatWith(count);
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		List<MatterInfo> list = new List<MatterInfo>();
		for (int i = 0; i < count; i++)
		{
			MatterDef matterDef = applicationState.matterManager.GenerateRandomLootMatter();
			list.Add(new MatterInfo(matterDef, 1));
		}
		list = MatterInfo.FoldMatterInfo(list);
		List<GlobalRewardInfo> list2 = new List<GlobalRewardInfo>();
		foreach (MatterInfo item in list)
		{
			GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo();
			globalRewardInfo.matter = item;
			list2.Add(globalRewardInfo);
		}
		return new GlobalRewardInfo(GlobalRewardInfo.RewardNestedType.RandomChest, list2);
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(count);
	}
}
