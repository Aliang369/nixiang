using UnityEngine;

public class GachaGlobalReward : GlobalRewardSetup
{
	public int _count;

	public GachaPoolDef gachaPoolDef;

	public override string GlobalItemCellName => FGlobalItemCellName(_count);

	public override int GlobalItemCount => _count;

	public override Sprite GlobalItemCellIcon => ScriptableObjectSingleton<VFXDataPreset>.instance.icon_global_item_gacha_draw_stone;

	public override bool JumpToGachaAnnouncement => true;

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return base.GlobalItemStringConfig.GetGlobalItemDescription.FormatWith(_count);
	}

	private GachaGlobalReward()
	{
	}

	public GachaGlobalReward(int count)
	{
		_count = count;
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		GachaPoolState gachaPoolState = applicationState.gachaManager.GetGachaPoolState(gachaPoolDef ?? ScriptableObjectSingleton<FortressDataManager>.instance.baseGachaPoolDef);
		GlobalRewardPackage globalRewardPackage = applicationState.gachaManager.DrawXWithoutCost(gachaPoolState, _count);
		return new GlobalRewardInfo(GlobalRewardInfo.RewardNestedType.GachaDraw, globalRewardPackage.rewards);
	}

	public override string GetRewardBrief()
	{
		return FGetRewardBrief(_count);
	}
}
