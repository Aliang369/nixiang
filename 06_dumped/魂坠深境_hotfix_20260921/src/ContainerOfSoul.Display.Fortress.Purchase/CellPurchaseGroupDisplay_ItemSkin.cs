using System.Collections;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.Purchase;

public class CellPurchaseGroupDisplay_ItemSkin : PurchaseGroupDefDisplay, IFrameUpdate
{
	[SerializeField]
	public PurchaseCell purchaseCell1;

	[SerializeField]
	public PurchaseCell purchaseCell2;

	[SerializeField]
	public PurchaseCell purchaseCell3;

	[SerializeField]
	public TextMeshProUGUI cleanedText;

	[SerializeField]
	public GameObject expireDateHintArea;

	[SerializeField]
	public TextMeshProUGUI expireDateText;

	[SerializeField]
	private Animator animator;

	private RealMoneyToMatterPurchase localOfferSourcePurchase;

	private CardSkinOfferShelfPurchase cardSkinOfferShelfPurchase;

	private bool? lastNetworkShelfReady;

	private PurchaseGroupDef purchaseGroup => base.setupData;

	protected override void Init()
	{
		expireDateHintArea.SetActive(value: false);
		InitUI(purchaseCell1);
		InitUI(purchaseCell2);
		InitUI(purchaseCell3);
		SetPurchaseCellTimersVisible(visible: false);
		purchaseCell1.SetInfoPanel(UIHelper.GetDefaultCommonInfoPanel());
		purchaseCell2.SetInfoPanel(UIHelper.GetDefaultCommonInfoPanel());
		purchaseCell3.SetInfoPanel(UIHelper.GetDefaultCommonInfoPanel());
	}

	protected override void OnSetup(PurchaseGroupDef _)
	{
		lastNetworkShelfReady = null;
		CellUI<PurchaseGroupDef>.fortressState.StartCoroutine(ShowCor);
	}

	public void FrameUpdate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			expireDateHintArea.SetActive(value: false);
			SetPurchaseCellTimersVisible(visible: false);
			DriveCardSkinOfferSelfHeal();
		}
	}

	private void DriveCardSkinOfferSelfHeal()
	{
		if (GlobalConfigManager.instance.enableNetwork && cardSkinOfferShelfPurchase != null)
		{
			CardSkinOfferController cardSkinOffers = CellUI<PurchaseGroupDef>.applicationState.purchaseManager.CardSkinOffers;
			cardSkinOffers.UpdateNetworkShelfIfNeeded();
			bool isNetworkShelfReady = cardSkinOffers.IsNetworkShelfReady;
			if (lastNetworkShelfReady != isNetworkShelfReady)
			{
				lastNetworkShelfReady = isNetworkShelfReady;
				purchaseCell2.SetDirty();
			}
		}
	}

	protected override void UpdateDisplay()
	{
		if (purchaseGroup == null)
		{
			return;
		}
		SetPurchaseCellTimersVisible(visible: false);
		ShuffleSkinPurchase shuffleSkinPurchase = purchaseGroup.purchases.OfType<ShuffleSkinPurchase>().FirstOrDefault();
		ShuffleSkinBundlePurchase shuffleSkinBundlePurchase = purchaseGroup.purchases.OfType<ShuffleSkinBundlePurchase>().FirstOrDefault();
		RealMoneyToMatterPurchase selectablePurchase = purchaseGroup.purchases.OfType<RealMoneyToMatterPurchase>().FirstOrDefault((RealMoneyToMatterPurchase purchase) => purchase.purchaseId == "0000302RealMoneyToMatterPurchase");
		purchaseCell1.gameObject.SetActive(shuffleSkinPurchase != null);
		purchaseCell2.gameObject.SetActive(selectablePurchase != null);
		purchaseCell3.gameObject.SetActive(shuffleSkinBundlePurchase != null);
		if (shuffleSkinPurchase != null)
		{
			purchaseCell1.Setup(shuffleSkinPurchase);
		}
		if (shuffleSkinBundlePurchase != null)
		{
			purchaseCell3.Setup(shuffleSkinBundlePurchase);
		}
		if (selectablePurchase != null)
		{
			if (cardSkinOfferShelfPurchase == null || localOfferSourcePurchase != selectablePurchase)
			{
				localOfferSourcePurchase = selectablePurchase;
				cardSkinOfferShelfPurchase = new CardSkinOfferShelfPurchase(selectablePurchase, CellUI<PurchaseGroupDef>.applicationState.purchaseManager.CardSkinOffers);
			}
			purchaseCell2.SetPurchaseButtonTextOverride(ScriptableObjectSingleton<TextDataPreset>.instance.term_view);
			purchaseCell2.SetGlobalItemInfoEnabled(enabled: false);
			purchaseCell2.SetCustomPurchaseAction(delegate
			{
				CardSkinOfferPanel cardSkinOfferPanel = CellUI<PurchaseGroupDef>.fortressState.uiManager.cardSkinOfferPanel;
				cardSkinOfferPanel.ConfigureRefreshCountdown(purchaseCell2.GetRefreshTimeDisplayTemplate());
				cardSkinOfferPanel.ShowUI(selectablePurchase);
			});
			purchaseCell2.Setup(cardSkinOfferShelfPurchase);
		}
		cleanedText.gameObject.SetActive(shuffleSkinPurchase == null && shuffleSkinBundlePurchase == null && selectablePurchase == null);
	}

	private void SetPurchaseCellTimersVisible(bool visible)
	{
		purchaseCell1.SetRefreshTimeVisible(visible);
		purchaseCell2.SetRefreshTimeVisible(visible);
		purchaseCell3.SetRefreshTimeVisible(visible);
	}

	private IEnumerator ShowCor()
	{
		base.gameObject.SetActive(value: true);
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.show);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
	}
}
