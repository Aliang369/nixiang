using System;
using System.Collections.Generic;
using System.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;

public class StoreEvent : WorldEvent
{
	[Serializable]
	public class QualityPriceInfo
	{
		public QualityLevel level;

		public int buyCostCoin;

		public int sellCostCoin;
	}

	[Serializable]
	public class PricedItem
	{
		public int price;

		public bool free;

		public Item item;

		public int TreasureValue => item.treasureValue;

		public bool IsTreasure => TreasureValue > 0;
	}

	public class StoreConfig
	{
		public float buyItemAniDialogWaitTime;

		public WorldEventChoiceText buyItemChoiceText;

		public DialogueSequence buyItemDialogue;

		public ClavisDef clavis;

		public DialogueSequence collectorDialogue;

		public DialogueSequence enterStoreDialogue;

		public DialogueSequence exitDialogue;

		public int itemForSellCount = 5;

		public int maxRefreshTimes = 1;

		public SkeletonDataAsset npcSkeletonDataAsset;

		public List<QualityPriceInfo> qualityInfos;

		public int refreshItemBasePrice = 3;

		public WorldEventChoiceText refreshStorChoiceText;

		public int sellIncreasePrice = 4;

		public float sellItemAniWaitTime;

		public DialogueSequence sellItemNoTreasureDialogue;

		public DialogueSequence sellItemTreasureDialogue;

		public int stageIncreaseBuyItemPrice = 2;

		public int stageIncreaseSellItemPrice = 2;

		public DialogueSequence switchToSellPanelDialogue;
	}

	public const string npcIdleAniName = "#idle#";

	public const string npcEnterStoreAniName = "#入场#";

	public const string npcExitStoreAniName = "#退场#";

	[NonSerialized]
	public List<PricedItem> boughtItemsFromStore;

	[NonSerialized]
	public List<PricedItem> itemsForSell;

	[NonSerialized]
	public List<PricedItem> soldItems;

	[NonSerialized]
	public int initCoin;

	[NonSerialized]
	public int refreshTimes;

	public override WorldEventDisplayType displayType => WorldEventDisplayType.GoldStore;

	private SkeletonAnimation NpcSpineAnimationInstance => gameState.currentEncounter.room.GetComponentsInChildren<SkeletonAnimation>().FirstOrDefault((SkeletonAnimation s) => (UnityEngine.Object)(object)((SkeletonRenderer)s).skeletonDataAsset == (UnityEngine.Object)(object)Config.npcSkeletonDataAsset);

	public StoreConfig Config => ScriptableObjectSingleton<GameDataManager>.instance.storeConfig;

	private bool EndlessRefreshVairantLoc
	{
		get
		{
			PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
			if (currentLocation == null)
			{
				return false;
			}
			return currentLocation.variantType == PathLocation.LocationVariantType.GoldStoreEndlessRefresh;
		}
	}

	public override WorldEventPanel GetDisplayWorldEventPanel(GameStateInfo gameState)
	{
		return gameState.UIManager.worldEventPanel_GoldStore;
	}

	protected override void Init()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		currentDisplayInfo = GenerateDefaultWorldEventDisplayInfo();
		refreshTimes = 0;
		GenerateRandomItemsForBuy();
		initCoin = gameState.playerState.coinCount;
		soldItems = new List<PricedItem>();
		boughtItemsFromStore = new List<PricedItem>();
		PlayRoomNpcShootAnimation("#入场#");
		Bootstrap.instance.audioManager.PlaySoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_ambience_store);
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_enter_shop);
	}

	public override void FinishedWorldEvent()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_ambience_store);
		base.FinishedWorldEvent();
	}

	public int GetBuyItemCost(Item item)
	{
		int num = 0;
		QualityPriceInfo qualityPriceInfo = Config.qualityInfos.Find((QualityPriceInfo t) => t.level == item.qualityLevel);
		if (qualityPriceInfo != null)
		{
			num += qualityPriceInfo.buyCostCoin + gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.StoreBuyItemCostDelta);
		}
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.FixStoreBuyCardPriceToOne))
		{
			num = 1;
		}
		return ApplyActiveStorePriceDiscount(num);
	}

	public int GetRefreshPrice()
	{
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.StoreBuyItemRefreshNoCostOnce))
		{
			return 0;
		}
		int num = Config.refreshItemBasePrice + refreshTimes;
		if (EndlessRefreshVairantLoc)
		{
			num += refreshTimes;
		}
		return ApplyActiveStorePriceDiscount(num);
	}

	public int GetRemoveItemCost(Item item)
	{
		int num = 0;
		if (item.treasureValue > 0)
		{
			return -item.treasureValue;
		}
		QualityPriceInfo qualityPriceInfo = Config.qualityInfos.Find((QualityPriceInfo t) => t.level == item.qualityLevel);
		if (qualityPriceInfo != null)
		{
			num += qualityPriceInfo.sellCostCoin + gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.StoreSellItemCostDelta);
		}
		if (!gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NoIncreasePriceForStoreDeleteCard))
		{
			num += soldItems.Count((PricedItem p) => !p.IsTreasure) * Config.sellIncreasePrice;
		}
		if (!gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NoStoreRecycleItemPriceStageIncrease))
		{
			num += gameState.difficultyState.CurrentStage * ScriptableObjectSingleton<GameDataManager>.instance.storeConfig.stageIncreaseSellItemPrice;
		}
		return ApplyActiveStorePriceDiscount(num);
	}

	public bool CanSellItem(Item item)
	{
		if (item != null)
		{
			return gameState.contractState?.selectedContract?.contractDef?.contractRule?.CanSellItem(gameState, item) ?? true;
		}
		return false;
	}

	private int ApplyActiveStorePriceDiscount(int price)
	{
		int num = 0;
		foreach (GameSkill activatedSkill in gameState.playerState.ActivatedSkills)
		{
			if (activatedSkill?.skillDef?.abilities == null)
			{
				continue;
			}
			foreach (Ability ability in activatedSkill.skillDef.abilities)
			{
				if (ability is StorePriceDiscountAbility storePriceDiscountAbility)
				{
					num = Mathf.Max(num, storePriceDiscountAbility.GetDiscountPercent(activatedSkill));
				}
			}
		}
		return StorePriceDiscountAbility.ApplyToPrice(price, num);
	}

	public PricedItem GetPriceItem(Item item)
	{
		if (!CanSellItem(item))
		{
			return null;
		}
		PricedItem pricedItem = new PricedItem
		{
			price = GetRemoveItemCost(item),
			item = item,
			free = false
		};
		int variableValue = gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.StoreFreeRecycleItemCount);
		variableValue = Mathf.Max(variableValue, 0);
		List<PricedItem> list = soldItems.FindAll((PricedItem p) => p.free);
		if (pricedItem.price > 0 && list.Count < variableValue)
		{
			pricedItem.price = 0;
			pricedItem.free = true;
		}
		return pricedItem;
	}

	public void GenerateRandomItemsForBuy()
	{
		int itemForSellCount = Config.itemForSellCount;
		itemForSellCount += gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.StoreBuyItemCountDelta);
		PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
		if (currentLocation != null && currentLocation.variantType == PathLocation.LocationVariantType.GoldStoreMoreItem)
		{
			itemForSellCount *= 2;
		}
		List<Item> list = gameState.generationManager.GenerateStoreCardItems(itemForSellCount);
		if (gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableGoldStoreBuyLootItem))
		{
			list.AddRange(gameState.generationManager.GenerateRandomLoot(1));
		}
		list = list.OrderBy((Item item) => item.qualityLevel).ToList();
		itemsForSell = list.ConvertAll((Item item) => new PricedItem
		{
			item = item,
			price = GetBuyItemCost(item)
		});
		AppendFinalEndingKeyItemForBuy();
		gameState.contractState?.selectedContract?.contractDef?.ModifyStoreItemsForBuy(gameState, itemsForSell);
		foreach (PricedItem item in itemsForSell)
		{
			bool selectable = gameState.playerState.HasEnoughCoin(item.price);
			ThinkingAnalyticsExtends.TrackExposeBonusCard(gameState, item.item, "商店", selectable);
		}
	}

	private void AppendFinalEndingKeyItemForBuy()
	{
		ItemDefinition keyDef = ScriptableObjectSingleton<GameDataManager>.instance.finalEndingKeyItem;
		if (!(keyDef == null) && gameState.difficultyState.contractDifficulty >= DifficultyLevel.Nightmare && !gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.StoreFinalEndingKeyPurchased) && !FinalEndingKeyUtility.HasPlayerFinalEndingKey(gameState) && !itemsForSell.Any((PricedItem p) => p.item?.GetDef() == keyDef))
		{
			Item item = new Item(keyDef);
			itemsForSell.Add(new PricedItem
			{
				item = item,
				price = GetBuyItemCost(item)
			});
		}
	}

	public void RefreshBuyItems()
	{
		GenerateRandomItemsForBuy();
		EnqueueAction(new ChangeCoinAction(-GetRefreshPrice(), ChangeCoinAction.ChangeReason.Store));
		gameState.globalVariableStateInfo.SetVariableValue(InGameGlobalVariableStateInfo.StoreBuyItemRefreshNoCostOnce, 0);
		refreshTimes++;
	}

	public bool EnableRefreshBuyItems()
	{
		bool flag = gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.StoreBuyItemEndlessRefresh) || EndlessRefreshVairantLoc;
		if (gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableGoldStoreReroll) || gameState.contractState.selectedContract.contractDef.contractType == ContractDef.ContractType.PVPOffender)
		{
			return (refreshTimes < Config.maxRefreshTimes) | flag;
		}
		return false;
	}

	public bool CanRefreshBuyItems()
	{
		return (int)gameState.playerState.coinCount >= GetRefreshPrice();
	}

	private WorldEventDisplayInfo GenerateDefaultWorldEventDisplayInfo()
	{
		return new WorldEventDisplayInfo
		{
			title = EventName,
			description = EventDescription
		};
	}

	public void BuyItem(PricedItem pi)
	{
		EnqueueAction(new ChangeCoinAction(-pi.price, ChangeCoinAction.ChangeReason.Store));
		EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
		{
			items = new List<Item> { pi.item.Copy() }
		});
		itemsForSell.Remove(pi);
		boughtItemsFromStore.Add(pi);
		if (FinalEndingKeyUtility.IsFinalEndingKey(pi.item))
		{
			gameState.globalVariableStateInfo.SetVariableValue(InGameGlobalVariableStateInfo.StoreFinalEndingKeyPurchased, 1);
		}
		gameState.contractState?.selectedContract?.contractDef?.NotifyStoreItemBought(gameState, pi.item);
		ThinkingAnalyticsExtends.TrackBonusCard(gameState, pi.item, "商店");
	}

	public void SellPlayerDeckItem(PricedItem pi)
	{
		if (pi != null && CanSellItem(pi.item))
		{
			gameState.actionManager.EnqueueAction(new ExileItemAction(pi.item, null));
			gameState.actionManager.EnqueueAction(new ChangeCoinAction(-pi.price, ChangeCoinAction.ChangeReason.Store));
			gameState.actionManager.EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerStoreSellItem(pi.item);
			});
			soldItems.Add(pi);
		}
	}

	public bool IsItemBoughtFormStore(PricedItem item)
	{
		return boughtItemsFromStore.Contains(item);
	}

	public void PlayRoomNpcShootAnimation(string aniName, bool resetToIdle = true)
	{
		SkeletonAnimation ani = NpcSpineAnimationInstance;
		if ((UnityEngine.Object)(object)ani == null)
		{
			return;
		}
		TrackEntry trackEntry = ani.PlayAnimation(aniName, loop: false);
		if (resetToIdle)
		{
			trackEntry.Complete += delegate
			{
				ani.PlayAnimation("#idle#", loop: true);
			};
		}
	}
}
