public sealed class CardSkinOfferShelfPurchase : RealMoneyToMatterPurchase
{
	private readonly CardSkinOfferController controller;

	public override string PurchaseTitleText => ScriptableObjectSingleton<TextDataPreset>.instance.term_card_skin_choice;

	public CardSkinOfferShelfPurchase(RealMoneyToMatterPurchase sourcePurchase, CardSkinOfferController controller)
		: base(sourcePurchase.realMoneyCost, sourcePurchase.gain)
	{
		this.controller = controller;
		purchaseId = sourcePurchase.purchaseId;
		mattercost = sourcePurchase.mattercost;
		discount = sourcePurchase.discount;
		buyTimesLimit = sourcePurchase.buyTimesLimit;
		repeatBuyTimes = sourcePurchase.repeatBuyTimes;
		discountRepeatBuyTimes = sourcePurchase.discountRepeatBuyTimes;
		resetTimesType = sourcePurchase.resetTimesType;
		customResetDays = sourcePurchase.customResetDays;
		resetDay = sourcePurchase.resetDay;
		hideRangeSliderArea = sourcePurchase.hideRangeSliderArea;
		bonusCount = sourcePurchase.bonusCount;
		firstPurchaseDouble = sourcePurchase.firstPurchaseDouble;
		useMatterLootAfterPurchaseInsteadShow = sourcePurchase.useMatterLootAfterPurchaseInsteadShow;
		overrideSprite = sourcePurchase.overrideSprite;
	}

	public override bool IsAllowPurchase(ApplicationStateInfo applicationState)
	{
		if (!controller.HasAnyOffer())
		{
			return false;
		}
		return base.IsAllowPurchase(applicationState);
	}
}
