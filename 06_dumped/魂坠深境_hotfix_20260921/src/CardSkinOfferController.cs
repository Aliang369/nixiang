using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.RealName.Utils;
using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public sealed class CardSkinOfferController
{
	private const int OfferSlotCount = 3;

	private const int LocalRefreshCostCount = 30;

	private static readonly TimeSpan NetworkShelfRetryDelay = TimeSpan.FromSeconds(30.0);

	public const string PurchaseIdSlot1 = "0000302RealMoneyToMatterPurchase";

	public const string PurchaseIdSlot2 = "0000303RealMoneyToMatterPurchase";

	public const string PurchaseIdSlot3 = "0000304RealMoneyToMatterPurchase";

	private static readonly string[] PurchaseIds = new string[3] { "0000302RealMoneyToMatterPurchase", "0000303RealMoneyToMatterPurchase", "0000304RealMoneyToMatterPurchase" };

	private readonly ApplicationStateInfo applicationState;

	private readonly Func<int, int> randomIndex;

	private bool requestInProgress;

	private bool networkShelfDirty;

	private bool reconcileNetworkShelfAfterRequest;

	private DateTime nextNetworkShelfRetryAtUtc;

	private bool currentRequestSuccess;

	private readonly List<Action<bool>> pendingRequestCompleted = new List<Action<bool>>();

	private readonly List<WaitPanelBase.Token> pendingRequestWaitPanels = new List<WaitPanelBase.Token>();

	public CardSkinOfferState State { get; } = new CardSkinOfferState();

	public bool IsRequestInProgress => requestInProgress;

	public bool IsLoaded
	{
		get
		{
			if (GlobalConfigManager.instance.enableNetwork)
			{
				return State.loaded;
			}
			return true;
		}
	}

	public bool IsNetworkShelfReady
	{
		get
		{
			if (GlobalConfigManager.instance.enableNetwork)
			{
				if (State.loaded && !networkShelfDirty)
				{
					return !IsNetworkShelfExpired();
				}
				return false;
			}
			return true;
		}
	}

	public DateTime CurrentUtcTime
	{
		get
		{
			if (!GlobalConfigManager.instance.enableNetwork)
			{
				return DateTime.UtcNow;
			}
			return GetServerUtcTime();
		}
	}

	public event Action StateChanged;

	public CardSkinOfferController(ApplicationStateInfo applicationState, Func<int, int> randomIndex = null)
	{
		this.applicationState = applicationState;
		this.randomIndex = randomIndex ?? ((Func<int, int>)((int count) => UnityEngine.Random.Range(0, count)));
	}

	public void EnsureValidOffers()
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		List<ItemSkinSetBatch> rotation = CurrentRotation();
		int currentDailyPeriod = ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentDailyPeriod;
		if (!MatchesCurrentRotation(rotation))
		{
			ResetForRotation(rotation, currentDailyPeriod);
		}
		else if (State.dailyPeriod != currentDailyPeriod)
		{
			RefreshForDailyReset();
			State.dailyPeriod = currentDailyPeriod;
		}
		foreach (CardSkinOfferSlot slot in State.slots)
		{
			if (!(slot.itemSkin != null) || applicationState.itemSkinManager.IsItemSkinUnlocked(slot.itemSkin))
			{
				slot.itemSkin = Pick(GetUnownedCandidates(slot.batch));
			}
		}
		UpdateLocalShelfMetadata();
	}

	public bool HasAnyOffer()
	{
		EnsureValidOffers();
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return State.hasAnyOffer;
		}
		if (IsNetworkShelfReady)
		{
			return State.hasAnyOffer;
		}
		return false;
	}

	public bool CanRefresh()
	{
		EnsureValidOffers();
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return State.canRefresh;
		}
		if (IsNetworkShelfReady)
		{
			return State.canRefresh;
		}
		return false;
	}

	public MatterInfo GetRefreshCost()
	{
		EnsureValidOffers();
		if (GlobalConfigManager.instance.enableNetwork)
		{
			return State.refreshCost;
		}
		return new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.creditCurrencyMatter, 30);
	}

	public void RefreshOffers(Action<bool> completed)
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			RefreshNetworkOffers(completed);
			return;
		}
		MatterInfo refreshCost = GetRefreshCost();
		bool flag = TryRefreshLocal(refreshCost);
		if (flag)
		{
			StateChanged?.Invoke();
		}
		completed?.Invoke(flag);
	}

	private bool TryRefreshLocal(MatterInfo refreshCost)
	{
		EnsureValidOffers();
		if (refreshCost?.matterDef == null || !State.canRefresh || !refreshCost.IsPayable(applicationState))
		{
			return false;
		}
		if (!applicationState.matterManager.ConsumeMatter(refreshCost, "卡牌形态商城刷新"))
		{
			return false;
		}
		foreach (CardSkinOfferSlot slot in State.slots)
		{
			List<ItemSkinDef> alternativeCandidates = GetAlternativeCandidates(slot);
			if (alternativeCandidates.Count > 0)
			{
				slot.itemSkin = Pick(alternativeCandidates);
			}
		}
		UpdateLocalShelfMetadata();
		return true;
	}

	public void RefillPurchasedSlot(CardSkinOfferSlot purchasedSlot)
	{
		if (purchasedSlot != null && State.slots.Contains(purchasedSlot))
		{
			purchasedSlot.itemSkin = Pick(GetUnownedCandidates(purchasedSlot.batch));
			UpdateLocalShelfMetadata();
			StateChanged?.Invoke();
		}
	}

	public bool CanPurchaseOffer(CardSkinOfferSlot slot)
	{
		EnsureValidOffers();
		if (GlobalConfigManager.instance.enableNetwork && !IsNetworkShelfReady)
		{
			return false;
		}
		if (slot?.itemSkin != null && State.slots.Contains(slot))
		{
			return !applicationState.itemSkinManager.IsItemSkinUnlocked(slot.itemSkin);
		}
		return false;
	}

	public List<GlobalRewardInfo> GeneratePurchaseReward(CardSkinOfferSlot slot)
	{
		if (GlobalConfigManager.instance.enableNetwork || !CanPurchaseOffer(slot))
		{
			return new List<GlobalRewardInfo>();
		}
		return new List<GlobalRewardInfo>
		{
			new GlobalRewardInfo
			{
				unlockItemSkin = slot.itemSkin
			}
		};
	}

	public RealMoneyToMatterPurchase GetPurchaseForSlot(CardSkinOfferSlot slot)
	{
		if (slot == null || slot.slotIndex < 0 || slot.slotIndex >= PurchaseIds.Length)
		{
			return null;
		}
		string purchaseId = PurchaseIds[slot.slotIndex];
		return ScriptableObjectSingleton<CalendarDataManager>.instance.skinPurchaseGroupDef?.purchases.OfType<RealMoneyToMatterPurchase>().FirstOrDefault((RealMoneyToMatterPurchase purchase) => purchase.purchaseId == purchaseId) ?? (applicationState.purchaseManager.GetPurchaseInfo(purchaseId) as RealMoneyToMatterPurchase);
	}

	public void PreparePurchase(CardSkinOfferSlot slot, RealMoneyToMatterPurchase purchase, Action<bool> completed)
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			completed?.Invoke(CanPurchaseOffer(slot));
			return;
		}
		if (!CanPurchaseOffer(slot) || purchase == null || requestInProgress || slot.slotIndex < 0 || slot.slotIndex >= PurchaseIds.Length || purchase.purchaseId != PurchaseIds[slot.slotIndex])
		{
			completed?.Invoke(obj: false);
			return;
		}
		requestInProgress = true;
		ClientPrepareCardSkinOfferPurchaseRequest request = new ClientPrepareCardSkinOfferPurchaseRequest
		{
			PurchaseId = purchase.purchaseId,
			SlotIndex = slot.slotIndex,
			ExpectedCardSkinId = slot.itemSkin.FullID,
			DailyPeriod = State.dailyPeriod,
			RefreshRevision = State.refreshRevision
		};
		GeneralGameAsyncOperation<ClientPrepareCardSkinOfferPurchaseRequest, CardSkinOfferShelfResult> operation = new GeneralGameAsyncOperation<ClientPrepareCardSkinOfferPurchaseRequest, CardSkinOfferShelfResult>(request, applicationState.ChillyRoomSDK.Purchase.PrepareCardSkinOfferPurchaseReliableAsync);
		operation.OnComplete(delegate
		{
			ApplyNetworkShelf(operation.OpResult);
			completed?.Invoke(obj: true);
		}).OnFault(delegate
		{
			completed?.Invoke(obj: false);
		}).OnOver(delegate
		{
			requestInProgress = false;
			StateChanged?.Invoke();
		})
			.ManualRetry()
			.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_preparing_card_skin_purchase);
	}

	public void RequestNetworkOffers(Action<bool> completed = null)
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			EnsureValidOffers();
			completed?.Invoke(obj: true);
			return;
		}
		if (requestInProgress)
		{
			WaitPanelBase.Token item = BootstrapExtra.instance.waitPanelBase.Show(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_network_communicating);
			pendingRequestWaitPanels.Add(item);
			if (completed != null)
			{
				pendingRequestCompleted.Add(completed);
			}
			return;
		}
		requestInProgress = true;
		currentRequestSuccess = false;
		GeneralGameAsyncOperationResultOnly<CardSkinOfferShelfResult> operation = new GeneralGameAsyncOperationResultOnly<CardSkinOfferShelfResult>(applicationState.ChillyRoomSDK.Purchase.GetCardSkinOffersAsync);
		operation.OnComplete(delegate
		{
			ApplyNetworkShelf(operation.OpResult);
			currentRequestSuccess = true;
			completed?.Invoke(obj: true);
		}).OnFault(delegate
		{
			networkShelfDirty = true;
			nextNetworkShelfRetryAtUtc = DateTime.UtcNow.Add(NetworkShelfRetryDelay);
			completed?.Invoke(obj: false);
		}).OnOver(delegate
		{
			requestInProgress = false;
			foreach (WaitPanelBase.Token pendingRequestWaitPanel in pendingRequestWaitPanels)
			{
				BootstrapExtra.instance.waitPanelBase.Hide(pendingRequestWaitPanel);
			}
			pendingRequestWaitPanels.Clear();
			foreach (Action<bool> item2 in pendingRequestCompleted)
			{
				item2(currentRequestSuccess);
			}
			pendingRequestCompleted.Clear();
			StateChanged?.Invoke();
		});
		operation.ManualRetry();
		operation.EnqueueRun(GameAsyncOperationLane.DuringLoading, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_card_skin_offers);
	}

	public void RequestNetworkOffersIfNeeded(Action<bool> completed = null)
	{
		if (IsNetworkShelfReady)
		{
			completed?.Invoke(obj: true);
		}
		else
		{
			RequestNetworkOffers(completed);
		}
	}

	public void UpdateNetworkShelfIfNeeded()
	{
		if (GlobalConfigManager.instance.enableNetwork && !IsNetworkShelfReady && !requestInProgress && !(DateTime.UtcNow < nextNetworkShelfRetryAtUtc))
		{
			networkShelfDirty = true;
			RequestNetworkOffers();
		}
	}

	public void ReloadNetworkOffers()
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			networkShelfDirty = true;
			nextNetworkShelfRetryAtUtc = default(DateTime);
			RequestNetworkOffers();
		}
	}

	private void RefreshNetworkOffers(Action<bool> completed)
	{
		if (!IsNetworkShelfReady || !State.canRefresh || requestInProgress)
		{
			completed?.Invoke(obj: false);
			return;
		}
		requestInProgress = true;
		MatterInfo chargedCost = State.refreshCost;
		GeneralGameAsyncOperationResultOnly<CardSkinOfferShelfResult> operation = new GeneralGameAsyncOperationResultOnly<CardSkinOfferShelfResult>(applicationState.ChillyRoomSDK.Purchase.RefreshCardSkinOffersAsync);
		operation.OnComplete(delegate
		{
			if (chargedCost != null)
			{
				applicationState.matterManager.ConsumeMatter(chargedCost, "卡牌形态商城刷新");
			}
			ApplyNetworkShelf(operation.OpResult);
			completed?.Invoke(obj: true);
		}).OnFault(delegate
		{
			networkShelfDirty = true;
			reconcileNetworkShelfAfterRequest = true;
			completed?.Invoke(obj: false);
		}).OnOver(delegate
		{
			requestInProgress = false;
			StateChanged?.Invoke();
			if (reconcileNetworkShelfAfterRequest)
			{
				reconcileNetworkShelfAfterRequest = false;
				nextNetworkShelfRetryAtUtc = default(DateTime);
				RequestNetworkOffers();
			}
		})
			.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_refreshing_card_skin_offers);
	}

	private void ApplyNetworkShelf(CardSkinOfferShelfResult result)
	{
		if (result == null)
		{
			return;
		}
		NetworkConvertContext networkConvertContext = new NetworkConvertContext(applicationState);
		List<ItemSkinSetBatch> source = CurrentRotation();
		State.slots.Clear();
		foreach (CardSkinOfferSlotResult offer in from cardSkinOfferSlotResult in result.Offers.OrEmptyIfNull()
			orderby cardSkinOfferSlotResult.SlotIndex
			select cardSkinOfferSlotResult)
		{
			ItemSkinSetBatch itemSkinSetBatch = source.FirstOrDefault((ItemSkinSetBatch candidate) => candidate.skinSetDef.FullID == offer.CardSkinSetId && candidate.batchIndex == offer.BatchIndex);
			ItemSkinDef itemSkinDef = ((offer.SoldOut || string.IsNullOrEmpty(offer.CardSkinId)) ? null : networkConvertContext.FullIdToCTAsset<ItemSkinDef>(offer.CardSkinId));
			if (!offer.SoldOut && (itemSkinSetBatch == null || itemSkinDef == null))
			{
				TraceLog.Error($"[CardSkinOfferController] 无法解析服务端心选卡牌 Slot={offer.SlotIndex}, Set={offer.CardSkinSetId}, Skin={offer.CardSkinId}");
			}
			State.slots.Add(new CardSkinOfferSlot
			{
				slotIndex = offer.SlotIndex,
				batch = itemSkinSetBatch,
				itemSkin = itemSkinDef
			});
		}
		State.dailyPeriod = result.DailyPeriod;
		State.refreshRevision = result.RefreshRevision;
		State.nextDailyRefreshAtUtc = NormalizeUtc(result.NextDailyRefreshAtUtc);
		State.refreshCost = result.RefreshCost?.ToClient(networkConvertContext);
		State.canRefresh = result.CanRefresh;
		State.hasAnyOffer = result.HasAnyOffer;
		State.loaded = true;
		networkShelfDirty = false;
		nextNetworkShelfRetryAtUtc = default(DateTime);
	}

	private bool IsNetworkShelfExpired()
	{
		if (State.nextDailyRefreshAtUtc != default(DateTime))
		{
			return State.nextDailyRefreshAtUtc <= GetServerUtcTime();
		}
		return false;
	}

	private static DateTime GetServerUtcTime()
	{
		try
		{
			return ServerTimePoller.Instance.GetCurrentTime().UtcDateTime;
		}
		catch
		{
			return DateTime.UtcNow;
		}
	}

	private static DateTime NormalizeUtc(DateTime value)
	{
		return value.Kind switch
		{
			DateTimeKind.Utc => value, 
			DateTimeKind.Local => value.ToUniversalTime(), 
			_ => DateTime.SpecifyKind(value, DateTimeKind.Utc), 
		};
	}

	private List<ItemSkinSetBatch> CurrentRotation()
	{
		return (ScriptableObjectSingleton<CalendarDataManager>.instance.skinPurchaseShuffle ?? new List<ItemSkinSetBatch>()).Where((ItemSkinSetBatch batch) => batch?.skinSetDef != null).Take(3).ToList();
	}

	private bool MatchesCurrentRotation(IReadOnlyList<ItemSkinSetBatch> rotation)
	{
		if (State.slots.Count != rotation.Count)
		{
			return false;
		}
		for (int i = 0; i < rotation.Count; i++)
		{
			ItemSkinSetBatch batch = State.slots[i].batch;
			ItemSkinSetBatch itemSkinSetBatch = rotation[i];
			if (batch?.skinSetDef != itemSkinSetBatch?.skinSetDef || batch?.batchIndex != itemSkinSetBatch?.batchIndex)
			{
				return false;
			}
		}
		return true;
	}

	private void ResetForRotation(IEnumerable<ItemSkinSetBatch> rotation, int dailyPeriod)
	{
		State.slots.Clear();
		int num = 0;
		foreach (ItemSkinSetBatch item in rotation)
		{
			State.slots.Add(new CardSkinOfferSlot
			{
				slotIndex = num++,
				batch = item,
				itemSkin = Pick(GetUnownedCandidates(item))
			});
		}
		State.dailyPeriod = dailyPeriod;
	}

	private void RefreshForDailyReset()
	{
		foreach (CardSkinOfferSlot slot in State.slots)
		{
			List<ItemSkinDef> unownedCandidates = GetUnownedCandidates(slot.batch);
			List<ItemSkinDef> list = unownedCandidates.Where((ItemSkinDef skin) => skin != slot.itemSkin).ToList();
			slot.itemSkin = Pick((list.Count > 0) ? list : unownedCandidates);
		}
	}

	private List<ItemSkinDef> GetUnownedCandidates(ItemSkinSetBatch batch)
	{
		if (batch?.skinSetDef == null)
		{
			return new List<ItemSkinDef>();
		}
		return (batch.GetSubSkins(ungot: false) ?? new List<ItemSkinDef>()).Where((ItemSkinDef skin) => skin != null && !applicationState.itemSkinManager.IsItemSkinUnlocked(skin)).Distinct().ToList();
	}

	private List<ItemSkinDef> GetAlternativeCandidates(CardSkinOfferSlot slot)
	{
		CardSkinOfferSlot cardSkinOfferSlot = slot;
		if (cardSkinOfferSlot == null || cardSkinOfferSlot.soldOut)
		{
			return new List<ItemSkinDef>();
		}
		return (from skin in GetUnownedCandidates(slot.batch)
			where skin != slot.itemSkin
			select skin).ToList();
	}

	private ItemSkinDef Pick(IReadOnlyList<ItemSkinDef> candidates)
	{
		if (candidates == null || candidates.Count == 0)
		{
			return null;
		}
		return candidates[Mathf.Clamp(randomIndex(candidates.Count), 0, candidates.Count - 1)];
	}

	private void UpdateLocalShelfMetadata()
	{
		State.loaded = true;
		State.hasAnyOffer = State.slots.Any((CardSkinOfferSlot slot) => !slot.soldOut);
		State.canRefresh = State.slots.Any((CardSkinOfferSlot slot) => GetAlternativeCandidates(slot).Count > 0);
		State.nextDailyRefreshAtUtc = ScriptableObjectSingleton<CalendarDataManager>.instance.GetDayPeriodEndTime().UtcDateTime;
		State.refreshCost = new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.creditCurrencyMatter, 30);
	}
}
