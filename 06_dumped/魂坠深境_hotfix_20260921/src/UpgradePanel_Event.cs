using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel_Event : UpgradePanel
{
	public class AiUpgradeCandidate
	{
		public int Index;

		public Item Item;

		public int CrystalCost;

		public int HealthCost;
	}

	public UpgradePanelSkinSelectCell skinSelectCellPrefab;

	public GameObject skinSelectCellParent;

	public GameButton skinUpgradeHelpButton;

	public GameButton inspectAllSkinUpgradeButton;

	public ItemSkinConvertInspectPanel inspectPanel;

	[NonSerialized]
	private CellUIPrefabPool<UpgradePanelSkinSelectCell, UpgradePanelSkinSelectCell.CellData> skinSelectCellPool;

	[NonSerialized]
	public int initCrystal;

	[NonSerialized]
	private int upgradeTimes;

	[SerializeField]
	private CanvasGroup panel;

	[SerializeField]
	private CanvasGroup background1;

	[SerializeField]
	private Image background2;

	[NonSerialized]
	private Vector2 background2InitPos;

	[NonSerialized]
	private bool panelShown;

	[NonSerialized]
	private Sequence sequence;

	protected override bool shouldUpdate => parent.gameObject.activeInHierarchy;

	public bool IsHealthCostUpgradeRoom
	{
		get
		{
			if (UIBase.gameState.dungeonMapManager.currentLocation != null)
			{
				return UIBase.gameState.dungeonMapManager.currentLocation.variantType == PathLocation.LocationVariantType.HealthCostUpgradeRoom;
			}
			return false;
		}
	}

	protected override void Init()
	{
		base.Init();
		InitUI(inspectPanel);
		skinSelectCellPool = GetPrefabPool<UpgradePanelSkinSelectCell, UpgradePanelSkinSelectCell.CellData>(skinSelectCellPrefab);
		inspectAllSkinUpgradeButton.onClick.AddListener(delegate
		{
			inspectPanel.Show();
		});
		skinUpgradeHelpButton.onClick.AddListener(delegate
		{
			UIBase.applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_setSkin.Show();
		});
		InitAnim();
	}

	public void Setup()
	{
		itemArea.Setup(UIBase.gameState.playerState.DeckContainer);
		upgradeTimes = 0;
		initCrystal = UIBase.gameState.playerState.crystalCount;
		upgradedItems = new List<Item>();
		previewItem = null;
	}

	public void Unset()
	{
		itemArea.Unset();
		upgradedItems = new List<Item>();
		upgradeTimes = 0;
		initCrystal = 0;
		previewItem = null;
	}

	public List<AiUpgradeCandidate> GetAiUpgradeCandidates()
	{
		List<AiUpgradeCandidate> list = new List<AiUpgradeCandidate>();
		if (!UIBase.gameState.UIManager.worldEventPanel_UpgradeRoom.IsAiUpgradePanelActive)
		{
			return list;
		}
		Item item = selectedItem;
		ItemSkinDef itemSkinDef = upgradeTargetSkin;
		bool flag = isSkinUpgradeMode;
		SetMode(isSkinUpgrade: false);
		foreach (Item item2 in UIBase.gameState.playerState.DeckContainer.GetAllItems().OrEmptyIfNull())
		{
			if (item2 != null)
			{
				selectedItem = item2;
				upgradeTargetSkin = null;
				if (CanConfirmUpgrade())
				{
					list.Add(new AiUpgradeCandidate
					{
						Index = list.Count,
						Item = item2,
						CrystalCost = GetUpgradeCrystalPrice(),
						HealthCost = GetUpgradeHealthPrice()
					});
				}
			}
		}
		isSkinUpgradeMode = flag;
		selectedItem = item;
		upgradeTargetSkin = itemSkinDef;
		return list;
	}

	public bool HasAiUpgradableItem()
	{
		Item item = selectedItem;
		ItemSkinDef itemSkinDef = upgradeTargetSkin;
		bool flag = isSkinUpgradeMode;
		SetMode(isSkinUpgrade: false);
		bool result = false;
		foreach (Item item2 in UIBase.gameState.playerState.DeckContainer.GetAllItems().OrEmptyIfNull())
		{
			if (item2 != null)
			{
				selectedItem = item2;
				upgradeTargetSkin = null;
				if (CanConfirmUpgrade())
				{
					result = true;
					break;
				}
			}
		}
		isSkinUpgradeMode = flag;
		selectedItem = item;
		upgradeTargetSkin = itemSkinDef;
		return result;
	}

	public bool TryUpgradeForAiSession(AiUpgradeCandidate candidate)
	{
		if (candidate?.Item == null || !UIBase.gameState.UIManager.worldEventPanel_UpgradeRoom.IsAiUpgradePanelActive)
		{
			return false;
		}
		SetMode(isSkinUpgrade: false);
		selectedItem = candidate.Item;
		upgradeTargetSkin = null;
		if (!CanConfirmUpgrade())
		{
			return false;
		}
		UpgradeItem();
		return true;
	}

	public void CloseForAiSession()
	{
		if (UIBase.gameState.UIManager.worldEventPanel_UpgradeRoom.IsAiUpgradePanelActive)
		{
			HidePanel();
		}
	}

	public ItemDisplayBase GetSkinUpgradeGuideTargetDisplay()
	{
		return itemArea.GetAllItemDisplays().FirstOrDefault((ItemDisplayBase display) => display.item != null && UIBase.applicationState.itemSkinManager.GetAllUnlockedSkinsOfItem(display.item.def).Count > 0);
	}

	public override void Select(SelectableModuleBase module)
	{
		base.Select(module);
		if (isSkinUpgradeMode)
		{
			Item item = (module.parent as ItemDisplayBase).item;
			List<ItemSkinDef> allValidSkinsOfItem = UIBase.applicationState.itemSkinManager.GetAllValidSkinsOfItem(item.def);
			List<ItemSkinDef> source = allValidSkinsOfItem.Where((ItemSkinDef sk) => sk.IsUnlocked(UIBase.applicationState)).ToList();
			if (source.Any())
			{
				upgradeTargetSkin = source.FirstOrDefault();
			}
			else if (allValidSkinsOfItem.Any())
			{
				upgradeTargetSkin = allValidSkinsOfItem.FirstOrDefault();
			}
			else
			{
				upgradeTargetSkin = null;
			}
		}
	}

	public override bool IsSelectDark(SelectableModuleBase module)
	{
		if (isSkinUpgradeMode)
		{
			Item item = (module.parent as ItemDisplayBase).item;
			if (!UIBase.applicationState.itemSkinManager.GetAllValidSkinsOfItem(item.def).Any((ItemSkinDef s) => UIBase.applicationState.itemSkinManager.IsItemSkinUnlocked(s)))
			{
				return true;
			}
		}
		return base.IsSelectDark(module);
	}

	protected override void UpdateDisplay()
	{
		base.UpdateDisplay();
		bool upgrading = itemInfoDisplay.upgrading;
		List<ItemSkinDef> source = ((selectedItem == null) ? null : UIBase.applicationState.itemSkinManager.GetAllValidSkinsOfItem(selectedItem.def));
		if (upgrading || !isSkinUpgradeMode || source.IsNullOrEmpty())
		{
			skinSelectCellParent.gameObject.SetActive(value: false);
		}
		else
		{
			skinSelectCellParent.gameObject.SetActive(value: true);
			skinSelectCellPool.DespawnAll();
			foreach (ItemSkinDef item in source.OrEmptyIfNull())
			{
				skinSelectCellPool.Spawn(new UpgradePanelSkinSelectCell.CellData(item));
			}
		}
		skinUpgradeHelpButton.gameObject.SetActive(isSkinUpgradeMode);
		inspectAllSkinUpgradeButton.gameObject.SetActive(isSkinUpgradeMode);
	}

	public override void ShowPanel()
	{
		OnShowUpgradePanel();
	}

	public override void HidePanel()
	{
		currentUpgradeNFX?.Stop();
		currentUpgradeNFX = null;
		UIBase.gameState.UIManager.worldEventPanel_UpgradeRoom.animState = UpgradeRoomEventPanel.AnimState.Idle;
		UIBase.gameState.UIManager.worldEventPanel_UpgradeRoom.SetDirty();
	}

	public override void UpgradeItem()
	{
		skinSelectCellParent.gameObject.SetActive(value: false);
		base.UpgradeItem();
		upgradeTimes++;
	}

	private int CalculateCrystalUpgradePrice()
	{
		UpgradeRoomEvent.UpgradeRoomConfig upgradeConfig = ScriptableObjectSingleton<GameDataManager>.instance.upgradeConfig;
		int num = 0;
		num += upgradeConfig.upgradeBasePrice;
		num += UIBase.gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.ItemUpgradePriceDelta);
		num += upgradeConfig.stageIncreasePrice * UIBase.gameState.difficultyState.CurrentStage;
		PathLocation currentLocation = UIBase.gameState.dungeonMapManager.currentLocation;
		if (currentLocation != null && currentLocation.variantType == PathLocation.LocationVariantType.UpgradeRoomFreeInitUpgrade)
		{
			num = 0;
		}
		if (!UIBase.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NoIncreasePriceForCardUpgrade))
		{
			num += upgradeConfig.upgradeUpPrice * upgradeTimes;
		}
		return Mathf.Max(0, num);
	}

	public override int GetUpgradeCrystalPrice()
	{
		if (IsHealthCostUpgradeRoom)
		{
			return 0;
		}
		if (isSkinUpgradeMode && base.toShowItem.upgraded)
		{
			return 0;
		}
		return CalculateCrystalUpgradePrice();
	}

	public override int GetUpgradeHealthPrice()
	{
		if (IsHealthCostUpgradeRoom)
		{
			if (isSkinUpgradeMode && base.toShowItem.upgraded)
			{
				return 0;
			}
			return CalculateCrystalUpgradePrice() * 3;
		}
		return 0;
	}

	private void InitAnim()
	{
		background2InitPos = background2.rectTransform.anchoredPosition;
		panelShown = false;
	}

	private async void OnShowUpgradePanel()
	{
		if (!panelShown)
		{
			panelShown = true;
			UpgradeRoomEventPanel.Config upgradeRoomDisplayConfig = UIBase.gameState.applicationState.vfxDataPreset.upgradeRoomDisplayConfig;
			sequence?.Kill();
			sequence = DOTween.Sequence();
			base.gameObject.SetActive(value: true);
			sequence.Append(DOTweenModuleUI.DOFade(background1, 0f, 0f));
			sequence.Join(DOTweenModuleUI.DOFade(background2, 0f, 0f));
			sequence.Join(DOTweenModuleUI.DOFade(panel, 0f, 0f));
			sequence.Join(DOTweenModuleUI.DOAnchorPos(background2.rectTransform, background2InitPos + new Vector2(background2.rectTransform.GetWidth(), 0f), 0f, false));
			sequence.Insert(0f, DOTweenModuleUI.DOFade(background1, 1f, upgradeRoomDisplayConfig.openPanelTime1));
			sequence.Insert(upgradeRoomDisplayConfig.openPanelTime1, DOTweenModuleUI.DOFade(background2, 1f, upgradeRoomDisplayConfig.openPanelTime2));
			sequence.Insert(upgradeRoomDisplayConfig.openPanelTime1, DOTweenModuleUI.DOAnchorPos(background2.rectTransform, background2InitPos, upgradeRoomDisplayConfig.openPanelTime2, false));
			sequence.Insert(upgradeRoomDisplayConfig.openPanelTime1 + upgradeRoomDisplayConfig.openPanelTime2 - upgradeRoomDisplayConfig.openPanelTime3, DOTweenModuleUI.DOFade(panel, 1f, upgradeRoomDisplayConfig.openPanelTime3));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
			await DOTweenModuleUnityVersion.AsyncWaitForKill((Tween)sequence);
		}
	}
}
