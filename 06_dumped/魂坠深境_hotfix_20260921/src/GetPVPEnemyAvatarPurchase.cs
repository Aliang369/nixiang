using System.Collections.Generic;
using UnityEngine;

public class GetPVPEnemyAvatarPurchase : PurchaseInfo
{
	[SaveField(null, null)]
	public EnemyAssetRef unlockEnemyAvatar;

	private AssetCatalog CurrentAssetCatalog => Bootstrap.instance.assetCatalog;

	public override string PurchaseTitleText => unlockEnemyAvatar.GetAssetPreData(CurrentAssetCatalog).name;

	public override Sprite GlobalItemCellIcon => unlockEnemyAvatar.GetAssetPreData(CurrentAssetCatalog).thumbnail;

	public override string GlobalItemTitle => unlockEnemyAvatar.GetAssetPreData(CurrentAssetCatalog).name;

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo();
		globalRewardInfo.unlockPVPEnemyAvatar = unlockEnemyAvatar.GetAssetPreData(applicationState.assetCatalog);
		return new List<GlobalRewardInfo> { globalRewardInfo };
	}
}
