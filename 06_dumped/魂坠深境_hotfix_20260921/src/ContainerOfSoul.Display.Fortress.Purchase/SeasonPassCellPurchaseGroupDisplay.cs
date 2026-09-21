using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.Purchase;

public class SeasonPassCellPurchaseGroupDisplay : PurchaseGroupDefDisplay
{
	public SeasonPassPurchaseCell unlockSeasonPassPurchaseCell;

	public SeasonPassPurchaseCell unlockHighSeasonPassPurchaseCell;

	[SerializeField]
	private Animator animator;

	protected override void Init()
	{
		InitUI(unlockSeasonPassPurchaseCell);
		InitUI(unlockHighSeasonPassPurchaseCell);
	}

	protected override void OnSetup(PurchaseGroupDef setupData)
	{
		base.OnSetup(setupData);
		CellUI<PurchaseGroupDef>.fortressState.StartCoroutine(ShowCor);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		List<PurchaseInfo> purchases = base.setupData.purchases;
		PurchaseInfo purchaseInfo = purchases.Find(IsUnlockBaseSeasonPassPurchase);
		PurchaseInfo purchaseInfo2 = purchases.Find(IsUpgradeToHighSeasonPassPurchase);
		PurchaseInfo purchaseInfo3 = purchases.Find(IsUnlockHighSeasonPassPurchase);
		unlockSeasonPassPurchaseCell.Setup(purchaseInfo);
		if (CellUI<PurchaseGroupDef>.applicationState.seasonManager.currentSeason.seasonPassUnlocked && !CellUI<PurchaseGroupDef>.applicationState.seasonManager.currentSeason.highseasonPassUnlocked)
		{
			unlockHighSeasonPassPurchaseCell.Setup(purchaseInfo2);
		}
		else
		{
			unlockHighSeasonPassPurchaseCell.Setup(purchaseInfo3);
		}
	}

	private static bool IsUnlockBaseSeasonPassPurchase(PurchaseInfo purchase)
	{
		if (HasSeasonPassReward<UnlockSeasonPassSetup>(purchase))
		{
			return !HasSeasonPassReward<UnlockHighSeasonPassSetup>(purchase);
		}
		return false;
	}

	private static bool IsUnlockHighSeasonPassPurchase(PurchaseInfo purchase)
	{
		if (HasSeasonPassReward<UnlockSeasonPassSetup>(purchase))
		{
			return HasSeasonPassReward<UnlockHighSeasonPassSetup>(purchase);
		}
		return false;
	}

	private static bool IsUpgradeToHighSeasonPassPurchase(PurchaseInfo purchase)
	{
		if (!HasSeasonPassReward<UnlockSeasonPassSetup>(purchase))
		{
			return HasSeasonPassReward<UnlockHighSeasonPassSetup>(purchase);
		}
		return false;
	}

	private static bool HasSeasonPassReward<T>(PurchaseInfo purchase) where T : GlobalRewardSetup
	{
		if (!(purchase is RewardBundlePurchase rewardBundlePurchase) || rewardBundlePurchase.rewardBundle?.rewards == null)
		{
			return false;
		}
		return rewardBundlePurchase.rewardBundle.rewards.Exists((GlobalRewardSetup r) => r is T);
	}

	private IEnumerator ShowCor()
	{
		base.gameObject.SetActive(value: true);
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.show);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
	}
}
