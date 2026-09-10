using UnityEngine;

public class UnlockPermanentVIPSetup : GlobalRewardSetup
{
	public override QualityLevel GlobalItemQuality => QualityLevel.Epic;

	public override Sprite GlobalItemCellIcon => ScriptableObjectSingleton<VFXDataPreset>.instance.icon_global_item_unlock_vip;

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			unlockPermanentVIP = true
		};
	}
}
