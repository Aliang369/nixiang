using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.Services.ContainerOfSoul;
using GameRandom;
using UnityEngine;

public class GachaManager : GlobalGameManager
{
	public sealed class GachaHistorySyncBatch
	{
		public List<GachaHistorySyncSnapshot> snapshots = new List<GachaHistorySyncSnapshot>();
	}

	public sealed class GachaHistorySyncSnapshot
	{
		public string gachaPoolId;

		public int totalCount;

		public List<GachaDrawHistory> replacementHistory;
	}

	public sealed class GachaHistoryDisplaySnapshot
	{
		public int totalCount;

		public List<string> gachaPoolIds;

		public List<GachaDrawHistory> history;
	}

	private sealed class GachaHistorySyncTarget
	{
		public string gachaPoolId;

		public int localCount;
	}

	private const int FullHistorySyncLimit = 5000;

	private const int HistoryPageSize = 100;

	[SaveField(null, null)]
	public List<GachaPoolState> gachaPoolStates;

	[SaveField(null, null)]
	public List<GachaPityState> gachaPityStates;

	private HashSet<string> syncingGachaPoolIds = new HashSet<string>();

	public PurchaseGroupDef hiddenPurchaseGroup;

	public bool hasNew => gachaPoolStates.Any((GachaPoolState s) => !s.viewed && s.valid);

	public GachaPoolState defaultState => GetGachaPoolState(ScriptableObjectSingleton<FortressDataManager>.instance.baseGachaPoolDef);

	public PurchaseInfo ConvertToGachaTicketMatterPurchase => hiddenPurchaseGroup.purchases.OfType<ConvertMatterToMatterPurchase>().FirstOrDefault((ConvertMatterToMatterPurchase p) => !p.buyTimesLimit && p.gain.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.gachaTicketMatter);

	public PurchaseInfo ConvertToLimitGachaTicketMatterPurchase => hiddenPurchaseGroup.purchases.OfType<ConvertMatterToMatterPurchase>().FirstOrDefault((ConvertMatterToMatterPurchase p) => !p.buyTimesLimit && p.gain.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.limitGachaTicketMatter);

	public GachaPoolState GetGachaPoolState(GachaPoolDef gachaPoolDef)
	{
		return gachaPoolStates.FirstOrDefault((GachaPoolState s) => s.def == gachaPoolDef);
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (syncingGachaPoolIds == null)
		{
			syncingGachaPoolIds = new HashSet<string>();
		}
		if (gachaPoolStates == null)
		{
			gachaPoolStates = new List<GachaPoolState>();
			foreach (GachaPoolDef validAsset in applicationState.assetCatalog.gachaPoolCatalog.ValidAssets)
			{
				gachaPoolStates.Add(new GachaPoolState(validAsset));
			}
		}
		if (gachaPityStates == null)
		{
			gachaPityStates = new List<GachaPityState>();
			RebuildPityStatesFromHistory();
		}
		hiddenPurchaseGroup = applicationState.assetCatalog.purchaseGroupCatalog.ValidAssets.FirstOrDefault((PurchaseGroupDef p) => p.groupType == PurchaseGroupType.None);
	}

	public GeneralGameAsyncOperationResultOnly<GachaHistorySyncBatch> CreateHistorySynchronizationOperation()
	{
		return CreateHistorySynchronizationOperation(gachaPoolStates);
	}

	private GeneralGameAsyncOperationResultOnly<GachaHistorySyncBatch> CreateHistorySynchronizationOperation(IEnumerable<GachaPoolState> pools)
	{
		List<GachaPoolState> list = pools.Where((GachaPoolState state) => state?.def != null).Distinct().ToList();
		List<GachaHistorySyncTarget> targets = list.Select((GachaPoolState state) => new GachaHistorySyncTarget
		{
			gachaPoolId = state.def.FullID,
			localCount = (state.gachaDrawHistoryInfos?.Count ?? 0)
		}).ToList();
		foreach (GachaPoolState item in list)
		{
			item.gachaHistoryReady = false;
			syncingGachaPoolIds.Add(item.def.FullID);
		}
		PurchaseClient purchaseClient = base.applicationState.ChillyRoomSDK.Purchase;
		return new GeneralGameAsyncOperationResultOnly<GachaHistorySyncBatch>(() => FetchHistorySyncBatchAsync(purchaseClient, targets));
	}

	private void RequestHistoryRecovery(GachaPoolState pool)
	{
		if (pool?.def == null || syncingGachaPoolIds.Contains(pool.def.FullID))
		{
			return;
		}
		GeneralGameAsyncOperationResultOnly<GachaHistorySyncBatch> operation = CreateHistorySynchronizationOperation(new GachaPoolState[1] { pool });
		operation.OnComplete(delegate
		{
			if (!ApplyHistorySyncBatch(operation.OpResult))
			{
				TraceLog.Error("抽卡历史恢复结果无效: pool=" + pool.def.FullID);
			}
		}).OnFault(delegate
		{
			pool.gachaHistoryReady = false;
			syncingGachaPoolIds.Remove(pool.def.FullID);
		}).EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_syncing_gacha_history);
	}

	public bool ApplyHistorySyncBatch(GachaHistorySyncBatch batch)
	{
		if (batch?.snapshots == null)
		{
			return false;
		}
		NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
		bool flag = true;
		foreach (GachaHistorySyncSnapshot snapshot in batch.snapshots)
		{
			GachaPoolState gachaPoolState = gachaPoolStates.FirstOrDefault((GachaPoolState state) => state?.def != null && state.def.FullID == snapshot.gachaPoolId);
			if (gachaPoolState == null)
			{
				flag = false;
				continue;
			}
			if (snapshot.replacementHistory != null)
			{
				List<GachaHistoryInfo> list = (from history in (from history in snapshot.replacementHistory
						where history != null
						select history.ToClient(context)).WhereNotNull()
					orderby history.date
					select history).ToList();
				if (list.Count != snapshot.totalCount)
				{
					TraceLog.Error($"抽卡历史转换数量不一致: pool={snapshot.gachaPoolId}, converted={list.Count}, total={snapshot.totalCount}");
					gachaPoolState.gachaHistoryReady = false;
					syncingGachaPoolIds.Remove(snapshot.gachaPoolId);
					flag = false;
					continue;
				}
				gachaPoolState.gachaDrawHistoryInfos = list;
			}
			else
			{
				GachaPoolState gachaPoolState2 = gachaPoolState;
				if (gachaPoolState2.gachaDrawHistoryInfos == null)
				{
					gachaPoolState2.gachaDrawHistoryInfos = new List<GachaHistoryInfo>();
				}
				gachaPoolState.gachaDrawHistoryInfos = gachaPoolState.gachaDrawHistoryInfos.OrderBy((GachaHistoryInfo history) => history.date).ToList();
			}
			gachaPoolState.gachaHistoryReady = (gachaPoolState.gachaDrawHistoryInfos?.Count ?? 0) == snapshot.totalCount;
			syncingGachaPoolIds.Remove(snapshot.gachaPoolId);
			flag &= gachaPoolState.gachaHistoryReady;
		}
		RebuildPityStatesFromHistory();
		base.applicationState.savingManager.SetProfileDirtyToSave();
		return flag;
	}

	public void MarkAllHistorySynchronizationFailed()
	{
		foreach (string poolId in syncingGachaPoolIds.ToList())
		{
			GachaPoolState gachaPoolState = gachaPoolStates.FirstOrDefault((GachaPoolState state) => state?.def != null && state.def.FullID == poolId);
			if (gachaPoolState != null)
			{
				gachaPoolState.gachaHistoryReady = false;
			}
		}
		syncingGachaPoolIds.Clear();
	}

	public void MarkLoadedHistoriesReady()
	{
		foreach (GachaPoolState item in gachaPoolStates.Where((GachaPoolState state) => state?.def != null))
		{
			GachaPoolState current;
			GachaPoolState gachaPoolState = (current = item);
			if (current.gachaDrawHistoryInfos == null)
			{
				current.gachaDrawHistoryInfos = new List<GachaHistoryInfo>();
			}
			gachaPoolState.gachaDrawHistoryInfos = gachaPoolState.gachaDrawHistoryInfos.OrderBy((GachaHistoryInfo history) => history.date).ToList();
			gachaPoolState.gachaHistoryReady = true;
		}
		syncingGachaPoolIds.Clear();
		RebuildPityStatesFromHistory();
	}

	public static bool CanMergeHistoryIncrement(int localCount, int deltaCount, int serverTotal)
	{
		if (localCount >= 0 && deltaCount >= 0 && serverTotal >= 0)
		{
			return (long)localCount + (long)deltaCount == serverTotal;
		}
		return false;
	}

	private async Task<GachaHistorySyncBatch> FetchHistorySyncBatchAsync(PurchaseClient purchaseClient, List<GachaHistorySyncTarget> targets)
	{
		GachaHistorySyncBatch batch = new GachaHistorySyncBatch();
		foreach (GachaHistorySyncTarget target in targets)
		{
			ClientGachaHistoryPageResult clientGachaHistoryPageResult = await purchaseClient.QueryGachaHistoryReliableAsync(new ClientGachaHistoryQueryRequest
			{
				PageIndex = 1,
				PageSize = 1,
				GachaPoolId = target.gachaPoolId
			});
			if (clientGachaHistoryPageResult.TotalCount < 0)
			{
				throw new InvalidOperationException($"服务器返回了非法抽卡历史总数: {clientGachaHistoryPageResult.TotalCount}");
			}
			GachaHistorySyncSnapshot snapshot = new GachaHistorySyncSnapshot
			{
				gachaPoolId = target.gachaPoolId,
				totalCount = clientGachaHistoryPageResult.TotalCount
			};
			if (target.localCount != clientGachaHistoryPageResult.TotalCount)
			{
				GachaHistorySyncSnapshot gachaHistorySyncSnapshot = snapshot;
				gachaHistorySyncSnapshot.replacementHistory = await FetchCompleteHistoryAsync(purchaseClient, target.gachaPoolId, clientGachaHistoryPageResult.TotalCount);
			}
			batch.snapshots.Add(snapshot);
		}
		return batch;
	}

	private async Task<List<GachaDrawHistory>> FetchCompleteHistoryAsync(PurchaseClient purchaseClient, string gachaPoolId, int expectedTotal)
	{
		return await FetchCompleteHistoryAsync(purchaseClient, gachaPoolId, null, expectedTotal);
	}

	public List<GachaPoolDef> GetHistorySharedGroup(GachaPoolDef poolDef)
	{
		if (poolDef == null)
		{
			return new List<GachaPoolDef>();
		}
		return ScriptableObjectSingleton<FortressDataManager>.instance.gachaHistorySharedGroups.OrEmptyIfNull().FirstOrDefault((FortressDataManager.GachaHistorySharedGroup sharedGroup) => sharedGroup?.groups != null && sharedGroup.groups.Contains(poolDef))?.groups.Where((GachaPoolDef pool) => pool != null).Distinct().ToList() ?? new List<GachaPoolDef>();
	}

	public async Task<GachaHistoryDisplaySnapshot> FetchHistoryDisplaySnapshotAsync(GachaPoolDef poolDef)
	{
		List<GachaPoolDef> historySharedGroup = GetHistorySharedGroup(poolDef);
		if (historySharedGroup.Count == 0)
		{
			historySharedGroup.Add(poolDef);
		}
		List<string> poolIds = historySharedGroup.Select((GachaPoolDef pool) => pool.FullID).ToList();
		PurchaseClient purchaseClient = base.applicationState.ChillyRoomSDK.Purchase;
		ClientGachaHistoryPageResult clientGachaHistoryPageResult = await purchaseClient.QueryGachaHistoryReliableAsync(new ClientGachaHistoryQueryRequest
		{
			PageIndex = 1,
			PageSize = 1,
			GachaPoolId = poolDef.FullID,
			GachaPoolIds = poolIds
		});
		if (clientGachaHistoryPageResult.TotalCount < 0)
		{
			throw new InvalidOperationException($"服务器返回了非法抽卡历史总数: {clientGachaHistoryPageResult.TotalCount}");
		}
		GachaHistoryDisplaySnapshot gachaHistoryDisplaySnapshot = new GachaHistoryDisplaySnapshot
		{
			totalCount = clientGachaHistoryPageResult.TotalCount,
			gachaPoolIds = poolIds
		};
		GachaHistoryDisplaySnapshot gachaHistoryDisplaySnapshot2 = gachaHistoryDisplaySnapshot;
		gachaHistoryDisplaySnapshot2.history = await FetchCompleteHistoryAsync(purchaseClient, poolDef.FullID, poolIds, clientGachaHistoryPageResult.TotalCount);
		return gachaHistoryDisplaySnapshot;
	}

	public bool ApplyHistoryDisplaySnapshot(GachaHistoryDisplaySnapshot snapshot)
	{
		if (snapshot?.gachaPoolIds == null || snapshot.history == null)
		{
			return false;
		}
		NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
		Dictionary<string, List<GachaDrawHistory>> dictionary = (from history in snapshot.history
			where history != null
			group history by history.GachaPoolId).ToDictionary((IGrouping<string, GachaDrawHistory> group) => group.Key, (IGrouping<string, GachaDrawHistory> group) => group.ToList());
		HashSet<string> targetPoolIds = snapshot.gachaPoolIds.ToHashSet();
		List<string> list = dictionary.Keys.Where((string item) => !targetPoolIds.Contains(item)).ToList();
		if (list.Count > 0)
		{
			TraceLog.Error("抽卡历史展示返回了未请求的卡池: " + string.Join(",", list));
			return false;
		}
		int num = 0;
		foreach (string poolId in snapshot.gachaPoolIds)
		{
			GachaPoolState gachaPoolState = gachaPoolStates.FirstOrDefault((GachaPoolState state) => state?.def != null && state.def.FullID == poolId);
			if (gachaPoolState == null)
			{
				return false;
			}
			List<GachaHistoryInfo> list2 = (dictionary.TryGetValue(poolId, out var value) ? (from history in value.Select((GachaDrawHistory history) => history.ToClient(context)).WhereNotNull()
				orderby history.date
				select history).ToList() : new List<GachaHistoryInfo>());
			if (list2.Count != (value?.Count ?? 0))
			{
				TraceLog.Error($"抽卡历史展示转换数量不一致: pool={poolId}, converted={list2.Count}, expected={value.Count}");
				gachaPoolState.gachaHistoryReady = false;
				return false;
			}
			gachaPoolState.gachaDrawHistoryInfos = list2;
			gachaPoolState.gachaHistoryReady = true;
			num += list2.Count;
		}
		if (num != snapshot.totalCount)
		{
			TraceLog.Error($"抽卡历史展示落地数量不一致: applied={num}, total={snapshot.totalCount}");
			return false;
		}
		RebuildPityStatesFromHistory();
		base.applicationState.savingManager.SetProfileDirtyToSave();
		return true;
	}

	private async Task<List<GachaDrawHistory>> FetchCompleteHistoryAsync(PurchaseClient purchaseClient, string gachaPoolId, List<string> gachaPoolIds, int expectedTotal)
	{
		if (expectedTotal == 0)
		{
			return new List<GachaDrawHistory>();
		}
		if (expectedTotal <= 5000)
		{
			ClientGachaFullHistoryResult obj = await purchaseClient.SyncGachaHistoryReliableAsync(new ClientGachaHistorySyncRequest
			{
				GachaPoolId = gachaPoolId,
				GachaPoolIds = gachaPoolIds
			});
			List<GachaDrawHistory> list = obj.Items ?? new List<GachaDrawHistory>();
			if (obj.TotalDrawCount == expectedTotal && list.Count == expectedTotal)
			{
				return list;
			}
		}
		List<GachaDrawHistory> result = new List<GachaDrawHistory>(expectedTotal);
		int pageIndex = 1;
		while (result.Count < expectedTotal)
		{
			ClientGachaHistoryPageResult clientGachaHistoryPageResult = await purchaseClient.QueryGachaHistoryReliableAsync(new ClientGachaHistoryQueryRequest
			{
				PageIndex = pageIndex,
				PageSize = 100,
				GachaPoolId = gachaPoolId,
				GachaPoolIds = gachaPoolIds
			});
			if (clientGachaHistoryPageResult.TotalCount != expectedTotal)
			{
				throw new InvalidOperationException($"分页同步期间抽卡历史总数发生变化: pool={gachaPoolId}, expected={expectedTotal}, actual={clientGachaHistoryPageResult.TotalCount}");
			}
			List<GachaDrawHistory> list2 = clientGachaHistoryPageResult.Items ?? new List<GachaDrawHistory>();
			if (list2.Count == 0)
			{
				throw new InvalidOperationException($"分页同步提前结束: pool={gachaPoolId}, received={result.Count}, total={expectedTotal}");
			}
			result.AddRange(list2);
			if (!clientGachaHistoryPageResult.HasMore && result.Count < expectedTotal)
			{
				throw new InvalidOperationException($"分页同步返回数量不足: pool={gachaPoolId}, received={result.Count}, total={expectedTotal}");
			}
			pageIndex++;
		}
		if (result.Count != expectedTotal)
		{
			throw new InvalidOperationException($"分页同步返回数量不一致: pool={gachaPoolId}, received={result.Count}, total={expectedTotal}");
		}
		return result;
	}

	public void RebuildPityStatesFromHistory()
	{
		if (gachaPityStates == null)
		{
			gachaPityStates = new List<GachaPityState>();
		}
		gachaPityStates.Clear();
		foreach (IGrouping<string, GachaPoolState> item in from state in gachaPoolStates
			where state?.def != null && !string.IsNullOrWhiteSpace(state.def.pityGroupId)
			group state by state.def.pityGroupId)
		{
			GachaPityState gachaPityState = new GachaPityState(item.Key);
			GachaPoolDef def = item.First().def;
			foreach (GachaHistoryInfo item2 in from history in item.SelectMany((GachaPoolState state) => state.gachaDrawHistoryInfos ?? new List<GachaHistoryInfo>())
				orderby history.date
				select history)
			{
				UpdatePityStateAfterDraw(def, item2, gachaPityState);
			}
			gachaPityStates.Add(gachaPityState);
		}
	}

	private GachaPityState GetOrCreatePityState(GachaPoolDef poolDef)
	{
		if (poolDef == null || string.IsNullOrWhiteSpace(poolDef.pityGroupId))
		{
			return null;
		}
		if (gachaPityStates == null)
		{
			gachaPityStates = new List<GachaPityState>();
		}
		GachaPityState gachaPityState = gachaPityStates.FirstOrDefault((GachaPityState item) => item.groupId == poolDef.pityGroupId);
		if (gachaPityState != null)
		{
			return gachaPityState;
		}
		gachaPityState = new GachaPityState(poolDef.pityGroupId);
		gachaPityStates.Add(gachaPityState);
		return gachaPityState;
	}

	private GachaHistoryInfo GenerateGachaDrawWithRecord(GachaPoolState gachaPool)
	{
		GachaHistoryInfo gachaHistoryInfo = GenerateGachaDraw(gachaPool);
		gachaPool.gachaDrawHistoryInfos.Add(gachaHistoryInfo);
		UpdatePityStateAfterDraw(gachaPool.def, gachaHistoryInfo);
		return gachaHistoryInfo;
	}

	private GachaHistoryInfo GenerateNonUpCharacterObsession(GachaPoolState gachaPool)
	{
		HashSet<PlayerCharacterDef> upCharacterSet = gachaPool.def.UPGachaWeightContents.Keys.OfType<PlayerCharacterDef>().ToHashSet();
		List<KeyValuePair<CTScriptableObject, int>> list = gachaPool.def.baseGachaWeightContents.Where((KeyValuePair<CTScriptableObject, int> kv) => kv.Key is PlayerCharacterDef item && !upCharacterSet.Contains(item)).ToList();
		if (!list.Any())
		{
			return null;
		}
		PlayerCharacterDef character = list.SelectRandomWeight((KeyValuePair<CTScriptableObject, int> kv) => kv.Value).Key as PlayerCharacterDef;
		return gachaPool.def.GenerateCharacterObsession(character);
	}

	private GachaHistoryInfo GenerateGachaDraw(GachaPoolState gachaPool)
	{
		if (gachaPool.def.enableSkillGuideDrawRule)
		{
			int count = gachaPool.gachaDrawHistoryInfos.Count;
			if (count < gachaPool.def.gachaGuideDrawList.Count)
			{
				return gachaPool.def.GenerateGuideDraw(gachaPool.def.gachaGuideDrawList[count]);
			}
		}
		if (gachaPool.def.enableGrandPrizeRule && !gachaPool.gachaDrawHistoryInfos.Any((GachaHistoryInfo h) => h.contentType == GachaPoolDef.GachaContentType.Obsession && h.content == gachaPool.def.grandPrizeCharacter) && gachaPool.gachaDrawHistoryInfos.Count >= gachaPool.def.grandPrizeCount - 1)
		{
			return gachaPool.def.GenerateGrandPrize();
		}
		Dictionary<GachaPoolDef.GachaDrawBlock, float> blockFinalWeight = ((IEnumerable<GachaPoolDef.GachaDrawBlock>)gachaPool.def.gachaDrawBlocks).ToDictionary((Func<GachaPoolDef.GachaDrawBlock, GachaPoolDef.GachaDrawBlock>)((GachaPoolDef.GachaDrawBlock b) => b), (Func<GachaPoolDef.GachaDrawBlock, float>)((GachaPoolDef.GachaDrawBlock b) => b.weight));
		if (gachaPool.def.enableSoftPityRule)
		{
			GachaPityState orCreatePityState = GetOrCreatePityState(gachaPool.def);
			int num = gachaPool.gachaDrawHistoryInfos.FindLastIndex((GachaHistoryInfo h) => h.qualityLevel == gachaPool.def.softPityQualityLevel);
			int num2 = ((orCreatePityState != null) ? (orCreatePityState.NeverHitSoftPityTarget ? orCreatePityState.totalDrawCount : (orCreatePityState.drawsAfterLastSoftPityTarget + 1)) : ((num == -1) ? gachaPool.gachaDrawHistoryInfos.Count : (gachaPool.gachaDrawHistoryInfos.Count - num)));
			if (gachaPool.def.enableIntroSoftPityRule && (orCreatePityState?.NeverHitSoftPityTarget ?? (num == -1)))
			{
				num2 += gachaPool.def.softPityMaxCount - gachaPool.def.introSoftPityCount;
			}
			if (num2 > gachaPool.def.softPityEffectCount)
			{
				float weightTransferRatio = Mathf.Clamp((float)(num2 - gachaPool.def.softPityEffectCount) / (float)(gachaPool.def.softPityMaxCount - gachaPool.def.softPityEffectCount), 0f, 1f);
				if (weightTransferRatio > 0f)
				{
					GachaPoolDef.GachaDrawBlock targetQualityBlock = blockFinalWeight.Keys.Where((GachaPoolDef.GachaDrawBlock b) => b.qualityLevel == gachaPool.def.softPityQualityLevel).ToList().SelectRandom();
					blockFinalWeight.Where((KeyValuePair<GachaPoolDef.GachaDrawBlock, float> kv) => kv.Key.qualityLevel != gachaPool.def.softPityQualityLevel).ToList().ForEach(delegate(KeyValuePair<GachaPoolDef.GachaDrawBlock, float> kv)
					{
						float num4 = kv.Value * weightTransferRatio;
						blockFinalWeight[kv.Key] -= num4;
						blockFinalWeight[targetQualityBlock] += num4;
					});
				}
			}
		}
		if (gachaPool.def.enableHardPityRule)
		{
			GachaPityState orCreatePityState2 = GetOrCreatePityState(gachaPool.def);
			bool num3;
			if (orCreatePityState2 != null)
			{
				if (orCreatePityState2.NeverHitHardPityQualified)
				{
					goto IL_039d;
				}
				num3 = orCreatePityState2.drawsAfterLastHardPityQualified >= gachaPool.def.hardPityCount - 1;
			}
			else
			{
				num3 = !gachaPool.gachaDrawHistoryInfos.TakeLast(gachaPool.def.hardPityCount - 1).Any((GachaHistoryInfo h) => h.qualityLevel >= gachaPool.def.hardPityQualityLevel);
			}
			if (num3)
			{
				goto IL_039d;
			}
		}
		goto IL_048d;
		IL_048d:
		GachaPoolDef.GachaDrawBlock selectedBlock = blockFinalWeight.Where((KeyValuePair<GachaPoolDef.GachaDrawBlock, float> kv) => kv.Value > 0f).ToList().SelectRandomWeight((KeyValuePair<GachaPoolDef.GachaDrawBlock, float> kv) => kv.Value)
			.Key;
		if (gachaPool.def.enableGachaWishRule && selectedBlock.contentType == GachaPoolDef.GachaContentType.Obsession && !gachaPool.WishUsedThisWeek && gachaPool.wishCharacter != null)
		{
			return gachaPool.def.GenerateWishObsession(gachaPool.wishCharacter);
		}
		if (gachaPool.def.enableUPRule)
		{
			List<KeyValuePair<CTScriptableObject, int>> list = gachaPool.def.UPGachaWeightContents.Where((KeyValuePair<CTScriptableObject, int> kv) => kv.Key is PlayerCharacterDef).ToList();
			List<KeyValuePair<CTScriptableObject, int>> source = gachaPool.def.UPGachaWeightContents.Where((KeyValuePair<CTScriptableObject, int> kv) => kv.Key is SkillDef).ToList();
			bool flag = UnityEngine.Random.Range(0f, 1f) < (float)gachaPool.def.UPPercent / 100f;
			if (selectedBlock.contentType == GachaPoolDef.GachaContentType.Obsession && list.Any())
			{
				if (flag)
				{
					return gachaPool.def.GenerateCharacterObsession(list.SelectRandomWeight((KeyValuePair<CTScriptableObject, int> kv) => kv.Value).Key as PlayerCharacterDef);
				}
				GachaHistoryInfo gachaHistoryInfo = GenerateNonUpCharacterObsession(gachaPool);
				if (gachaHistoryInfo != null)
				{
					return gachaHistoryInfo;
				}
			}
			if (flag && selectedBlock.contentType == GachaPoolDef.GachaContentType.Skill && source.Any())
			{
				List<KeyValuePair<CTScriptableObject, int>> list2 = source.Where((KeyValuePair<CTScriptableObject, int> s) => (s.Key as SkillDef).BaseQualityLevel == selectedBlock.qualityLevel).ToList();
				if (list2.Any())
				{
					SkillDef skill = list2.SelectRandomWeight((KeyValuePair<CTScriptableObject, int> kv) => kv.Value).Key as SkillDef;
					return gachaPool.def.GenerateSkill(skill, selectedBlock.contentType);
				}
			}
		}
		return gachaPool.def.GenerateBaseBlockDraw(base.applicationState, selectedBlock);
		IL_039d:
		List<GachaPoolDef.GachaDrawBlock> list3 = blockFinalWeight.Keys.Where((GachaPoolDef.GachaDrawBlock b) => b.qualityLevel < gachaPool.def.hardPityQualityLevel).ToList();
		float subQualityBlocksSumWeight = list3.Sum((GachaPoolDef.GachaDrawBlock b) => blockFinalWeight[b]);
		list3.ForEach(delegate(GachaPoolDef.GachaDrawBlock b)
		{
			blockFinalWeight[b] = 0f;
		});
		List<KeyValuePair<GachaPoolDef.GachaDrawBlock, float>> list4 = blockFinalWeight.Where((KeyValuePair<GachaPoolDef.GachaDrawBlock, float> s) => s.Key.qualityLevel == gachaPool.def.hardPityQualityLevel).ToList();
		float targetQualityBlocksWeightSum = list4.Sum((KeyValuePair<GachaPoolDef.GachaDrawBlock, float> kv) => kv.Value);
		if (targetQualityBlocksWeightSum > 0f)
		{
			list4.ForEach(delegate(KeyValuePair<GachaPoolDef.GachaDrawBlock, float> kv)
			{
				float num4 = subQualityBlocksSumWeight * (kv.Value / targetQualityBlocksWeightSum);
				blockFinalWeight[kv.Key] += num4;
			});
		}
		goto IL_048d;
	}

	public GlobalRewardPackage DrawXWithoutCost(GachaPoolState gachaPool, int count)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		for (int i = 0; i < count; i++)
		{
			GachaHistoryInfo gachaHistoryInfo = GenerateGachaDrawWithRecord(gachaPool);
			gachaHistoryInfo.reward.SetupGachaDisplayContent(gachaHistoryInfo.contentType, gachaHistoryInfo.content, gachaHistoryInfo.qualityLevel);
			list.Add(gachaHistoryInfo.reward);
		}
		return new GlobalRewardPackage(list);
	}

	public GlobalRewardPackage DrawX(GachaPoolState gachaPool, int count)
	{
		MatterInfo costOfDrawX = gachaPool.GetCostOfDrawX(count);
		if (costOfDrawX.count > 0)
		{
			base.applicationState.matterManager.ConsumeMatter(costOfDrawX, "探寻抽卡");
		}
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		for (int i = 0; i < count; i++)
		{
			GachaHistoryInfo gachaHistoryInfo = GenerateGachaDrawWithRecord(gachaPool);
			gachaHistoryInfo.reward.SetupGachaDisplayContent(gachaHistoryInfo.contentType, gachaHistoryInfo.content, gachaHistoryInfo.qualityLevel);
			list.Add(gachaHistoryInfo.reward);
		}
		base.applicationState.eventManager.gachaDrawEvent.TriggerEvent((gachaPool.def, count));
		ThinkingAnalyticsExtends.TrackFortressGachaDraw(base.applicationState, count, gachaPool.def, list, gachaPool.wishCharacter);
		return new GlobalRewardPackage(list);
	}

	public GlobalRewardPackage DrawX(GachaDrawResult networkResult, GachaPoolState requestedPool, int count)
	{
		List<GachaDrawHistory> list = networkResult?.NewPoolState?.DrawRewardHistory;
		bool flag = networkResult != null && networkResult.GachaTotal <= 0 && list != null;
		if (networkResult?.NewPoolState == null || requestedPool?.def == null || networkResult.NewPoolState.GachaPoolId != requestedPool.def.FullID || networkResult.GachaResults == null || networkResult.GachaResults.Count != count || (!flag && networkResult.GachaTotal < count))
		{
			TraceLog.Error($"抽卡响应校验失败: pool={requestedPool?.def?.FullID}, count={count}, " + "resultPool=" + networkResult?.NewPoolState?.GachaPoolId + ", " + $"resultCount={networkResult?.GachaResults?.Count ?? (-1)}, total={networkResult?.GachaTotal ?? (-1)}");
			if (requestedPool != null)
			{
				RequestHistoryRecovery(requestedPool);
			}
			return null;
		}
		NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
		requestedPool.wishCharacter = context.FullIdToCTAsset<PlayerCharacterDef>(networkResult.NewPoolState.WishTargetId);
		List<GachaHistoryInfo> list2 = networkResult.GachaResults.Select((GachaDrawHistory history) => history.ToClient(context)).WhereNotNull().ToList();
		if (list2.Count != count)
		{
			TraceLog.Error($"抽卡结果转换数量不一致: pool={requestedPool.def.FullID}, converted={list2.Count}, expected={count}");
			RequestHistoryRecovery(requestedPool);
			return null;
		}
		List<GachaHistoryInfo> list3 = null;
		if (flag)
		{
			list3 = (from history in (from history in list
					where history != null
					select history.ToClient(context)).WhereNotNull()
				orderby history.date
				select history).ToList();
			if (list3.Count != list.Count)
			{
				TraceLog.Error("旧版抽卡历史转换数量不一致: pool=" + requestedPool.def.FullID + ", " + $"converted={list3.Count}, expected={list.Count}");
				return null;
			}
		}
		MatterInfo costOfDrawX = requestedPool.GetCostOfDrawX(count);
		if (costOfDrawX.count > 0)
		{
			base.applicationState.matterManager.ConsumeMatter(costOfDrawX, "探寻抽卡");
		}
		GachaPoolState gachaPoolState = requestedPool;
		if (gachaPoolState.gachaDrawHistoryInfos == null)
		{
			gachaPoolState.gachaDrawHistoryInfos = new List<GachaHistoryInfo>();
		}
		if (flag)
		{
			requestedPool.gachaDrawHistoryInfos = list3;
			requestedPool.gachaHistoryReady = true;
			RebuildPityStatesFromHistory();
			base.applicationState.savingManager.SetProfileDirtyToSave();
		}
		else if (requestedPool.gachaHistoryReady && CanMergeHistoryIncrement(requestedPool.gachaDrawHistoryInfos.Count, list2.Count, networkResult.GachaTotal))
		{
			requestedPool.gachaDrawHistoryInfos.AddRange(list2);
			requestedPool.gachaDrawHistoryInfos = requestedPool.gachaDrawHistoryInfos.OrderBy((GachaHistoryInfo history) => history.date).ToList();
			foreach (GachaHistoryInfo item in list2)
			{
				UpdatePityStateAfterDraw(requestedPool.def, item);
			}
			base.applicationState.savingManager.SetProfileDirtyToSave();
		}
		else
		{
			TraceLog.Warn("抽卡历史增量校验失败，准备恢复: pool=" + requestedPool.def.FullID + ", " + $"local={requestedPool.gachaDrawHistoryInfos.Count}, delta={list2.Count}, total={networkResult.GachaTotal}");
			requestedPool.gachaHistoryReady = false;
			RequestHistoryRecovery(requestedPool);
		}
		List<GlobalRewardInfo> list4 = list2.Select((GachaHistoryInfo r) => r.reward).ToList();
		GlobalRewardPackage result = new GlobalRewardPackage(list4);
		base.applicationState.eventManager.gachaDrawEvent.TriggerEvent((requestedPool.def, count));
		ThinkingAnalyticsExtends.TrackFortressGachaDraw(base.applicationState, count, requestedPool.def, list4, requestedPool.wishCharacter);
		return result;
	}

	private void UpdatePityStateAfterDraw(GachaPoolDef poolDef, GachaHistoryInfo drawInfo, GachaPityState pityState = null)
	{
		if (pityState == null)
		{
			pityState = GetOrCreatePityState(poolDef);
		}
		if (pityState == null)
		{
			return;
		}
		pityState.totalDrawCount++;
		if (poolDef.enableSoftPityRule)
		{
			if (drawInfo.qualityLevel == poolDef.softPityQualityLevel)
			{
				pityState.drawsAfterLastSoftPityTarget = 0;
			}
			else
			{
				pityState.drawsAfterLastSoftPityTarget++;
			}
		}
		if (poolDef.enableHardPityRule)
		{
			if (drawInfo.qualityLevel >= poolDef.hardPityQualityLevel)
			{
				pityState.drawsAfterLastHardPityQualified = 0;
			}
			else
			{
				pityState.drawsAfterLastHardPityQualified++;
			}
		}
	}

	public List<GachaPoolState> SortedPool()
	{
		List<GachaPoolState> list = new List<GachaPoolState>(gachaPoolStates);
		list.RemoveAll((GachaPoolState s) => !s.valid);
		list.Sort((GachaPoolState s1, GachaPoolState s2) => s2.def.displayPriority.CompareTo(s1.def.displayPriority));
		return list;
	}

	public string GetGachaContentDescription(GachaPoolDef pool)
	{
		return "";
	}
}
