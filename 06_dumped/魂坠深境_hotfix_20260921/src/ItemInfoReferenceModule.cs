using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoReferenceModule : ItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	private EnchantDisplay enchantReferencePrefab;

	[SerializeField]
	private RuneDisplay runeDisplayPrefab;

	[SerializeField]
	private YinYangInfoDisplay yangInfoDisplay;

	[SerializeField]
	private RectTransform sideReferencePivot;

	[SerializeField]
	private ScrollRectExtend sideReferenceScrollRect;

	[NonSerialized]
	private CellUIPrefabPool<EnchantDisplay, Enchant> enchantReferencePool;

	[NonSerialized]
	private CellUIPrefabPool<RuneDisplay, Rune> runeDisplayPool;

	[NonSerialized]
	private bool showingSideReference;

	[NonSerialized]
	private CellUIPrefabPool<YinYangInfoDisplay, YinYangFeatureAbility.IYinYangInfoData> yinYangInfoDisplayPool;

	[SerializeField]
	private MainItemInfoDisplayBase itemReferencePrefab;

	[SerializeField]
	private RectTransform itemReferencePivot;

	[SerializeField]
	private GameButton itemReferenceHideButton;

	[NonSerialized]
	private CellUIPrefabPool<MainItemInfoDisplayBase, Item> itemReferencePool;

	[NonSerialized]
	private List<MainItemInfoDisplayBase> itemReferences;

	[SerializeField]
	public CommonReference commonReference;

	[NonSerialized]
	private bool showingStory;

	public bool CanShowItemReference
	{
		get
		{
			if (itemReferencePivot != null)
			{
				return itemReferenceHideButton != null;
			}
			return false;
		}
	}

	public override void Init()
	{
		parent.InitUI(commonReference);
		runeDisplayPool = parent.GetPrefabPool<RuneDisplay, Rune>(runeDisplayPrefab, sideReferencePivot);
		enchantReferencePool = parent.GetPrefabPool<EnchantDisplay, Enchant>(enchantReferencePrefab, sideReferencePivot);
		yinYangInfoDisplayPool = parent.GetPrefabPool<YinYangInfoDisplay, YinYangFeatureAbility.IYinYangInfoData>(yangInfoDisplay, sideReferencePivot);
		itemReferencePool = parent.GetPrefabPool<MainItemInfoDisplayBase, Item>(itemReferencePrefab, itemReferencePivot);
		itemReferences = new List<MainItemInfoDisplayBase>();
		showingSideReference = false;
		itemReferenceHideButton.onClick.AddListener(delegate
		{
			if (parent.parent is ItemSkinPreviewPanel itemSkinPreviewPanel)
			{
				itemSkinPreviewPanel.originItemInfo.GetModule<ItemInfoReferenceModule>().HideItemReference();
				itemSkinPreviewPanel.newItemInfo.GetModule<ItemInfoReferenceModule>().HideItemReference();
			}
			else
			{
				HideItemReference();
			}
		});
	}

	public void ToggleSideReference()
	{
		if (showingSideReference)
		{
			HideAllSideReference();
		}
		else
		{
			ShowAllSideReference();
		}
	}

	public void ShowAllSideReference()
	{
		HideAllSideReference();
		UnsetItemReference();
		showingSideReference = true;
		runeDisplayPool.DespawnAll();
		for (int num = base.item.Runes.Count - 1; num >= 0; num--)
		{
			runeDisplayPool.Spawn(base.item.Runes[num]);
		}
		enchantReferencePool.DespawnAll();
		for (int i = 0; i < base.item.Enchants.Count; i++)
		{
			enchantReferencePool.Spawn(base.item.Enchants[i]);
		}
		YinYangFeatureAbility.DisplayInfos displayInfos = YinYangFeatureAbility.GetYinYangDisplayModule(base.item)?.GetDisplayInfo(base.item, UIDisplayModule.gameState);
		yinYangInfoDisplayPool.DespawnAll();
		if (displayInfos?.yinConvertInfo != null)
		{
			yinYangInfoDisplayPool.Spawn(displayInfos.yinConvertInfo);
		}
		if (displayInfos?.yangConvertInfo != null)
		{
			yinYangInfoDisplayPool.Spawn(displayInfos.yangConvertInfo);
		}
		if (displayInfos?.yangAddInfo != null)
		{
			yinYangInfoDisplayPool.Spawn(displayInfos.yangAddInfo);
		}
		if (displayInfos?.yinAddInfo != null)
		{
			yinYangInfoDisplayPool.Spawn(displayInfos.yinAddInfo);
		}
		if (displayInfos?.yangConsumeInfo != null)
		{
			yinYangInfoDisplayPool.Spawn(displayInfos.yangConsumeInfo);
		}
		if (displayInfos?.yinConsumeInfo != null)
		{
			yinYangInfoDisplayPool.Spawn(displayInfos.yinConsumeInfo);
		}
		SetSideReferencePosition();
	}

	public void HideAllSideReference()
	{
		showingSideReference = false;
		runeDisplayPool?.DespawnAll();
		enchantReferencePool?.DespawnAll();
		yinYangInfoDisplayPool?.DespawnAll();
	}

	private void SetSideReferencePosition()
	{
		sideReferenceScrollRect.RectTransform().anchoredPosition = base.itemInfoDisplay.GetLastItemInfoDisplayPosition() + new Vector2(470f, 350f);
	}

	public void HideAll()
	{
		itemReferenceHideButton.gameObject.SetActive(value: false);
		UnsetCommonReference();
		UnsetItemReference();
	}

	public bool SetupItemReference(Item item, MainItemInfoDisplayBase from, bool isUpgrade = false)
	{
		if (item == null || from == null)
		{
			return false;
		}
		if (!EnsureItemReferencePool(from))
		{
			Debug.LogError("[ItemInfoReference] 无法初始化卡牌引用池", this);
			return false;
		}
		showingStory = false;
		HideAllSideReference();
		SetItemReferencePosition();
		itemReferencePivot.SetAsLastSibling();
		itemReferenceHideButton.gameObject.SetActive(value: true);
		itemReferenceHideButton.transform.SetAsLastSibling();
		int num = itemReferences.IndexOf(from);
		if (!base.itemInfoDisplay.IsReference(from))
		{
			num = 0;
		}
		else
		{
			if (num == -1)
			{
				throw new Exception("from display not found");
			}
			num++;
		}
		return SetupItemReference(item, num, from, isUpgrade);
	}

	public void UnsetItemReference()
	{
		UnsetItemReference(0);
		itemReferenceHideButton.gameObject.SetActive(value: false);
	}

	private bool SetupItemReference(Item item, int index, MainItemInfoDisplayBase from, bool isUpgrade = false)
	{
		UnsetItemReference(index);
		if (index >= itemReferences.Count)
		{
			MainItemInfoDisplayBase mainItemInfoDisplayBase = itemReferencePool.Spawn(item, null, delegate
			{
			});
			if (mainItemInfoDisplayBase == null)
			{
				Debug.LogError("[ItemInfoReference] 卡牌引用池无法生成预览卡", this);
				itemReferenceHideButton.gameObject.SetActive(itemReferences.Count > 0);
				base.itemInfoDisplay.upgradeTextGenerator.before = null;
				base.itemInfoDisplay.upgradeTextGenerator.after = null;
				base.itemInfoDisplay.upgradeTextGenerator.Clear();
				return false;
			}
			itemReferences.Add(mainItemInfoDisplayBase);
			mainItemInfoDisplayBase.transform.SetSiblingIndex(index);
		}
		itemReferences[index].setAsUpgradeDisplay = isUpgrade;
		itemReferences[index].Setup(item);
		itemReferences[index].ForceUpdate();
		LayoutRebuilder.ForceRebuildLayoutImmediate(itemReferencePivot);
		base.itemInfoDisplay.upgradeTextGenerator.before = from.GetModule<SpellTextModule>();
		base.itemInfoDisplay.upgradeTextGenerator.after = itemReferences[index].GetModule<SpellTextModule>();
		base.itemInfoDisplay.SetDirty();
		return true;
	}

	public void UnsetItemReference(MainItemInfoDisplayBase from)
	{
		showingStory = false;
		int num = itemReferences.IndexOf(from);
		if (!base.itemInfoDisplay.IsReference(from))
		{
			num = 0;
		}
		else
		{
			if (num == -1)
			{
				throw new Exception("from display not found");
			}
			num++;
		}
		UnsetItemReference(num);
		if (num == 0)
		{
			itemReferenceHideButton.gameObject.SetActive(value: false);
		}
	}

	private void UnsetItemReference(int index)
	{
		if (itemReferencePool != null && itemReferences != null)
		{
			int count = itemReferences.Count;
			for (int i = index; i < count; i++)
			{
				itemReferencePool.Despawn(itemReferences[index]);
				itemReferences.Remove(itemReferences[index]);
			}
		}
	}

	private bool EnsureItemReferencePool(MainItemInfoDisplayBase from)
	{
		if (!CanShowItemReference)
		{
			return false;
		}
		if (itemReferences == null)
		{
			itemReferences = new List<MainItemInfoDisplayBase>();
		}
		if (itemReferencePool != null)
		{
			return true;
		}
		if (parent == null)
		{
			parent = from.itemInfoDisplay ?? from.GetComponentInParent<ItemInfoDisplayBase>(includeInactive: true);
		}
		if (parent == null || itemReferencePrefab == null)
		{
			return false;
		}
		itemReferencePool = parent.GetPrefabPool<MainItemInfoDisplayBase, Item>(itemReferencePrefab, itemReferencePivot);
		return itemReferencePool != null;
	}

	private void SetItemReferencePosition()
	{
		itemReferencePivot.anchoredPosition = base.itemInfoDisplay.GetLastItemInfoDisplayPosition() + new Vector2(220f, 0f);
	}

	private void HideItemReference()
	{
		UnsetItemReference();
		UnsetCommonReference();
		ShowAllSideReference();
		base.itemInfoDisplay.SetDirty();
	}

	public void SetupRefObjectDisplay(MainItemInfoDisplayBase mainItemInfoDisplay, object refObject)
	{
		showingStory = false;
		if (refObject is Item item)
		{
			SetupItemReference(item, mainItemInfoDisplay);
			return;
		}
		if (refObject is ItemDefinition def)
		{
			SetupItemReference(new Item(def), mainItemInfoDisplay);
			return;
		}
		SetCommonReferencePosition(mainItemInfoDisplay);
		commonReference.Setup(new CommonInfoData(refObject, CommonInfoData.ControlType.Click, null, false));
	}

	public void SetupItemStoryReference(MainItemInfoDisplayBase mainItemInfoDisplay, Item item)
	{
		if (showingStory)
		{
			UnsetCommonReference();
			return;
		}
		showingStory = true;
		SetCommonReferencePosition(mainItemInfoDisplay);
		commonReference.Setup(new CommonInfoData(item, CommonInfoData.ControlType.Click, null, false));
	}

	public void UnsetCommonReference()
	{
		showingStory = false;
		commonReference.Unset();
	}

	private void SetCommonReferencePosition(MainItemInfoDisplayBase mainItemInfoDisplay)
	{
		if (mainItemInfoDisplay != null)
		{
			commonReference.transform.position = mainItemInfoDisplay.transform.position;
			commonReference.RectTransform().anchoredPosition = commonReference.RectTransform().anchoredPosition + new Vector2(0f, 60f);
		}
		else
		{
			commonReference.RectTransform().anchoredPosition = base.itemInfoDisplay.GetLastItemInfoDisplayPosition() + new Vector2(0f, 60f);
		}
	}
}
