using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSkinOfferPanel : CellUI<RealMoneyToMatterPurchase>, ISelectablePanel, IFrameUpdate
{
	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private TextMeshProUGUI confirmButtonText;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private CellUIPrefabPoolConfig<CardSkinOfferCell, CardSkinOfferSlot> pool;

	private GameButton refreshButton;

	private TextMeshProUGUI refreshButtonText;

	private MatterCell refreshCostCell;

	private CardSkinOfferSlot selectedSlot;

	private bool playRefreshVfxOnNextSync;

	private GameObject refreshCountdownArea;

	private TextMeshProUGUI refreshCountdownText;

	private int observedDailyPeriod = int.MinValue;

	private long displayedCountdownSecond = long.MinValue;

	private bool purchaseInProgress;

	private CardSkinOfferController controller => CellUI<RealMoneyToMatterPurchase>.applicationState.purchaseManager.CardSkinOffers;

	public RealMoneyToMatterPurchase SourcePurchase => base.setupData;

	internal bool ShouldPlayRefreshVfx => playRefreshVfxOnNextSync;

	protected override void Init()
	{
		Init(pool);
		backButton.onClick.AddListener(((CellUI<RealMoneyToMatterPurchase>)this).HideUI<RealMoneyToMatterPurchase>);
		confirmButton.onClick.AddListener(OnConfirmClicked);
		CreateRefreshButton();
		controller.StateChanged += OnOfferStateChanged;
	}

	protected override void OnSetup(RealMoneyToMatterPurchase purchase)
	{
		if (purchase == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		selectedSlot = null;
		playRefreshVfxOnNextSync = false;
		purchaseInProgress = false;
		controller.EnsureValidOffers();
		observedDailyPeriod = controller.State.dailyPeriod;
		displayedCountdownSecond = long.MinValue;
		if (GlobalConfigManager.instance.enableNetwork)
		{
			controller.RequestNetworkOffersIfNeeded();
		}
		SetDirty();
	}

	protected override void OnUnset()
	{
		selectedSlot = null;
		playRefreshVfxOnNextSync = false;
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		if (SourcePurchase != null)
		{
			titleText.text = CellUI<RealMoneyToMatterPurchase>.textData.term_current_card_skin_offer;
			SetButtonText(confirmButton, GetPurchasePriceText());
			controller.EnsureValidOffers();
			CardSkinOfferSlot cardSkinOfferSlot = selectedSlot;
			if (cardSkinOfferSlot == null || cardSkinOfferSlot.soldOut || !controller.State.slots.Contains(selectedSlot))
			{
				selectedSlot = null;
			}
			pool.DespawnAll();
			pool.Sync(controller.State.slots);
			playRefreshVfxOnNextSync = false;
			MatterInfo refreshCost = controller.GetRefreshCost();
			refreshButtonText.text = CellUI<RealMoneyToMatterPurchase>.textData.term_refresh;
			refreshCostCell.gameObject.SetActive(refreshCost != null);
			if (refreshCost != null)
			{
				refreshCostCell.Setup(refreshCost);
			}
			refreshButton.interactable = !purchaseInProgress && !controller.IsRequestInProgress && controller.CanRefresh() && (refreshCost?.IsPayable(CellUI<RealMoneyToMatterPurchase>.applicationState) ?? false);
			confirmButton.interactable = CanPurchaseSelectedSlot();
			UpdateRefreshCountdown(force: true);
		}
	}

	public void FrameUpdate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			controller.EnsureValidOffers();
			controller.UpdateNetworkShelfIfNeeded();
			if (GlobalConfigManager.instance.enableNetwork && !controller.IsNetworkShelfReady && selectedSlot != null)
			{
				selectedSlot = null;
				SetDirty();
			}
			int dailyPeriod = controller.State.dailyPeriod;
			if (observedDailyPeriod != dailyPeriod)
			{
				observedDailyPeriod = dailyPeriod;
				selectedSlot = null;
				playRefreshVfxOnNextSync = false;
				SetDirty();
				CellUI<RealMoneyToMatterPurchase>.fortressState.uiManager.purchaseUIPanel.SetDirty();
			}
			UpdateRefreshCountdown(force: false);
		}
	}

	public void ConfigureRefreshCountdown(GameObject template)
	{
		if (!(refreshCountdownArea != null) && !(template == null) && titleText.transform.parent is RectTransform rectTransform)
		{
			refreshCountdownArea = UnityEngine.Object.Instantiate(template, rectTransform);
			refreshCountdownArea.name = "CardSkinOfferRefreshCountdown";
			RectTransform obj = refreshCountdownArea.transform as RectTransform;
			obj.anchorMin = new Vector2(0.5f, 0.5f);
			obj.anchorMax = new Vector2(0.5f, 0.5f);
			obj.pivot = new Vector2(0f, 0.5f);
			obj.anchoredPosition = new Vector2(-690f, 294f);
			obj.localScale = Vector3.one;
			refreshCountdownText = refreshCountdownArea.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
			if (refreshCountdownText != null)
			{
				TextMeshProUGUI textMeshProUGUI = UnityEngine.Object.Instantiate(refreshCountdownText, refreshCountdownArea.transform);
				textMeshProUGUI.name = "RefreshCountdownLabel";
				textMeshProUGUI.text = CellUI<RealMoneyToMatterPurchase>.textData.term_refresh_countdown;
				textMeshProUGUI.transform.SetAsFirstSibling();
				textMeshProUGUI.raycastTarget = false;
			}
			Graphic[] componentsInChildren = refreshCountdownArea.GetComponentsInChildren<Graphic>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].raycastTarget = false;
			}
			refreshCountdownArea.SetActive(value: true);
			displayedCountdownSecond = long.MinValue;
			UpdateRefreshCountdown(force: true);
		}
	}

	private void UpdateRefreshCountdown(bool force)
	{
		if (refreshCountdownText == null)
		{
			return;
		}
		DateTime nextDailyRefreshAtUtc = controller.State.nextDailyRefreshAtUtc;
		if (!(nextDailyRefreshAtUtc == default(DateTime)))
		{
			TimeSpan timeSpan = nextDailyRefreshAtUtc - controller.CurrentUtcTime;
			if (timeSpan < TimeSpan.Zero)
			{
				timeSpan = TimeSpan.Zero;
			}
			long num = (long)Math.Ceiling(timeSpan.TotalSeconds);
			if (force || displayedCountdownSecond != num)
			{
				displayedCountdownSecond = num;
				refreshCountdownText.text = TextDisplayHelper.GetTimeSpanText(timeSpan);
			}
		}
	}

	private bool CanPurchaseSelectedSlot()
	{
		RealMoneyToMatterPurchase selectedPurchase = GetSelectedPurchase();
		if (purchaseInProgress || controller.IsRequestInProgress || !controller.CanPurchaseOffer(selectedSlot) || selectedPurchase == null)
		{
			return false;
		}
		if (selectedPurchase.IsAllowPurchase(CellUI<RealMoneyToMatterPurchase>.applicationState))
		{
			return selectedPurchase.IsEnoughCost(CellUI<RealMoneyToMatterPurchase>.applicationState);
		}
		return false;
	}

	private void OnConfirmClicked()
	{
		if (!CanPurchaseSelectedSlot())
		{
			return;
		}
		CardSkinOfferSlot purchasedSlot = selectedSlot;
		ItemSkinDef purchasedSkin = purchasedSlot.itemSkin;
		RealMoneyToMatterPurchase selectedPurchase = GetSelectedPurchase();
		if (selectedPurchase == null)
		{
			return;
		}
		if (GlobalConfigManager.instance.enableNetwork)
		{
			purchaseInProgress = true;
			SetDirty();
			controller.PreparePurchase(purchasedSlot, selectedPurchase, delegate(bool prepared)
			{
				if (!prepared)
				{
					purchaseInProgress = false;
					SetDirty();
				}
				else
				{
					CellUI<RealMoneyToMatterPurchase>.applicationState.purchaseManager.DoPurchase(selectedPurchase, 1, delegate(GlobalRewardPackage reward)
					{
						//IL_0063: Unknown result type (might be due to invalid IL or missing references)
						ItemSkinDef itemSkinDef = reward.rewards?.Find((GlobalRewardInfo rewardInfo) => rewardInfo?.unlockItemSkin != null)?.unlockItemSkin;
						reward.title = (itemSkinDef ?? purchasedSkin).ItemSkinNameWithSet;
						Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_purchase_click);
						selectedSlot = null;
						CellUI<RealMoneyToMatterPurchase>.fortressState.uiManager.globalRewardPackageDisplay.Setup(reward);
						CellUI<RealMoneyToMatterPurchase>.fortressState.uiManager.bindAccountLoginHint.ShowIfNeeded();
						controller.ReloadNetworkOffers();
					}, delegate(bool success)
					{
						purchaseInProgress = false;
						if (!success)
						{
							controller.RequestNetworkOffers();
						}
						SetDirty();
						CellUI<RealMoneyToMatterPurchase>.fortressState.uiManager.purchaseUIPanel.SetDirty();
					});
				}
			});
			return;
		}
		List<GlobalRewardInfo> list = controller.GeneratePurchaseReward(purchasedSlot);
		if (list.Count != 0)
		{
			CellUI<RealMoneyToMatterPurchase>.applicationState.purchaseManager.TryDoLocalPurchase(selectedPurchase, 1, list, purchasedSkin.ItemSkinNameWithSet, delegate(GlobalRewardPackage reward)
			{
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_purchase_click);
				controller.RefillPurchasedSlot(purchasedSlot);
				selectedSlot = null;
				SetDirty();
				CellUI<RealMoneyToMatterPurchase>.fortressState.uiManager.purchaseUIPanel.SetDirty();
				CellUI<RealMoneyToMatterPurchase>.fortressState.uiManager.globalRewardPackageDisplay.Setup(reward);
				CellUI<RealMoneyToMatterPurchase>.fortressState.uiManager.bindAccountLoginHint.ShowIfNeeded();
			});
		}
	}

	private void OnRefreshClicked()
	{
		MatterInfo refreshCost = controller.GetRefreshCost();
		if (purchaseInProgress || controller.IsRequestInProgress || !controller.CanRefresh() || refreshCost == null || !refreshCost.IsPayable(CellUI<RealMoneyToMatterPurchase>.applicationState))
		{
			OnRefreshNotInteractableClicked();
			return;
		}
		controller.RefreshOffers(delegate(bool success)
		{
			if (!success)
			{
				SetDirty();
			}
			else
			{
				selectedSlot = null;
				playRefreshVfxOnNextSync = true;
				SetDirty();
				CellUI<RealMoneyToMatterPurchase>.fortressState.uiManager.purchaseUIPanel.SetDirty();
			}
		});
		SetDirty();
	}

	private void OnRefreshNotInteractableClicked()
	{
		MatterInfo refreshCost = controller.GetRefreshCost();
		bool num = controller.CanRefresh();
		bool flag = controller.State.slots.Count > 0 && controller.State.slots.TrueForAll((CardSkinOfferSlot slot) => slot.soldOut);
		string s = (num ? CellUI<RealMoneyToMatterPurchase>.textData.term_not_purchasable_not_enough_matter_arg0_hint.FormatWith(refreshCost?.matterDef?.matterName) : (flag ? CellUI<RealMoneyToMatterPurchase>.textData.term_card_skin_offer_all_collected : CellUI<RealMoneyToMatterPurchase>.textData.term_card_skin_offer_no_refresh_option));
		CellUI<RealMoneyToMatterPurchase>.fortressState.uiManager.quickHintPanel.HintText(s);
	}

	private void OnOfferStateChanged()
	{
		if (base.gameObject.activeInHierarchy)
		{
			SetDirty();
		}
		CellUI<RealMoneyToMatterPurchase>.fortressState?.uiManager?.purchaseUIPanel?.SetDirty();
	}

	private void CreateRefreshButton()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(confirmButton.gameObject, confirmButton.transform.parent);
		gameObject.name = "RefreshCardSkinOffersButton";
		refreshButton = gameObject.GetComponent<GameButton>();
		refreshButton.onClick.RemoveAllListeners();
		refreshButton.onNotInteractableClick.RemoveAllListeners();
		refreshButton.onClick.AddListener(OnRefreshClicked);
		refreshButton.onNotInteractableClick.AddListener(OnRefreshNotInteractableClicked);
		refreshButtonText = gameObject.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
		refreshCostCell = gameObject.GetComponentInChildren<MatterCell>(includeInactive: true);
		if (refreshButtonText == null || refreshCostCell == null)
		{
			Debug.LogError("刷新按钮缺少文本或货币显示组件", gameObject);
			return;
		}
		InitUI(refreshCostCell);
		refreshCostCell.gameObject.SetActive(value: true);
		refreshCostCell.transform.SetAsLastSibling();
		if (confirmButton.transform is RectTransform rectTransform && refreshButton.transform is RectTransform rectTransform2)
		{
			Vector2 sizeDelta = rectTransform.sizeDelta;
			Vector2 sizeDelta2 = new Vector2(sizeDelta.x * 0.72f, sizeDelta.y);
			float num = sizeDelta2.x + 24f + sizeDelta.x;
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			rectTransform2.sizeDelta = sizeDelta2;
			rectTransform2.anchoredPosition = new Vector2(anchoredPosition.x - num * 0.5f + sizeDelta2.x * 0.5f, anchoredPosition.y);
			rectTransform.anchoredPosition = new Vector2(anchoredPosition.x + num * 0.5f - sizeDelta.x * 0.5f, anchoredPosition.y);
			if (refreshButton.TryGetComponent<HorizontalLayoutGroup>(out var component))
			{
				component.spacing = 18f;
				component.childAlignment = (TextAnchor)4;
			}
			refreshButtonText.rectTransform.sizeDelta = new Vector2(72f, sizeDelta2.y - 14f);
			refreshButtonText.enableAutoSizing = true;
			refreshButtonText.fontSizeMin = 24f;
			refreshButtonText.fontSizeMax = 36f;
			refreshButtonText.enableWordWrapping = false;
			refreshButtonText.alignment = TextAlignmentOptions.MidlineRight;
			refreshCostCell.rectTransform.sizeDelta = new Vector2(86f, 50f);
			ConfigureRefreshCostCell(refreshCostCell);
		}
	}

	private static void ConfigureRefreshCostCell(MatterCell matterCell)
	{
		if (matterCell.transform.Find("Image") is RectTransform rectTransform)
		{
			rectTransform.anchorMin = new Vector2(0f, 0.5f);
			rectTransform.anchorMax = new Vector2(0f, 0.5f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = new Vector2(23f, 0f);
			rectTransform.sizeDelta = new Vector2(52f, 52f);
		}
		if (matterCell.transform.Find("Text") is RectTransform rectTransform2)
		{
			rectTransform2.anchorMin = new Vector2(0f, 0.5f);
			rectTransform2.anchorMax = new Vector2(0f, 0.5f);
			rectTransform2.pivot = new Vector2(0f, 0.5f);
			rectTransform2.anchoredPosition = new Vector2(43f, 0f);
			rectTransform2.sizeDelta = new Vector2(43f, 50f);
			if (rectTransform2.TryGetComponent<TextMeshProUGUI>(out var component))
			{
				component.enableAutoSizing = true;
				component.fontSizeMin = 22f;
				component.fontSizeMax = 34f;
				component.enableWordWrapping = false;
				component.alignment = TextAlignmentOptions.MidlineLeft;
			}
		}
	}

	private string GetPurchasePriceText()
	{
		RealMoneyToMatterPurchase realMoneyToMatterPurchase = GetSelectedPurchase() ?? SourcePurchase;
		if (realMoneyToMatterPurchase == null)
		{
			return string.Empty;
		}
		string text = CellUI<RealMoneyToMatterPurchase>.applicationState.purchaseManager.LocPrice(realMoneyToMatterPurchase.purchaseId);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return CellUI<RealMoneyToMatterPurchase>.textData.term_real_money_price_arg0.FormatWith(realMoneyToMatterPurchase.realMoneyCost);
	}

	private RealMoneyToMatterPurchase GetSelectedPurchase()
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return SourcePurchase;
		}
		return controller.GetPurchaseForSlot(selectedSlot);
	}

	private static void SetButtonText(GameButton button, string value)
	{
		TextMeshProUGUI[] componentsInChildren = button.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].text = value;
		}
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		if (TryGetSlot(module, out var slot) && !slot.soldOut)
		{
			CellUI<RealMoneyToMatterPurchase>.fortressState.uiManager.universalCommonInfoPanel.HideAll();
			selectedSlot = slot;
			SetDirty();
		}
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		if (TryGetSlot(module, out var slot) && selectedSlot == slot)
		{
			selectedSlot = null;
			SetDirty();
		}
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		if (TryGetSlot(module, out var slot))
		{
			return selectedSlot == slot;
		}
		return false;
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		if (TryGetSlot(module, out var slot))
		{
			return !slot.soldOut;
		}
		return false;
	}

	private static bool TryGetSlot(SelectableModuleBase module, out CardSkinOfferSlot slot)
	{
		slot = (module?.parent as CardSkinOfferCell)?.setupData;
		return slot != null;
	}
}
