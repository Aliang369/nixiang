using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using GameRandom;

public class MatterManager : GlobalGameManager
{
	[SaveField(null, null)]
	public bool initMatterAdded;

	[SaveField(null, null)]
	public List<MatterInfo> matterInfos;

	public MatterInfo CommonCurrencyMatter => matterInfos.Find((MatterInfo t) => t.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.commonCurrencyMatter);

	public MatterInfo EssenceMatter => matterInfos.Find((MatterInfo t) => t.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.essenceCurrencyMatter);

	public MatterInfo CreditMatter => matterInfos.Find((MatterInfo t) => t.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.creditCurrencyMatter);

	public MatterInfo GachaTicketMatter => matterInfos.Find((MatterInfo t) => t.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.gachaTicketMatter);

	public MatterInfo StoreTokenMatter => matterInfos.Find((MatterInfo t) => t.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.storeTokenMatter);

	public MatterInfo DailyTokenMatter => matterInfos.Find((MatterInfo t) => t.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.weeklyTokenMatter);

	public MatterInfo SkillWordRecycleMatter => matterInfos.Find((MatterInfo t) => t.matterDef == ScriptableObjectSingleton<FortressDataManager>.instance.skillWordRecycleMatter);

	public override void Init(ApplicationStateInfo applicationState)
	{
		applicationState.assetCatalog.matterDefCatalog.ValidAssets.BuildToList(ref matterInfos, (MatterDef def, MatterInfo info) => info.matterDef == def, (MatterDef def) => new MatterInfo(def, 0));
	}

	public void PostInit()
	{
		if (initMatterAdded)
		{
			return;
		}
		foreach (MatterReward initMatter in ScriptableObjectSingleton<FortressDataManager>.instance.initMatters)
		{
			initMatter.GenerateReward(base.applicationState, GlobalRewardSourceType.Other, "").Give(base.applicationState);
		}
		initMatterAdded = true;
	}

	public int GetMatterCount(MatterDef def)
	{
		return matterInfos.Find((MatterInfo i) => i.matterDef == def)?.count ?? 0;
	}

	public MatterInfo GetMatterInfo(MatterDef def)
	{
		return matterInfos.Find((MatterInfo i) => i.matterDef == def) ?? new MatterInfo(def, 0);
	}

	public MatterDef GenerateRandomLootMatter()
	{
		return base.applicationState.assetCatalog.matterDefCatalog.ValidAssets.FindAll((MatterDef m) => m.IsMatterTypeEnemyLoot).SelectRandom();
	}

	public bool HasEnoughMatter(ICollection<MatterInfo> matterInfos)
	{
		return MatterInfo.FoldMatterInfo(matterInfos).All((MatterInfo m) => HasEnoughMatter(m.matterDef, m.count));
	}

	public bool HasEnoughMatter(MatterInfo matterInfo)
	{
		return HasEnoughMatter(matterInfo.matterDef, matterInfo.count);
	}

	public bool HasEnoughMatter(MatterDef def, int count)
	{
		MatterInfo matterInfo = matterInfos.Find((MatterInfo i) => i.matterDef == def);
		if (matterInfo == null)
		{
			return false;
		}
		return matterInfo.count >= count;
	}

	public void AddMatter(MatterInfo matterInfo, string TAcause)
	{
		AddMatter(matterInfo.matterDef, matterInfo.count, TAcause);
	}

	public void AddMatter(MatterDef def, int count, string TAcause)
	{
		if (def.matterType == MatterDef.MatterType.Loot)
		{
			base.applicationState.playerInfoManager.newLoots.AddUnique(def);
		}
		MatterInfo matterInfo = matterInfos.Find((MatterInfo i) => i.matterDef == def);
		if (matterInfo == null)
		{
			matterInfo = new MatterInfo(def, count);
			matterInfos.Add(matterInfo);
		}
		else
		{
			matterInfo.count += count;
		}
		base.applicationState.eventManager.matterChangeEvent.TriggerEvent(new MatterInfo(def, count));
		base.applicationState.globalUnlockManager.RecordGotObject(def);
		ThinkingAnalyticsExtends.TrackMatterChange(base.applicationState, def, count, matterInfo.count, TAcause);
		base.applicationState.savingManager.SetProfileDirtyToSave();
	}

	public bool ConsumeMatter(ICollection<MatterInfo> matterInfos, string TAcause)
	{
		if (matterInfos.Any((MatterInfo m) => !m.IsPayable(base.applicationState)))
		{
			return false;
		}
		foreach (MatterInfo matterInfo in matterInfos)
		{
			ConsumeMatter(matterInfo, TAcause);
		}
		return true;
	}

	public bool ConsumeMatter(MatterInfo matterInfo, string TAcause)
	{
		return ConsumeMatter(matterInfo.matterDef, matterInfo.count, TAcause);
	}

	public bool ConsumeMatter(MatterDef def, int count, string TAcause)
	{
		MatterInfo matterInfo = matterInfos.Find((MatterInfo i) => i.matterDef == def);
		if (matterInfo == null || matterInfo.count < count)
		{
			return false;
		}
		matterInfo.count -= count;
		base.applicationState.eventManager.matterChangeEvent.TriggerEvent(new MatterInfo(def, -count));
		ThinkingAnalyticsExtends.TrackMatterChange(base.applicationState, def, -count, matterInfo.count, TAcause);
		base.applicationState.savingManager.SetProfileDirtyToSave();
		return true;
	}

	public void OpenLoot(MatterDef def, int count)
	{
		if (def.matterType != MatterDef.MatterType.Loot)
		{
			return;
		}
		LootSetup lootSetup = def.lootSetup;
		FixedLootSetup lootSetup2 = lootSetup as FixedLootSetup;
		if (lootSetup2 == null || !GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
		GeneralGameAsyncOperation<LootRequest, LootReply> lootOp = new GeneralGameAsyncOperation<LootRequest, LootReply>(new LootRequest(def.FullID, count), base.applicationState.ChillyRoomSDK.Loot.LootAsync);
		lootOp.OnComplete(delegate
		{
			OnComplete(lootOp.OpResult.RewardInfos.Select((ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo r) => r.ToClient(context)).ToList());
		}).ManualRetry().EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_opening_loot);
		void OnComplete(List<GlobalRewardInfo> rewards)
		{
			TraceLog.TryDebug(() => "[ChillyRoomServiceExtends] OpenLoot.OnComplete() - rewards: \n" + TraceLog.ToJsonString(rewards));
			base.applicationState.matterManager.ConsumeMatter(def, count, "开启宝箱");
			foreach (GlobalRewardInfo reward in rewards)
			{
				reward.Give(base.applicationState);
			}
			ThinkingAnalyticsExtends.TrackFortressOpenLootMatter(base.applicationState, def, -1);
			GlobalRewardPackage pack = new GlobalRewardPackage(rewards)
			{
				title = "开启宝箱"
			};
			if (base.applicationState.fortressState != null)
			{
				base.applicationState.fortressState.StartCoroutine(() => base.applicationState.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(pack));
				base.applicationState.fortressState.uiManager.SetDirty();
			}
		}
	}

	public void OpenPickLoot(MatterDef def, int count, int index)
	{
		if (def.matterType != MatterDef.MatterType.Loot)
		{
			return;
		}
		LootSetup lootSetup = def.lootSetup;
		PickLootSetup lootSetup2 = lootSetup as PickLootSetup;
		if (lootSetup2 == null || lootSetup2.globalRewards.IsNullOrEmpty() || index >= lootSetup2.globalRewards.Count || !GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
		GeneralGameAsyncOperation<LootRequest, LootReply> lootOp = new GeneralGameAsyncOperation<LootRequest, LootReply>(new LootRequest(def.FullID, count, index), base.applicationState.ChillyRoomSDK.Loot.LootAsync);
		lootOp.OnComplete(delegate
		{
			OnComplete(lootOp.OpResult.RewardInfos.Select((ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo r) => r.ToClient(context)).ToList());
		}).ManualRetry().EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_opening_loot);
		void OnComplete(List<GlobalRewardInfo> rewards)
		{
			if (lootSetup2.mergeSameItemsOnOpen)
			{
				rewards = GlobalRewardInfo.CompressRewardsForDisplay(rewards);
			}
			TraceLog.TryDebug(() => "[ChillyRoomServiceExtends] OpenPickedLoot.OnComplete() - rewards: \n" + TraceLog.ToJsonString(rewards));
			foreach (GlobalRewardInfo reward in rewards)
			{
				reward.Give(base.applicationState);
			}
			base.applicationState.matterManager.ConsumeMatter(def, count, "开启宝箱");
			ThinkingAnalyticsExtends.TrackFortressOpenLootMatter(base.applicationState, def, index);
			GlobalRewardPackage pack = new GlobalRewardPackage(rewards)
			{
				title = "开启宝箱"
			};
			if (base.applicationState.fortressState != null)
			{
				base.applicationState.fortressState.StartCoroutine(() => base.applicationState.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(pack));
				base.applicationState.fortressState.uiManager.SetDirty();
			}
		}
	}

	public IEnumerator SyncMatter()
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			yield break;
		}
		GeneralGameAsyncOperationResultOnly<SyncMatterResult> op = new GeneralGameAsyncOperationResultOnly<SyncMatterResult>(base.applicationState.ChillyRoomSDK.ProfileData.SyncMatterAsync);
		op.ManualRetry().EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_network_communicating);
		yield return op.WaitForEnd();
		if (!op.isComplete)
		{
			TraceLog.Error("[MatterManager] SyncMatter Failed: " + op.errorCode);
			yield break;
		}
		try
		{
			NetworkConvertContext context = new NetworkConvertContext(base.applicationState);
			Dictionary<MatterDef, int> dictionary = MatterInfo.FoldMatterInfo((from m in op.OpResult.MatterInfos
				select m.ToClient(context) into m
				where m?.IsValidate() ?? false
				select m).ToList()).ToDictionary((MatterInfo info) => info.matterDef, (MatterInfo info) => info.count);
			foreach (MatterInfo matterInfo in matterInfos)
			{
				int count = matterInfo.count;
				matterInfo.count = dictionary.GetValueOrDefault(matterInfo.matterDef, 0);
				if (count != matterInfo.count)
				{
					base.applicationState.eventManager.matterChangeEvent.TriggerEvent(new MatterInfo(matterInfo.matterDef, matterInfo.count - count));
				}
			}
			base.applicationState.savingManager.SetProfileDirtyToSave();
		}
		catch (Exception arg)
		{
			TraceLog.Error($"[MatterManager] SyncMatter Failed: e {arg}");
		}
	}
}
