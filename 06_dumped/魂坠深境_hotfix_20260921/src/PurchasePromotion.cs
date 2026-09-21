public class PurchasePromotion
{
	[SaveField(null, null)]
	public string purchaseId;

	[SaveField(null, null)]
	public bool promoted;

	[SaveField(null, null)]
	public int promoteAfterXGame;

	public string PromotionId => $"Promotion_{purchaseId}_{promoteAfterXGame}";

	public bool IsPromotable(ApplicationStateInfo asi)
	{
		if (promoted)
		{
			return false;
		}
		RewardBundlePurchase purchaseInfo = GetPurchaseInfo(asi);
		if (purchaseInfo == null)
		{
			return false;
		}
		if (asi.purchaseManager.purchasedOrderHistory.ContainsKey(purchaseInfo.purchaseId))
		{
			return false;
		}
		if (promoteAfterXGame > 0 && promoteAfterXGame != asi.gameStatisticsManager.gameCount)
		{
			return false;
		}
		return true;
	}

	public PurchasePromotion Clone()
	{
		return MemberwiseClone() as PurchasePromotion;
	}

	public RewardBundlePurchase GetPurchaseInfo(ApplicationStateInfo asi)
	{
		return asi.purchaseManager.GetPurchaseInfo(purchaseId) as RewardBundlePurchase;
	}
}
