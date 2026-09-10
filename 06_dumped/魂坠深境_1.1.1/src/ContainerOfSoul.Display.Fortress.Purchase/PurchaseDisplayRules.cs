namespace ContainerOfSoul.Display.Fortress.Purchase;

internal static class PurchaseDisplayRules
{
	private const string DailyAdGiftPurchaseId = "0000010WatchAdGetMatterPurchase";

	public static bool IsDailyAdGiftPurchase(PurchaseInfo purchase)
	{
		if (purchase is WatchAdGetMatterPurchase)
		{
			return purchase.purchaseId == "0000010WatchAdGetMatterPurchase";
		}
		return false;
	}
}
