using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChillyRoom.Payment.V1;
using ChillyRoom.RealName.Utils;
using ChillyRoom.Services.Accounts.Features;
using ChillyRoom.Services.ContainerOfSoul;
using ChillyRoom.Services.Core;
using CoreKit.Agent;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

public class PurchaseManager : GlobalGameManager, IPostFetchData
{
	private class GoodsInfo
	{
		public Guid Id;

		public string ProID;

		public string ProductID;

		public string Name;

		public int Price;

		public string locPrice;
	}

	private class ItemConfigJson
	{
		public int count;

		public string id;

		public string productId;

		public string propId;
	}

	public class PayloadJson
	{
		public int count;

		public string productId;

		public string propId;
	}

	public class PurchaseHistoryRecord
	{
		public string orderId;

		public string purchaseItem;

		public DateTime purchaseDateTime;

		public string orderStatus;
	}

	[SaveField(null, null)]
	public List<PurchaseGroupDef> purchaseGroups;

	[SaveField(null, null)]
	public Dictionary<string, List<PurchaseRecord>> purchasedOrderHistory;

	[SaveField(null, null)]
	public List<PurchasePromotion> purchasePromotions;

	public List<LimitedTimeBundleInfo> limitedTimeBundleInfos;

	private CardSkinOfferController cardSkinOffers;

	private readonly List<GlobalRewardPackage> pendingRestoredOrderRewardPackages = new List<GlobalRewardPackage>();

	private readonly HashSet<long> handledReliableOrderIds = new HashSet<long>();

	private bool purchaseRelatedProfileRefreshInProgress;

	[NonSerialized]
	private bool purchasePeriodsInited;

	[NonSerialized]
	private int purchaseDailyPeriod;

	[NonSerialized]
	private int purchaseWeekPeriod;

	[NonSerialized]
	private int purchaseMonthPeriod;

	private const string CanonicalPurchasePropId = "00000110RealMoneyToMatterPurchase";

	private const string AlternatePurchasePropId = "00000110RMToMatterPurchase";

	private Dictionary<string, GoodsInfo> goodsInfos = new Dictionary<string, GoodsInfo>();

	public Dictionary<string, string> purchaseNameDict = new Dictionary<string, string>();

	public List<PurchaseHistoryRecord> orders = new List<PurchaseHistoryRecord>();

	public CardSkinOfferController CardSkinOffers => cardSkinOffers ?? (cardSkinOffers = new CardSkinOfferController(base.applicationState));

	private DateTime CurrentPurchaseUtcTime
	{
		get
		{
			if (!GlobalConfigManager.instance.enableNetwork)
			{
				return DateTime.UtcNow;
			}
			return ServerTimePoller.Instance.GetCurrentTime().UtcDateTime;
		}
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (purchaseGroups == null)
		{
			purchaseGroups = new List<PurchaseGroupDef>();
		}
		if (purchasePromotions == null)
		{
			purchasePromotions = ScriptableObjectSingleton<FortressDataManager>.instance.presetPurchasePromotions.Select((PurchasePromotion p) => p.Clone()).ToList();
		}
		if (limitedTimeBundleInfos == null)
		{
			limitedTimeBundleInfos = new List<LimitedTimeBundleInfo>();
		}
		if (purchasedOrderHistory == null)
		{
			purchasedOrderHistory = new Dictionary<string, List<PurchaseRecord>>();
		}
		cardSkinOffers = null;
		pendingRestoredOrderRewardPackages.Clear();
		handledReliableOrderIds.Clear();
	}

	IEnumerator IPostFetchData.PostFetchData()
	{
		InitPurchasesGroup();
		RecordActivePurchasePeriods();
		yield break;
	}

	private void InitPurchasesGroup()
	{
		if (purchaseGroups.IsNullOrEmpty())
		{
			purchaseGroups = new List<PurchaseGroupDef>();
			if (ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentWeeklyPurchaseGroup != null)
			{
				purchaseGroups.Add(ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentWeeklyPurchaseGroup);
			}
			if ((bool)ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentMonthlyPurchaseGroup)
			{
				purchaseGroups.Add(ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentMonthlyPurchaseGroup);
			}
			purchaseGroups.Add(ScriptableObjectSingleton<CalendarDataManager>.instance.seasonPurchaseGroup);
			PurchaseGroupDef purchaseGroupDef = base.applicationState.assetCatalog.purchaseGroupCatalog.ValidAssets.FirstOrDefault((PurchaseGroupDef p) => p.groupType == PurchaseGroupType.RealMoneyBuy);
			if (purchaseGroupDef != null)
			{
				purchaseGroups.Add(purchaseGroupDef);
			}
			PurchaseGroupDef purchaseGroupDef2 = base.applicationState.assetCatalog.purchaseGroupCatalog.ValidAssets.FirstOrDefault((PurchaseGroupDef p) => p.groupType == PurchaseGroupType.SeasonPass);
			if (purchaseGroupDef2 != null)
			{
				purchaseGroups.Add(purchaseGroupDef2);
			}
			List<PurchaseGroupDef> dailyStablePurchaseGroups = (ScriptableObjectSingleton<CalendarDataManager>.instance.dailyStablePurchaseGroupCalender ?? new List<PurchaseGroupDef>()).Where((PurchaseGroupDef p) => p != null).ToList();
			if (ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentDailyStablePurchaseGroup != null)
			{
				purchaseGroups.Add(ScriptableObjectSingleton<CalendarDataManager>.instance.CurrentDailyStablePurchaseGroup);
			}
			List<PurchaseGroupDef> list = base.applicationState.assetCatalog.purchaseGroupCatalog.ValidAssets.Where((PurchaseGroupDef p) => p.groupType == PurchaseGroupType.Stable).ToList();
			list.RemoveAll((PurchaseGroupDef p) => dailyStablePurchaseGroups.Contains(p));
			purchaseGroups.AddRange(list);
			purchaseGroups.RemoveAll((PurchaseGroupDef p) => p == null);
		}
	}

	public PurchaseInfo GetPurchaseInfo(string purchaseId)
	{
		return purchaseGroups.Where((PurchaseGroupDef pg) => pg != null && pg.purchases != null).SelectMany((PurchaseGroupDef pg) => pg.purchases).FirstOrDefault((PurchaseInfo p) => p != null && p.purchaseId == purchaseId);
	}

	public PurchaseGroupDef GetPurchaseGrouOfPurchase(string purchaseId)
	{
		PurchaseGroupDef purchaseGroupDef = purchaseGroups?.FirstOrDefault((PurchaseGroupDef pg) => pg != null && pg.purchases != null && pg.purchases.Any((PurchaseInfo p) => p != null && p.purchaseId == purchaseId));
		if (purchaseGroupDef != null)
		{
			return purchaseGroupDef;
		}
		return base.applicationState.assetCatalog.purchaseGroupCatalog.ValidAssets.FirstOrDefault((PurchaseGroupDef pg) => pg != null && pg.purchases != null && pg.purchases.Any((PurchaseInfo p) => p != null && p.purchaseId == purchaseId));
	}

	public IEnumerator PromoteFortressPurchaseCor()
	{
		return PromoteFortressPurchaseCor(null);
	}

	public IEnumerator PromoteFortressPurchaseCor(Action beforeVisibleInterruption)
	{
		IEnumerable<PurchasePromotion> enumerable = purchasePromotions.Where((PurchasePromotion p) => p?.IsPromotable(base.applicationState) ?? false);
		foreach (PurchasePromotion item in enumerable)
		{
			item.promoted = true;
			base.fortressState.uiManager.purchasePromotionPanel.Setup(item);
			beforeVisibleInterruption?.Invoke();
			base.fortressState.uiManager.purchasePromotionPanel.ShowUI();
			yield return new WaitUntil(() => !base.fortressState.uiManager.purchasePromotionPanel.gameObject.activeInHierarchy);
		}
	}

	public IEnumerator PromoteLimitedTimeBundleCor()
	{
		return PromoteLimitedTimeBundleCor(null);
	}

	public IEnumerator PromoteLimitedTimeBundleCor(Action beforeVisibleInterruption)
	{
		LimitedTimeBundlePanel limitedTimeBundlePanel = base.fortressState.uiManager.limitedTimeBundlePanel;
		LimitedTimeBundleInfo limitedTimeBundleInfo = base.applicationState.purchaseManager.limitedTimeBundleInfos?.FirstOrDefault((LimitedTimeBundleInfo info) => info != null && !info.PopupShown && info.EndTimeUtc.HasValue && info.EndTimeUtc.Value > DateTime.UtcNow);
		if (limitedTimeBundleInfo != null && base.applicationState.purchaseManager.GetPurchaseInfo(limitedTimeBundleInfo.PurchaseId) is RewardBundlePurchase && base.fortressState.uiManager.limitedTimeBundlePanel.Setup(limitedTimeBundleInfo))
		{
			limitedTimeBundleInfo.PopupShown = true;
			beforeVisibleInterruption?.Invoke();
			limitedTimeBundlePanel.ShowUI();
			MarkLimitedTimeBundlePopupShown(limitedTimeBundleInfo.PurchaseId);
			yield return new WaitUntil(() => !limitedTimeBundlePanel.gameObject.activeInHierarchy);
		}
	}

	private void MarkLimitedTimeBundlePopupShown(string purchaseId)
	{
		if (!string.IsNullOrWhiteSpace(purchaseId) && GlobalConfigManager.instance.enableNetwork)
		{
			new GeneralGameAsyncOperationResultOnly<LimitedTimeBundleInfo>(async () => await base.applicationState.ChillyRoomSDK.Purchase.MarkLimitedTimeBundlePopupShownReliableAsync(new MarkLimitedTimeBundlePopupShownRequest
			{
				PurchaseId = purchaseId
			})).HideFaultPanel().DoesNotBlockLoading().EnqueueRun(GameAsyncOperationLane.Background);
		}
	}

	public List<PurchaseGroupDef> GetPurchaseGroupShowInMainPurchasePanel()
	{
		return (from g in purchaseGroups
			where g.ShowInMainPurchasePanel
			orderby g.layoutSortPriority descending
			select g).ToList();
	}

	private static string CanonicalizePurchasePropId(string propId)
	{
		if (!string.Equals(propId, "00000110RMToMatterPurchase", StringComparison.Ordinal))
		{
			return propId;
		}
		return "00000110RealMoneyToMatterPurchase";
	}

	private static bool MatchSkuItemConfigs(string candidateItemConfig, string currentItemConfig)
	{
		if (string.IsNullOrEmpty(candidateItemConfig) || string.IsNullOrEmpty(currentItemConfig))
		{
			return false;
		}
		try
		{
			string text = JsonConvert.DeserializeObject<GenericItemConfig>(candidateItemConfig)?.PropId;
			string text2 = JsonConvert.DeserializeObject<GenericItemConfig>(currentItemConfig)?.PropId;
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
			{
				return false;
			}
			return string.Equals(CanonicalizePurchasePropId(text), CanonicalizePurchasePropId(text2), StringComparison.Ordinal);
		}
		catch (JsonException)
		{
			return false;
		}
	}

	public string LocPrice(string propId)
	{
		if (!goodsInfos.TryGetValue(propId, out var value))
		{
			return string.Empty;
		}
		return value.locPrice;
	}

	public IEnumerator RequestSkuList()
	{
		TraceLog.Debug("RequestSkuList Should Run Once");
		AgentHub.MatchSkuFunc = MatchSkuItemConfigs;
		goodsInfos.Clear();
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			yield break;
		}
		GeneralGameAsyncOperationResultOnly<IResult<List<SKU>, Failure<ListSkuErrorKind>>> op = new GeneralGameAsyncOperationResultOnly<IResult<List<SKU>, Failure<ListSkuErrorKind>>>(async () => await base.applicationState.ChillyRoomSDK.Payment.ListSku());
		op.OnComplete(delegate
		{
			if (op.OpResult.IsOk())
			{
				foreach (SKU datum in op.OpResult.Data)
				{
					TraceLog.Debug($"[Dump sku] id: {datum.Id}, type: {datum.Type}, name: {datum.Name}, desc: {datum.Desc}, price: {datum.Price}, loc_price: {datum.Loc_price}, itemConfig: {datum.Item_config}, payType: {datum.Pay_type}");
					ItemConfigJson itemConfigJson = JsonUtility.FromJson<ItemConfigJson>(datum.Item_config);
					GoodsInfo goodsInfo = new GoodsInfo
					{
						Id = datum.Id,
						ProID = itemConfigJson.propId,
						ProductID = itemConfigJson.productId,
						Name = datum.Name,
						Price = datum.Price,
						locPrice = datum.Loc_price
					};
					TraceLog.Debug("goodsInfos add 商品：" + goodsInfo.ProID + " price:  " + goodsInfo.Price + " ProductID:  " + goodsInfo.ProductID + " Name:  " + goodsInfo.Name);
					goodsInfos.Add(CanonicalizePurchasePropId(itemConfigJson.propId), goodsInfo);
				}
				return;
			}
			TraceLog.Error("获取SKU列表失败");
		}).HideFaultPanel().DoesNotBlockLoading()
			.EnqueueRun(GameAsyncOperationLane.DuringLoading, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_purchase_data);
		yield return op.WaitForEnd();
	}

	public void PreparePurchaseName()
	{
		purchaseNameDict.Clear();
		foreach (PurchaseGroupDef validAsset in base.applicationState.assetCatalog.purchaseGroupCatalog.ValidAssets)
		{
			foreach (PurchaseInfo purchase in validAsset.purchases)
			{
				purchaseNameDict.Add(purchase.purchaseId, purchase.PurchaseTitleText);
			}
		}
	}

	public void ListOrders(Action cb)
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		TraceLog.Debug("[PurchaseManager]: ListOrders");
		GeneralGameAsyncOperationResultOnly<IResult<List<Payment.ClientOrder>, Failure<ListOrderErrorKind>>> op = new GeneralGameAsyncOperationResultOnly<IResult<List<Payment.ClientOrder>, Failure<ListOrderErrorKind>>>(async () => await base.applicationState.ChillyRoomSDK.Payment.ListOrder());
		op.OnComplete(delegate
		{
			if (op.OpResult.IsOk())
			{
				List<Payment.ClientOrder> data = op.OpResult.Data;
				orders.Clear();
				foreach (Payment.ClientOrder item2 in data)
				{
					TraceLog.Debug($"[PurchaseManager]: {item2.Order.Id} {item2.Order.Payload} {item2.Order.Created_at.UtcDateTime} {item2.Order.Status}");
					if (!string.IsNullOrEmpty(item2.Order.Payload))
					{
						string key = CanonicalizePurchasePropId(JsonConvert.DeserializeObject<PayloadJson>(item2.Order.Payload).propId);
						PurchaseHistoryRecord item = new PurchaseHistoryRecord
						{
							orderId = item2.Order.Id.ToString(),
							purchaseItem = purchaseNameDict[key],
							purchaseDateTime = item2.Order.Created_at.UtcDateTime,
							orderStatus = GetPurchaseStatusText(item2.Order.Status)
						};
						orders.Add(item);
					}
				}
				cb();
				TraceLog.Debug($"<color=green>触发了订单列表, 列表: {orders.Count}</color>");
			}
			else
			{
				TraceLog.Error("触发订单列表失败");
			}
		}).HideFaultPanel().EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_order_data);
	}

	private string GetPurchaseStatusText(ChillyRoom.Payment.V1.OrderStatus status)
	{
		return status switch
		{
			ChillyRoom.Payment.V1.OrderStatus.Claimed => ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_history_order_claimed, 
			ChillyRoom.Payment.V1.OrderStatus.Placed => ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_history_order_placed, 
			ChillyRoom.Payment.V1.OrderStatus.Canceled => ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_history_order_canceled, 
			ChillyRoom.Payment.V1.OrderStatus.Refunded => ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_history_order_refunded, 
			_ => status.ToString(), 
		};
	}

	private bool ActivePurchasePeriodChanged()
	{
		if (!purchasePeriodsInited)
		{
			RecordActivePurchasePeriods();
			return false;
		}
		DateTime currentPurchaseUtcTime = CurrentPurchaseUtcTime;
		if (purchaseDailyPeriod == ScriptableObjectSingleton<CalendarDataManager>.instance.GetDailyPeriod(currentPurchaseUtcTime) && purchaseWeekPeriod == ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriod(currentPurchaseUtcTime))
		{
			return purchaseMonthPeriod != ScriptableObjectSingleton<CalendarDataManager>.instance.GetMonthPeriod(currentPurchaseUtcTime);
		}
		return true;
	}

	private void RecordActivePurchasePeriods()
	{
		DateTime currentPurchaseUtcTime = CurrentPurchaseUtcTime;
		purchaseDailyPeriod = ScriptableObjectSingleton<CalendarDataManager>.instance.GetDailyPeriod(currentPurchaseUtcTime);
		purchaseWeekPeriod = ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriod(currentPurchaseUtcTime);
		purchaseMonthPeriod = ScriptableObjectSingleton<CalendarDataManager>.instance.GetMonthPeriod(currentPurchaseUtcTime);
		purchasePeriodsInited = true;
	}

	public bool ShouldRefreshRecord()
	{
		if (purchaseRelatedProfileRefreshInProgress || !ActivePurchasePeriodChanged())
		{
			return false;
		}
		return true;
	}

	public IEnumerator RefreshPurchaseProfileData(Action onRefreshed)
	{
		if (!ShouldRefreshRecord())
		{
			yield break;
		}
		purchaseRelatedProfileRefreshInProgress = true;
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			RecordActivePurchasePeriods();
			onRefreshed?.Invoke();
			purchaseRelatedProfileRefreshInProgress = false;
			yield break;
		}
		GeneralGameAsyncOperation<FetchProfileDataReq, PlayerProfileData> fetchProfileDataOp = new GeneralGameAsyncOperation<FetchProfileDataReq, PlayerProfileData>(new FetchProfileDataReq
		{
			DateTimeUtcOffsetOfMinutes = (int)DateTimeOffset.Now.Offset.TotalMinutes
		}, base.applicationState.ChillyRoomSDK.ProfileData.FetchPlayerProfileDataAsync);
		fetchProfileDataOp.ManualRetry().EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_player_data);
		yield return fetchProfileDataOp.WaitForEnd();
		if (fetchProfileDataOp.isComplete)
		{
			try
			{
				ApplyPurchaseRelatedProfileData(fetchProfileDataOp.OpResult);
				base.applicationState.savingManager.SetProfileDirtyToSave();
				onRefreshed?.Invoke();
			}
			catch (Exception arg)
			{
				TraceLog.Error($"[PurchaseManager] RefreshPurchaseRelatedProfileDataIfPeriodChangedCor() - {arg}");
			}
		}
		purchaseRelatedProfileRefreshInProgress = false;
	}

	private void ApplyPurchaseRelatedProfileData(PlayerProfileData profileData)
	{
		NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
		base.applicationState.playerInfoManager.vip = profileData.Vip;
		base.applicationState.playerInfoManager.monthlyCardExpireDayPeriod = profileData.MonthlyCardExpireDayPeriod;
		base.applicationState.matterManager.matterInfos = profileData.MatterInfos.Select((ChillyRoom.Services.ContainerOfSoul.MatterInfo m) => m.ToClient(context)).WhereNotNull().ToList();
		base.applicationState.itemSkinManager.unlockedItemSkinLevels = (from kv in profileData.CardSkinUnlockLevels
			select new ItemSkinManager.ItemSkinState
			{
				skinDef = context.FullIdToCTAsset<ItemSkinDef>(kv.Key),
				level = kv.Value
			} into state
			where state.skinDef != null
			select state).ToList();
		base.applicationState.itemSkinManager.RebuildCache();
		purchaseGroups = profileData.ActivePurchaseGroups.Select(context.FullIdToCTAsset<PurchaseGroupDef>).WhereNotNull().ToList();
		purchasedOrderHistory = profileData.PurchaseRecords.ToDictionary((KeyValuePair<string, List<ChillyRoom.Services.ContainerOfSoul.PurchaseRecord>> record) => record.Key, (KeyValuePair<string, List<ChillyRoom.Services.ContainerOfSoul.PurchaseRecord>> record) => record.Value.Select((ChillyRoom.Services.ContainerOfSoul.PurchaseRecord p) => p.ToClient(context)).WhereNotNull().ToList());
		limitedTimeBundleInfos = profileData.LimitedTimeBundleInfos?.ToList() ?? new List<LimitedTimeBundleInfo>();
		RecordActivePurchasePeriods();
	}

	public async Task<bool> RestoreOrders()
	{
		if (Application.isEditor || !GlobalConfigManager.instance.enableNetwork)
		{
			return true;
		}
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug("[PurchaseManager] RestoreOrders()");
		}
		IResult<Payment.RestoreOrdersResult, Failure<ListOrderErrorKind>> result = await base.applicationState.ChillyRoomSDK.Payment.RestoreOrders();
		if (!result.IsOk())
		{
			TraceLog.Error($"[PurchaseManager]: RestoreOrders() - failed - Error: {result.Error}");
			return false;
		}
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug("[PurchaseManager] RestoreOrders() start poll orders");
		}
		List<long> orderIds = ShowRestoredOrderRewards(await StartPurchaseInfoPolling(1));
		await AcknowledgeOrdersAsync(orderIds);
		return true;
	}

	private List<long> ShowRestoredOrderRewards(ICollection<OrderResult> orderResults)
	{
		if (orderResults == null || orderResults.Count == 0)
		{
			return new List<long>();
		}
		NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
		List<OrderResult> list = orderResults.Where((OrderResult order) => order?.Result?.Record != null && order.Result.Rewards != null && !handledReliableOrderIds.Contains(order.OrderId)).ToList();
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(GlobalRewardInfo.UnfoldNestedReward(list.SelectMany((OrderResult order) => order.Result.Rewards.Select((ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo r) => r.ToClient(context))).ToList()));
		globalRewardPackage.title = ScriptableObjectSingleton<TextDataPreset>.instance.term_restore_order_purchase_items;
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug($"[PurchaseManager] ShowRestoredOrderRewards() got package - {globalRewardPackage}");
		}
		if (list.Count > 0)
		{
			globalRewardPackage.GiveAll(base.applicationState);
			foreach (OrderResult item in list)
			{
				RecordProcessedOrder(item.PurchaseId, item.Result.Record.ToClient(context));
				handledReliableOrderIds.Add(item.OrderId);
			}
			if (base.fortressState != null)
			{
				base.fortressState.uiManager.globalRewardPackageDisplay.Setup(globalRewardPackage);
			}
			else
			{
				pendingRestoredOrderRewardPackages.Add(globalRewardPackage);
			}
			return list.Select((OrderResult order) => order.OrderId).ToList();
		}
		return new List<long>();
	}

	public IEnumerator ShowPendingRestoredOrderRewardsCor()
	{
		return ShowPendingRestoredOrderRewardsCor(null);
	}

	public IEnumerator ShowPendingRestoredOrderRewardsCor(Action beforeVisibleInterruption)
	{
		if (pendingRestoredOrderRewardPackages.Count == 0)
		{
			yield break;
		}
		beforeVisibleInterruption?.Invoke();
		List<GlobalRewardPackage> list = pendingRestoredOrderRewardPackages.ToList();
		pendingRestoredOrderRewardPackages.Clear();
		foreach (GlobalRewardPackage item in list)
		{
			yield return base.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(item);
		}
	}

	public void DoPurchase(PurchaseInfo purchaseInfo, int times, Action<GlobalRewardPackage> afterPurchase)
	{
		TraceLog.Debug("[PurchaseInfo] DoPurchase()");
		DoPurchaseAsync(purchaseInfo, times, afterPurchase, null);
	}

	public void DoPurchase(PurchaseInfo purchaseInfo, int times, Action<GlobalRewardPackage> afterPurchase, Action<bool> afterFinished)
	{
		TraceLog.Debug("[PurchaseInfo] DoPurchase()");
		DoPurchaseAsync(purchaseInfo, times, afterPurchase, afterFinished);
	}

	public bool TryDoLocalPurchase(PurchaseInfo purchaseInfo, int times, List<GlobalRewardInfo> rewards, string rewardTitle, Action<GlobalRewardPackage> afterPurchase)
	{
		if (GlobalConfigManager.instance.enableNetwork || purchaseInfo == null || times <= 0 || rewards == null || !purchaseInfo.IsEnoughCost(base.applicationState, times) || !purchaseInfo.IsAllowPurchase(base.applicationState))
		{
			return false;
		}
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(rewards)
		{
			title = (rewardTitle ?? purchaseInfo.GlobalItemTitle)
		};
		ApplyPurchase(purchaseInfo, times, new PurchaseRecord
		{
			quantity = times,
			purchaseDateTime = DateTime.UtcNow,
			orderStatus = ChillyRoom.Services.ContainerOfSoul.OrderStatus.Paid
		}, globalRewardPackage);
		afterPurchase?.Invoke(globalRewardPackage);
		return true;
	}

	private async Task DoPurchaseAsync(PurchaseInfo purchaseInfo, int times, Action<GlobalRewardPackage> afterPurchase, Action<bool> afterFinished)
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(purchaseInfo.GenerateReward(base.applicationState, times));
			globalRewardPackage.title = purchaseInfo.GlobalItemTitle;
			ApplyPurchase(purchaseInfo, times, new PurchaseRecord
			{
				quantity = times,
				purchaseDateTime = DateTime.UtcNow,
				orderStatus = ChillyRoom.Services.ContainerOfSoul.OrderStatus.Paid
			}, globalRewardPackage);
			afterPurchase?.Invoke(globalRewardPackage);
			afterFinished?.Invoke(obj: true);
			return;
		}
		PurchaseItemRequest req = new PurchaseItemRequest(purchaseInfo, times);
		PurchaseItemResult result = null;
		if (purchaseInfo.realMoneyCost > 0)
		{
			WaitPanelBase.Token waitPanel = BootstrapExtra.instance.waitPanelBase.Show(base.textData.gameAsyncOperationText.term_order);
			result = await DoRealMoneyPurchase(req, purchaseInfo);
			BootstrapExtra.instance.waitPanelBase.Hide(waitPanel);
		}
		else if (!base.applicationState.playerInfoManager.vip && purchaseInfo is WatchAdGetMatterPurchase)
		{
			bool finished = false;
			Bootstrap.instance.adsManager.ShowAd("free_money", delegate
			{
				TraceLog.Debug("广告播放完成");
				SendPurchaseReq(req, delegate(PurchaseItemResult r)
				{
					finished = true;
					result = r;
				});
			}, delegate
			{
				TraceLog.Debug("广告播放失败free_money");
				finished = true;
			}, delegate
			{
				TraceLog.Debug("广告播放中断");
				finished = true;
			});
			await TaskHelper.Wait(() => finished);
		}
		else
		{
			bool finished2 = false;
			SendPurchaseReq(req, delegate(PurchaseItemResult r)
			{
				finished2 = true;
				result = r;
			});
			await TaskHelper.Wait(() => finished2);
		}
		ThinkingAnalyticsExtends.TrackFinishPurchase(base.applicationState, purchaseInfo);
		if (result == null)
		{
			afterFinished?.Invoke(obj: false);
			return;
		}
		NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
		GlobalRewardPackage globalRewardPackage2 = new GlobalRewardPackage(GlobalRewardInfo.UnfoldNestedReward(result.Rewards.Select((ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo r) => r.ToClient(context)).ToList()));
		PurchaseRecord purchaseRecord = result.Record.ToClient(context);
		long valueOrDefault = result.Record.OrderId.GetValueOrDefault();
		if (valueOrDefault > 0 && handledReliableOrderIds.Contains(valueOrDefault))
		{
			await AcknowledgeOrdersAsync(new long[1] { valueOrDefault });
			afterFinished?.Invoke(obj: true);
			return;
		}
		MatterInfo authoritativeMatterCost = result.MatterCost?.ToClient(context);
		ApplyPurchase(purchaseInfo, req.Times, purchaseRecord, globalRewardPackage2, authoritativeMatterCost);
		afterPurchase?.Invoke(globalRewardPackage2);
		if (valueOrDefault > 0)
		{
			handledReliableOrderIds.Add(valueOrDefault);
		}
		await AcknowledgeOrdersAsync(new long[1] { valueOrDefault });
		afterFinished?.Invoke(obj: true);
	}

	private void ApplyPurchase(PurchaseInfo purchaseInfo, int purchaseTimes, PurchaseRecord purchaseRecord, GlobalRewardPackage rewardPackage, MatterInfo authoritativeMatterCost = null)
	{
		MatterInfo matterInfo = authoritativeMatterCost;
		if (matterInfo == null)
		{
			MatterInfo matterInfo2 = purchaseInfo.CurrentMatterCost(base.applicationState);
			if (matterInfo2 != null)
			{
				matterInfo = new MatterInfo(matterInfo2.matterDef, matterInfo2.count * purchaseTimes);
			}
		}
		if (matterInfo != null)
		{
			base.applicationState.matterManager.ConsumeMatter(matterInfo, "商店购买/兑换");
		}
		rewardPackage.GiveAll(base.applicationState);
		if (purchaseInfo.realMoneyCost > 0)
		{
			base.applicationState.eventManager.purchasePayEvent.TriggerEvent(purchaseInfo.realMoneyCost * purchaseTimes * 100);
			base.applicationState.newbieFestivalManager?.AddLocalPaidPurchaseProgress(purchaseInfo.realMoneyCost, purchaseTimes);
		}
		RecordProcessedOrder(purchaseInfo.purchaseId, purchaseRecord);
	}

	private void RecordProcessedOrder(string purchaseId, PurchaseRecord purchaseRecord)
	{
		if (!string.IsNullOrEmpty(purchaseId) && purchaseRecord != null && (string.IsNullOrEmpty(purchaseRecord.orderId) || !purchasedOrderHistory.TryGetValue(purchaseId, out var value) || !value.Any((PurchaseRecord record) => record.orderId == purchaseRecord.orderId)))
		{
			if (!purchasedOrderHistory.TryGetValue(purchaseId, out var value2))
			{
				value2 = new List<PurchaseRecord>();
				purchasedOrderHistory[purchaseId] = value2;
			}
			value2.Add(purchaseRecord);
		}
	}

	private void SendPurchaseReq(PurchaseItemRequest req, Action<PurchaseItemResult> callback)
	{
		GeneralGameAsyncOperation<PurchaseItemRequest, PurchaseItemResult> op = new GeneralGameAsyncOperation<PurchaseItemRequest, PurchaseItemResult>(req, base.applicationState.ChillyRoomSDK.Purchase.PurchaseItemAsync);
		op.OnComplete(delegate
		{
			callback(op.OpResult);
		}).ManualRetry().EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_processing_purchase);
	}

	private async Task<PurchaseItemResult> DoRealMoneyPurchase(PurchaseItemRequest req, PurchaseInfo purchaseInfo)
	{
		if (goodsInfos.Count == 0)
		{
			TraceLog.Debug("[PurchaseManager] DoRealMoneyPurchase() - 未拉取到sku");
			return null;
		}
		if (!goodsInfos.TryGetValue(req.PurchaseId, out var sku))
		{
			TraceLog.Error("[PurchaseManager] DoRealMoneyPurchase() - 商品不存在");
			return null;
		}
		TraceLog.Debug("[PurchaseManager] DoRealMoneyPurchase()");
		ThinkingAnalyticsExtends.TrackRealMoneyPurchaseProgress(base.applicationState, purchaseInfo, "start");
		IResult<Order, Failure<PurchaseErrorKind>> result = await RealMoneyPurchaseRequest(sku);
		bool flag = result.IsOk();
		long? id = result.Data?.Id;
		string orderId = null;
		if (result.Error?.Details != null)
		{
			result.Error.Details.TryGetValue("order_id", out orderId);
		}
		TraceLog.Debug($"[PurchaseManager] DoRealMoneyPurchase() \n- {flag}\n- id {id}\n- order_id {orderId}\n- {result.Error}");
		if (!result.IsOk())
		{
			string orderId2 = Guid.NewGuid().ToString();
			if (result.Error?.Details != null && result.Error.Details.TryGetValue("order_id", out var value))
			{
				orderId2 = value;
			}
			Failure<PurchaseErrorKind> error = result.Error;
			if (error == null)
			{
				return null;
			}
			if (error.Error == PurchaseErrorKind.NativePayFailure)
			{
				TraceLog.Debug("[PurchaseManager] DoRealMoneyPurchase() - 用户取消支付");
				ThinkingAnalyticsExtends.TrackRealMoneyPurchaseProgress(base.applicationState, purchaseInfo, "sdk failure", error.Details);
				return null;
			}
			if (error.Error != PurchaseErrorKind.OrderAlreadyClaimed)
			{
				TraceLog.Debug("[PurchaseManager] DoRealMoneyPurchase() - 购买失败：" + result.Error);
				ThinkingAnalyticsExtends.TrackRealMoneyPurchaseProgress(base.applicationState, purchaseInfo, "unknown error", error.Details);
				SolarEngineManager.TrackPurchase(sku.Name, sku.ProductID, sku.Price, success: false, orderId2, "weixin");
				return null;
			}
			if (error.Details != null && orderId != null)
			{
				TraceLog.Warn("[PurchaseManager] DoRealMoneyPurchase() - 订单已被认领过，订单ID: " + orderId + ", 进行订单发货轮询");
			}
		}
		TraceLog.Debug("[PurchaseManager] DoRealMoneyPurchase() - 购买成功，进行订单发货轮询");
		List<OrderResult> list = await DoOrderPolling();
		if (list != null && list.Count > 0)
		{
			TraceLog.Debug($"[PurchaseManager] DoRealMoneyPurchase() - poll success - {list.Count}");
			try
			{
				SolarEngineManager.TrackPurchase(sku.Name, sku.ProductID, sku.Price, success: true, id.ToString());
			}
			catch (Exception arg)
			{
				TraceLog.Error($"[PurchaseManager] DoRealMoneyPurchase() - e {arg}");
			}
			ThinkingAnalyticsExtends.TrackRealMoneyPurchaseProgress(base.applicationState, purchaseInfo, "success");
			string expectedOrderId = ((!string.IsNullOrEmpty(id?.ToString())) ? id.ToString() : orderId);
			OrderResult orderResult = ((!string.IsNullOrEmpty(expectedOrderId)) ? list.FirstOrDefault((OrderResult orderResult2) => orderResult2.OrderId.ToString() == expectedOrderId) : null);
			if (orderResult == null)
			{
				orderResult = (from orderResult2 in list
					where orderResult2.PurchaseId == req.PurchaseId
					orderby orderResult2.Result?.Record?.PurchaseTime ?? DateTime.MinValue descending
					select orderResult2).FirstOrDefault();
			}
			if (orderResult == null)
			{
				TraceLog.Warn("[PurchaseManager] DoRealMoneyPurchase() - 未找到当前SKU对应的待确认订单: " + req.PurchaseId);
				return null;
			}
			return new PurchaseItemResult
			{
				Rewards = orderResult.Result.Rewards,
				Record = orderResult.Result.Record
			};
		}
		TraceLog.Debug("[PurchaseManager] DoRealMoneyPurchase() - poll failure");
		ThinkingAnalyticsExtends.TrackRealMoneyPurchaseProgress(base.applicationState, purchaseInfo, "pull failure");
		return null;
		async Task<List<OrderResult>> DoOrderPolling()
		{
			try
			{
				ICollection<OrderResult> collection = await StartPurchaseInfoPolling();
				if (collection != null && collection.Count > 0)
				{
					TraceLog.Debug($"[PurchaseManager] DoOrderPolling() - get result - {collection.Count}");
					foreach (OrderResult item in collection)
					{
						TraceLog.Debug($"[PurchaseManager] DoOrderPolling() - {item.OrderId}");
					}
					return collection.ToList();
				}
			}
			catch (Exception ex)
			{
				TraceLog.Error("[PurchaseManager] DoOrderPolling() - e: " + ex.Message);
			}
			return null;
		}
	}

	private async Task<IResult<Order, Failure<PurchaseErrorKind>>> RealMoneyPurchaseRequest(GoodsInfo sku)
	{
		double value = 60.0;
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			value = 130.0;
		}
		return await base.applicationState.ChillyRoomSDK.Payment.Purchase(sku.Id, 1, sku.Price).Timeout(TimeSpan.FromSeconds(value)).Catch<IResult<Order, Failure<PurchaseErrorKind>>, TimeoutException>(delegate
		{
			TraceLog.Error("[PurchaseManager] PayWtithSdk() - 支付超时");
			return Observable.Return(Result<Order, Failure<PurchaseErrorKind>>.Err(new Failure<PurchaseErrorKind>
			{
				Error = PurchaseErrorKind.Timeout
			}));
		})
			.Catch<IResult<Order, Failure<PurchaseErrorKind>>, Exception>(delegate
			{
				TraceLog.Error("[PurchaseManager] PayWtithSdk() - Payment.Purchase failed, unknow error");
				return Observable.Return(Result<Order, Failure<PurchaseErrorKind>>.Err(new Failure<PurchaseErrorKind>
				{
					Error = PurchaseErrorKind.Cancelled
				}));
			})
			.ToTask();
	}

	private async Task<ICollection<OrderResult>> StartPurchaseInfoPolling(int retryTime = 10)
	{
		TraceLog.Debug("[PurchaseManager] StartPurchaseInfoPolling()");
		int times = 0;
		while (times < retryTime)
		{
			if (TraceLog.EnabledLog)
			{
				TraceLog.Debug($"[PurchaseManager] StartPurchaseInfoPolling() start - {times}");
			}
			ICollection<OrderResult> collection = await base.applicationState.ChillyRoomSDK.Purchase.ConfirmOrderReliableAsync();
			if (collection == null || collection.Count <= 0)
			{
				if (TraceLog.EnabledLog)
				{
					TraceLog.Debug($"[PurchaseManager] StartPurchaseInfoPolling() no result - {times}");
				}
				times++;
				await Task.Delay(1000);
				continue;
			}
			if (TraceLog.EnabledLog)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (OrderResult item in collection)
				{
					stringBuilder.Append($"${item.OrderId}");
				}
				if (TraceLog.EnabledLog)
				{
					TraceLog.Debug($"[PurchaseManager] StartPurchaseInfoPolling() success - {stringBuilder}");
				}
			}
			return collection;
		}
		return null;
	}

	private async Task AcknowledgeOrdersAsync(IEnumerable<long> orderIds)
	{
		List<long> ids = orderIds.Where((long orderId) => orderId > 0).Distinct().ToList();
		if (ids.Count == 0)
		{
			return;
		}
		try
		{
			ClientAcknowledgeOrdersResult clientAcknowledgeOrdersResult = await base.applicationState.ChillyRoomSDK.Purchase.AcknowledgeOrdersReliableAsync(new ClientAcknowledgeOrdersRequest
			{
				OrderIds = ids
			});
			if (clientAcknowledgeOrdersResult?.AcknowledgedOrderIds == null || clientAcknowledgeOrdersResult.AcknowledgedOrderIds.Count != ids.Count)
			{
				TraceLog.Warn($"[PurchaseManager] AcknowledgeOrdersAsync() - 确认数量不一致, request={ids.Count}, response={(clientAcknowledgeOrdersResult?.AcknowledgedOrderIds?.Count).GetValueOrDefault()}");
			}
		}
		catch (Exception ex)
		{
			TraceLog.Warn("[PurchaseManager] AcknowledgeOrdersAsync() - " + ex.Message);
		}
	}
}
