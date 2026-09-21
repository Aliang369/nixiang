using Common.UI;
using ContainerOfSoul.Display.Game.BattleDisplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSkinOfferCell : CellUI<CardSkinOfferSlot>, IDisplayModulesController
{
	[SerializeField]
	private GlobalItemDisplay globalItemDisplay;

	[SerializeField]
	private RewardItemSkinDisplay fullCardDisplayPrefab;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private TextMeshProUGUI hint;

	[SerializeField]
	private GameButton purchaseButton;

	[SerializeField]
	private TextMeshProUGUI purchaseButtonText;

	[SerializeField]
	private GameObject soldOutMask;

	private const float FullCardScale = 0.6f;

	private static readonly Vector2 FullCardPosition = new Vector2(0f, -38f);

	private static readonly Vector2 SoldOutTextSize = new Vector2(320f, 90f);

	private RewardItemSkinDisplay fullCardDisplay;

	private ItemSkinDef displayedItemSkin;

	private MultiInfoStateModule showEffect;

	private bool playRefreshVfx;

	protected override void Init()
	{
		globalItemDisplay.gameObject.SetActive(value: false);
		purchaseButton.gameObject.SetActive(value: false);
		description.gameObject.SetActive(value: false);
		hint.gameObject.SetActive(value: false);
		soldOutMask.SetActive(value: false);
		Transform transform = base.transform.Find("-h-小卡-冷却遮罩");
		if (transform != null)
		{
			transform.gameObject.SetActive(value: false);
		}
		RectTransform obj = description.rectTransform;
		obj.anchorMin = new Vector2(0.5f, 0.5f);
		obj.anchorMax = new Vector2(0.5f, 0.5f);
		obj.pivot = new Vector2(0.5f, 0.5f);
		obj.anchoredPosition = FullCardPosition;
		obj.sizeDelta = SoldOutTextSize;
		description.enableAutoSizing = false;
		description.fontSize = 42f;
		description.alignment = TextAlignmentOptions.Center;
		description.raycastTarget = false;
		fullCardDisplay = Object.Instantiate(fullCardDisplayPrefab, base.transform);
		fullCardDisplay.name = fullCardDisplayPrefab.name;
		fullCardDisplay.transform.SetSiblingIndex(soldOutMask.transform.GetSiblingIndex());
		showEffect = fullCardDisplay.GetComponentInChildren<MultiInfoStateModule>(includeInactive: true);
		if (showEffect != null)
		{
			showEffect.suppressShowVfx = true;
		}
		ItemInfoDisplay_Normal componentInChildren = fullCardDisplay.GetComponentInChildren<ItemInfoDisplay_Normal>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.showStoryReference = false;
		}
		RectTransform obj2 = fullCardDisplay.rectTransform;
		obj2.anchorMin = new Vector2(0.5f, 0.5f);
		obj2.anchorMax = new Vector2(0.5f, 0.5f);
		obj2.pivot = new Vector2(0.5f, 0.5f);
		obj2.anchoredPosition = FullCardPosition;
		obj2.localScale = Vector3.one * 0.6f;
		Graphic[] componentsInChildren = fullCardDisplay.GetComponentsInChildren<Graphic>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].raycastTarget = false;
		}
		InitUI(fullCardDisplay);
	}

	protected override void OnSetup(CardSkinOfferSlot slot)
	{
		playRefreshVfx = (parent as CardSkinOfferPanel)?.ShouldPlayRefreshVfx ?? false;
		if (playRefreshVfx)
		{
			displayedItemSkin = null;
		}
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		bool flag = base.setupData?.soldOut ?? true;
		soldOutMask.SetActive(value: false);
		purchaseButton.gameObject.SetActive(value: false);
		globalItemDisplay.gameObject.SetActive(value: false);
		fullCardDisplay.gameObject.SetActive(!flag);
		description.gameObject.SetActive(flag);
		title.transform.parent.gameObject.SetActive(value: true);
		if (flag)
		{
			playRefreshVfx = false;
			displayedItemSkin = null;
			title.text = base.setupData?.batch?.DisplayName ?? string.Empty;
			description.text = CellUI<CardSkinOfferSlot>.textData.term_card_skin_offer_sold_out;
			return;
		}
		title.text = base.setupData.itemSkin.ItemSkinNameWithSet;
		if (!(displayedItemSkin == base.setupData.itemSkin))
		{
			displayedItemSkin = base.setupData.itemSkin;
			if (showEffect != null)
			{
				showEffect.suppressShowVfx = !playRefreshVfx;
			}
			fullCardDisplay.Setup(displayedItemSkin);
			if (showEffect != null)
			{
				showEffect.suppressShowVfx = true;
			}
			playRefreshVfx = false;
		}
	}
}
