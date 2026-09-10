using UnityEngine;

public class UnlockMonthlyCardSetup : GlobalRewardSetup
{
	public override QualityLevel GlobalItemQuality => QualityLevel.Epic;

	public override Sprite GlobalItemCellIcon => ScriptableObjectSingleton<VFXDataPreset>.instance.icon_global_item_unlock_monthly_card;

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			unlockMonthlyCard = true
		};
	}
}
