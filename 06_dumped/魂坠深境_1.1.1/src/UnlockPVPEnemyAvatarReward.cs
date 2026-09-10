using UnityEngine;

public class UnlockPVPEnemyAvatarReward : GlobalRewardSetup
{
	[SaveField(null, null)]
	public EnemyAssetRef unlockEnemyAvatar;

	public override Sprite GlobalItemCellIcon => unlockEnemyAvatar.GetAssetPreData(Bootstrap.instance.assetCatalog).thumbnail;

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		return new GlobalRewardInfo
		{
			unlockPVPEnemyAvatar = unlockEnemyAvatar.GetAssetPreData(applicationState.assetCatalog)
		};
	}
}
