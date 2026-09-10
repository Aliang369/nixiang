using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;

public class DiscoverManager : GlobalGameManager, IPostFetchData
{
	[SaveField(null, null)]
	private new ApplicationStateInfo applicationState;

	[SaveField(null, null)]
	private List<ICTAsset> discoveredObjects;

	[SaveField(null, null)]
	private List<ICTAsset> discoveredRewardedObjects;

	[SaveField(null, null)]
	private List<ICTAsset> discoveredViewedObjects;

	[SaveField(null, null)]
	private bool discoveredViewedObjectsInitialized;

	[SaveField(null, null)]
	private Dictionary<IndexBookPage, int> pageProgressRewardedLayers = new Dictionary<IndexBookPage, int>();

	private HashSet<ICTAsset> discoveredObjects_cache;

	private HashSet<ICTAsset> discoveredRewardedObjects_cache;

	private HashSet<ICTAsset> discoveredViewedObjects_cache;

	public override void Init(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState;
		SetDiscoverObjects(discoveredObjects ?? new List<ICTAsset>(), discoveredRewardedObjects ?? new List<ICTAsset>());
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, _OnEnterBattle);
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AddItem, _OnAddItem);
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.EnchantValueChange, _OnChangeEnchant);
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AddInfluence, _OnInfluenceAdd);
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AddRune, _OnAddRune);
	}

	public override void Dispose()
	{
		applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, _OnEnterBattle);
		applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AddItem, _OnAddItem);
		applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.EnchantValueChange, _OnChangeEnchant);
		applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AddInfluence, _OnInfluenceAdd);
		applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AddRune, _OnAddRune);
	}

	public IEnumerator PostFetchData()
	{
		yield return FetchDiscoverData();
	}

	public void Discover(ICTAsset obj)
	{
		if (obj != null)
		{
			InitializeDiscoveredViewedObjects();
			if (discoveredObjects_cache.Add(obj))
			{
				discoveredObjects.Add(obj);
				applicationState.eventManager.objectDiscoveredEvent.TriggerEvent(obj);
			}
		}
	}

	public void SetDiscoverObjects(List<ICTAsset> discoveredObjects, List<ICTAsset> discoveredRewardedObjects)
	{
		this.discoveredObjects = discoveredObjects;
		discoveredObjects_cache = new HashSet<ICTAsset>(discoveredObjects);
		this.discoveredRewardedObjects = discoveredRewardedObjects;
		discoveredRewardedObjects_cache = new HashSet<ICTAsset>(discoveredRewardedObjects);
		if (discoveredViewedObjects == null)
		{
			discoveredViewedObjects = new List<ICTAsset>();
		}
		if (!discoveredViewedObjectsInitialized && discoveredObjects.Count > 0)
		{
			InitializeDiscoveredViewedObjects();
		}
		else
		{
			RefreshDiscoveredViewedObjectsCache();
		}
	}

	public bool IsDiscovered(ICTAsset obj)
	{
		return discoveredObjects_cache.Contains(obj);
	}

	public bool IsRewarded(ICTAsset obj)
	{
		return discoveredRewardedObjects_cache.Contains(obj);
	}

	public bool IsViewed(ICTAsset obj)
	{
		if (obj != null)
		{
			return discoveredViewedObjects_cache.Contains(obj);
		}
		return false;
	}

	public bool IsNewDiscoveredUnviewed(ICTAsset obj)
	{
		if (obj != null && IsDiscovered(obj))
		{
			return !IsViewed(obj);
		}
		return false;
	}

	public void MarkDiscoveredContentViewed(ICTAsset obj)
	{
		if (obj != null)
		{
			InitializeDiscoveredViewedObjects();
			if (IsDiscovered(obj) && discoveredViewedObjects_cache.Add(obj))
			{
				discoveredViewedObjects.Add(obj);
			}
		}
	}

	public static IndexBookNewDiscoverTagMode GetNewDiscoverTagMode(IndexBookPage page)
	{
		if ((uint)(page - 2) <= 2u)
		{
			return IndexBookNewDiscoverTagMode.Reward;
		}
		return IndexBookNewDiscoverTagMode.View;
	}

	private void InitializeDiscoveredViewedObjects()
	{
		if (discoveredViewedObjects == null)
		{
			discoveredViewedObjects = new List<ICTAsset>();
		}
		if (discoveredViewedObjectsInitialized)
		{
			RefreshDiscoveredViewedObjectsCache();
			return;
		}
		discoveredViewedObjects.Clear();
		if (discoveredObjects != null)
		{
			discoveredViewedObjects.AddRange(discoveredObjects.Where((ICTAsset obj) => obj != null));
		}
		discoveredViewedObjectsInitialized = true;
		RefreshDiscoveredViewedObjectsCache();
	}

	private void RefreshDiscoveredViewedObjectsCache()
	{
		if (discoveredViewedObjects == null)
		{
			discoveredViewedObjects = new List<ICTAsset>();
		}
		if (discoveredObjects_cache == null)
		{
			discoveredObjects_cache = new HashSet<ICTAsset>(discoveredObjects ?? new List<ICTAsset>());
		}
		discoveredViewedObjects.RemoveAll((ICTAsset obj) => obj == null || !discoveredObjects_cache.Contains(obj));
		discoveredViewedObjects_cache = new HashSet<ICTAsset>(discoveredViewedObjects);
	}

	public void DiscoverAll()
	{
		foreach (ItemDefinition validAsset in applicationState.assetCatalog.itemDefCatalog.ValidAssets)
		{
			Discover(validAsset);
		}
		foreach (EnchantDef validAsset2 in applicationState.assetCatalog.enchantDefCatalog.ValidAssets)
		{
			Discover(validAsset2);
		}
		foreach (RuneDef validAsset3 in applicationState.assetCatalog.runeDefCatalog.ValidAssets)
		{
			Discover(validAsset3);
		}
		foreach (EnemyInfoData asset in applicationState.assetCatalog.enemyInfoDataCatalog.assets)
		{
			Discover(asset);
		}
		foreach (InfluenceDef validAsset4 in applicationState.assetCatalog.influenceCatalog.ValidAssets)
		{
			Discover(validAsset4);
		}
		foreach (SkillQualityIndexEntry skillQualityIndexEntry in applicationState.assetCatalog.GetSkillQualityIndexEntries())
		{
			Discover(skillQualityIndexEntry);
		}
	}

	public void MarkContentAsRewarded(ICTAsset content)
	{
		if (content != null && !discoveredRewardedObjects_cache.Contains(content))
		{
			discoveredRewardedObjects.Add(content);
			discoveredRewardedObjects_cache.Add(content);
		}
	}

	public void IncrementPageProgressRewardedLayer(IndexBookPage page)
	{
		if (!pageProgressRewardedLayers.TryGetValue(page, out var value))
		{
			value = 0;
		}
		pageProgressRewardedLayers[page] = value + 1;
	}

	public int GetPageProgressRewardedLayerCount(IndexBookPage page)
	{
		if (!pageProgressRewardedLayers.TryGetValue(page, out var value))
		{
			return 0;
		}
		return value;
	}

	public int GetPageReadyProgressRewardCount(IndexBookPage page)
	{
		IndexBookConfig indexBookConfig = ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs.FirstOrDefault((IndexBookConfig c) => c.page == page);
		if (indexBookConfig == null || indexBookConfig.indexProgressLayer.IsNullOrEmpty())
		{
			return 0;
		}
		int pageProgressRewardedLayerCount = GetPageProgressRewardedLayerCount(page);
		int indexPageDiscoveredCount = GetIndexPageDiscoveredCount(page);
		int num = 0;
		for (int num2 = pageProgressRewardedLayerCount; num2 < indexBookConfig.indexProgressLayer.Count && indexPageDiscoveredCount >= indexBookConfig.indexProgressLayer[num2].needDiscoverCount; num2++)
		{
			num++;
		}
		return num;
	}

	public int GetPageReadySingleRewardCount(IndexBookPage page)
	{
		if (ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs.FirstOrDefault((IndexBookConfig c) => c.page == page)?.singleDiscoverReward == null)
		{
			return 0;
		}
		return GetAllIndexPageSingleRewardContent(page, applicationState.assetCatalog).Count((ICTAsset content) => discoveredObjects_cache.Contains(content) && !discoveredRewardedObjects_cache.Contains(content));
	}

	public GlobalRewardPackage ConfirmGetIndexPageAllSingleDiscoverReward(IndexBookPage page)
	{
		IndexBookConfig indexBookConfig = ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs.FirstOrDefault((IndexBookConfig c) => c.page == page);
		if (indexBookConfig?.singleDiscoverReward == null)
		{
			return null;
		}
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		foreach (ICTAsset item2 in GetAllIndexPageSingleRewardContent(page, applicationState.assetCatalog))
		{
			if (discoveredObjects_cache.Contains(item2) && !discoveredRewardedObjects_cache.Contains(item2))
			{
				GlobalRewardInfo item = indexBookConfig.singleDiscoverReward.GenerateReward(applicationState, GlobalRewardSourceType.Other, item2);
				list.Add(item);
				MarkContentAsRewarded(item2);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(list);
		globalRewardPackage.GiveAll(applicationState);
		ThinkingAnalyticsExtends.TrackGetIndexbookReward(exploreProgress: (float)GetIndexPageDiscoveredCount(page) / (float)GetIndexPageMaxCount(page, applicationState.assetCatalog), applicationStateInfo: applicationState, rewardType: "单项发现奖励", pageType: page.ToString(), rewardCount: list.Count, rewardLevel: 0);
		return globalRewardPackage;
	}

	public GlobalRewardPackage ConfirmGetIndexPageNextProgressLayerReward(IndexBookPage page)
	{
		IndexBookConfig indexBookConfig = ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs.FirstOrDefault((IndexBookConfig c) => c.page == page);
		if (indexBookConfig == null || indexBookConfig.indexProgressLayer.IsNullOrEmpty())
		{
			return null;
		}
		if (!pageProgressRewardedLayers.TryGetValue(page, out var value))
		{
			value = 0;
		}
		if (value >= indexBookConfig.indexProgressLayer.Count)
		{
			return null;
		}
		IndexBookConfig.IndexBookProgressLayer indexBookProgressLayer = indexBookConfig.indexProgressLayer[value];
		int indexPageDiscoveredCount = GetIndexPageDiscoveredCount(page);
		if (indexPageDiscoveredCount < indexBookProgressLayer.needDiscoverCount)
		{
			return null;
		}
		GlobalRewardInfo globalRewardInfo = indexBookProgressLayer.reward?.GenerateReward(applicationState, GlobalRewardSourceType.Other, page.ToString());
		if (globalRewardInfo == null)
		{
			return null;
		}
		pageProgressRewardedLayers[page] = value + 1;
		GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(globalRewardInfo);
		globalRewardPackage.GiveAll(applicationState);
		ThinkingAnalyticsExtends.TrackGetIndexbookReward(exploreProgress: (float)indexPageDiscoveredCount / (float)GetIndexPageMaxCount(page, applicationState.assetCatalog), applicationStateInfo: applicationState, rewardType: "进度层奖励", pageType: page.ToString(), rewardCount: 1, rewardLevel: value + 1);
		return globalRewardPackage;
	}

	private void _OnEnterBattle(GameEvent.Type type, GameEvent data)
	{
		if (!IsCharacterTrialGameEvent(data))
		{
			CharacterAI enemy = data.gameState.battleState.enemy;
			if (enemy != null)
			{
				EnemyInfoData info = applicationState.assetCatalog.enemyInfoDataCatalog.GetInfo(enemy.component);
				Discover(info);
			}
		}
	}

	private void _OnAddItem(GameEvent.Type type, GameEvent data)
	{
		if (!IsCharacterTrialGameEvent(data) && data.TargetItem != null)
		{
			Discover(data.TargetItem.GetDef());
		}
	}

	private void _OnChangeEnchant(GameEvent.Type type, GameEvent data)
	{
		if (!IsCharacterTrialGameEvent(data) && data.TargetEnchant != null)
		{
			Discover(data.TargetEnchant.def);
		}
	}

	private void _OnInfluenceAdd(GameEvent.Type type, GameEvent data)
	{
		if (!IsCharacterTrialGameEvent(data) && data.TargetInfluence != null)
		{
			Discover(data.TargetInfluence.def);
		}
	}

	private void _OnAddRune(GameEvent.Type type, GameEvent data)
	{
		if (!IsCharacterTrialGameEvent(data) && data.TargetRune != null)
		{
			Discover(data.TargetRune.def);
		}
	}

	private static bool IsCharacterTrialGameEvent(GameEvent data)
	{
		if (data == null)
		{
			return false;
		}
		return data.gameState?.IsCharacterTrialGame == true;
	}

	public static int GetIndexPageMaxCount(IndexBookPage page, AssetCatalog catalog)
	{
		return GetAllIndexPageContent(page, catalog)?.Count() ?? 1;
	}

	public int GetIndexPageDiscoveredCount(IndexBookPage page)
	{
		return GetAllIndexPageContent(page, applicationState.assetCatalog)?.Count(IsDiscovered) ?? 1;
	}

	public int GetIndexPageRewardedCount(IndexBookPage page)
	{
		return GetAllIndexPageContent(page, applicationState.assetCatalog)?.Count((ICTAsset a) => discoveredRewardedObjects_cache.Contains(a)) ?? 1;
	}

	public float GetPageCollectRatio(IndexBookPage page)
	{
		int indexPageMaxCount = GetIndexPageMaxCount(page, applicationState.assetCatalog);
		if (indexPageMaxCount <= 0)
		{
			return 1f;
		}
		return (float)GetIndexPageDiscoveredCount(page) / (float)indexPageMaxCount;
	}

	public float GetAllPageCollectAvgRatio()
	{
		List<IndexBookPage> list = (from c in ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs
			select c.page into p
			where GetIndexPageMaxCount(p, applicationState.assetCatalog) > 1
			select p).ToList();
		return ((IEnumerable<IndexBookPage>)list).Sum((Func<IndexBookPage, float>)GetPageCollectRatio) / (float)list.Count;
	}

	public bool CheckIndexBookLayerRewardGetAble(IndexBookConfig.IndexBookProgressLayer layer, IndexBookPage page)
	{
		return GetIndexPageDiscoveredCount(page) >= layer.needDiscoverCount;
	}

	public IndexBookConfig.IndexBookProgressLayer GetPageNextToRewardProgressLayerConfig(IndexBookPage page)
	{
		IndexBookConfig indexBookConfig = ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs.FirstOrDefault((IndexBookConfig c) => c.page == page);
		if (indexBookConfig == null || indexBookConfig.indexProgressLayer.IsNullOrEmpty())
		{
			return null;
		}
		if (!pageProgressRewardedLayers.TryGetValue(page, out var value))
		{
			value = 0;
		}
		if (value >= indexBookConfig.indexProgressLayer.Count)
		{
			return null;
		}
		return indexBookConfig.indexProgressLayer[value];
	}

	public bool CheckPageWithAnyRewardToGet(IndexBookPage page)
	{
		if (ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs.FirstOrDefault((IndexBookConfig c) => c.page == page)?.singleDiscoverReward != null)
		{
			foreach (ICTAsset item in GetAllIndexPageSingleRewardContent(page, applicationState.assetCatalog))
			{
				if (IsDiscovered(item) && !IsRewarded(item))
				{
					return true;
				}
			}
		}
		IndexBookConfig.IndexBookProgressLayer pageNextToRewardProgressLayerConfig = GetPageNextToRewardProgressLayerConfig(page);
		if (pageNextToRewardProgressLayerConfig != null && CheckIndexBookLayerRewardGetAble(pageNextToRewardProgressLayerConfig, page))
		{
			return true;
		}
		return false;
	}

	public bool CheckWithAnyPageWithUnGotReward()
	{
		foreach (IndexBookConfig indexBookConfig in ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs)
		{
			if (CheckPageWithAnyRewardToGet(indexBookConfig.page))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsContentInConfiguredIndexPage(ICTAsset content, AssetCatalog catalog)
	{
		if (content == null || catalog == null || ScriptableObjectSingleton<FortressDataManager>.instance?.indexBookConfigs == null)
		{
			return false;
		}
		foreach (IndexBookConfig indexBookConfig in ScriptableObjectSingleton<FortressDataManager>.instance.indexBookConfigs)
		{
			IEnumerable<ICTAsset> allIndexPageContent = GetAllIndexPageContent(indexBookConfig.page, catalog);
			if (allIndexPageContent != null && allIndexPageContent.Contains(content))
			{
				return true;
			}
		}
		return false;
	}

	public List<(CharacterClassType, IndexBookPage)> GetAllClassItemIndexPagePairs()
	{
		return new List<(CharacterClassType, IndexBookPage)>
		{
			(CharacterClassType.Warrior, IndexBookPage.ClassItem_Warrior),
			(CharacterClassType.Wizard, IndexBookPage.ClassItem_Wizard),
			(CharacterClassType.Dancer, IndexBookPage.ClassItem_Dancer),
			(CharacterClassType.Musician, IndexBookPage.ClassItem_Musician),
			(CharacterClassType.XiaKe, IndexBookPage.ClassItem_Xiake),
			(CharacterClassType.Inquisitor, IndexBookPage.ClassItem_Inquisitor),
			(CharacterClassType.MaterialArtist, IndexBookPage.ClassItem_MaterialArtist)
		};
	}

	public static IndexBookPage GetIndexPageByCharacterClass(CharacterClassType classType)
	{
		return classType switch
		{
			CharacterClassType.Warrior => IndexBookPage.ClassItem_Warrior, 
			CharacterClassType.Wizard => IndexBookPage.ClassItem_Wizard, 
			CharacterClassType.Dancer => IndexBookPage.ClassItem_Dancer, 
			CharacterClassType.Musician => IndexBookPage.ClassItem_Musician, 
			CharacterClassType.XiaKe => IndexBookPage.ClassItem_Xiake, 
			CharacterClassType.Inquisitor => IndexBookPage.ClassItem_Inquisitor, 
			CharacterClassType.MaterialArtist => IndexBookPage.ClassItem_MaterialArtist, 
			_ => IndexBookPage.None, 
		};
	}

	public List<(CharacterClassType, IndexBookPage)> GetAllClassSkillIndexPagePairs()
	{
		return new List<(CharacterClassType, IndexBookPage)>
		{
			(CharacterClassType.Warrior, IndexBookPage.ClassSkill_Warrior),
			(CharacterClassType.Wizard, IndexBookPage.ClassSkill_Wizard),
			(CharacterClassType.Dancer, IndexBookPage.ClassSkill_Dancer),
			(CharacterClassType.Musician, IndexBookPage.ClassSkill_Musician),
			(CharacterClassType.XiaKe, IndexBookPage.ClassSkill_Xiake),
			(CharacterClassType.Inquisitor, IndexBookPage.ClassSkill_Inquisitor),
			(CharacterClassType.MaterialArtist, IndexBookPage.ClassSkill_MaterialArtist)
		};
	}

	public static IndexBookPage GetSkillIndexPageByCharacterClass(CharacterClassType classType)
	{
		return classType switch
		{
			CharacterClassType.Warrior => IndexBookPage.ClassSkill_Warrior, 
			CharacterClassType.Wizard => IndexBookPage.ClassSkill_Wizard, 
			CharacterClassType.Dancer => IndexBookPage.ClassSkill_Dancer, 
			CharacterClassType.Musician => IndexBookPage.ClassSkill_Musician, 
			CharacterClassType.XiaKe => IndexBookPage.ClassSkill_Xiake, 
			CharacterClassType.Inquisitor => IndexBookPage.ClassSkill_Inquisitor, 
			CharacterClassType.MaterialArtist => IndexBookPage.ClassSkill_MaterialArtist, 
			_ => IndexBookPage.None, 
		};
	}

	public static IEnumerable<T> GetAllIndexPageContent<T>(IndexBookPage page, AssetCatalog catalog)
	{
		return (GetAllIndexPageContent(page, catalog) ?? Enumerable.Empty<ICTAsset>()).OfType<T>();
	}

	public static IEnumerable<ICTAsset> GetAllIndexPageSingleRewardContent(IndexBookPage page, AssetCatalog catalog)
	{
		IEnumerable<ICTAsset> enumerable = GetAllIndexPageContent(page, catalog) ?? Enumerable.Empty<ICTAsset>();
		if (page != IndexBookPage.Influence)
		{
			return enumerable;
		}
		IEnumerable<InfluenceDef> second = catalog.influenceCatalog.ValidAssets.Where((InfluenceDef influence) => !influence.developingAssetFlag && influence.hideInIndex && influence.retainSingleDiscoverRewardWhenHidden);
		return enumerable.Concat(second).Distinct();
	}

	public static IEnumerable<ICTAsset> GetAllIndexPageContent(IndexBookPage page, AssetCatalog catalog)
	{
		return page switch
		{
			IndexBookPage.Enemy => catalog.enemyInfoDataCatalog.ValidAssets.Where((EnemyInfoData a) => !a.hideInIndex), 
			IndexBookPage.Rune => catalog.runeDefCatalog.ValidAssets.Where((RuneDef d) => !d.developingAssetFlag && !d.hideInIndex), 
			IndexBookPage.Enchant => catalog.enchantDefCatalog.ValidAssets.Where((EnchantDef d) => !d.developingAssetFlag && !d.hideInIndex), 
			IndexBookPage.Influence => catalog.influenceCatalog.ValidAssets.Where((InfluenceDef d) => !d.developingAssetFlag && !d.hideInIndex), 
			IndexBookPage.SkillGem => catalog.skillGemCatalog.ValidAssets.Where((SkillGemDef d) => !d.developingAssetFlag), 
			IndexBookPage.ClassItem_Warrior => from d in catalog.GetCharacterItems(CharacterClassType.Warrior)
				where !d.hideInIndex
				select d, 
			IndexBookPage.ClassItem_Wizard => from d in catalog.GetCharacterItems(CharacterClassType.Wizard)
				where !d.hideInIndex
				select d, 
			IndexBookPage.ClassItem_Dancer => from d in catalog.GetCharacterItems(CharacterClassType.Dancer)
				where !d.hideInIndex
				select d, 
			IndexBookPage.ClassItem_Musician => from d in catalog.GetCharacterItems(CharacterClassType.Musician)
				where !d.hideInIndex
				select d, 
			IndexBookPage.ClassItem_Xiake => from d in catalog.GetCharacterItems(CharacterClassType.XiaKe)
				where !d.hideInIndex
				select d, 
			IndexBookPage.ClassItem_Inquisitor => from d in catalog.GetCharacterItems(CharacterClassType.Inquisitor)
				where !d.hideInIndex
				select d, 
			IndexBookPage.ClassItem_MaterialArtist => from d in catalog.GetCharacterItems(CharacterClassType.MaterialArtist)
				where !d.hideInIndex
				select d, 
			IndexBookPage.Item_EnemyLoot => (from d in catalog.GetGlobalLootItems().Concat(catalog.itemDefCatalog.ValidAssets.Where((ItemDefinition d) => d.includeInExtraIndex))
				where !d.developingAssetFlag && !d.hideInIndex
				select d).Distinct(), 
			IndexBookPage.Item_Other => null, 
			IndexBookPage.ClassSkill_Warrior => catalog.GetSkillQualityIndexEntries().Where(delegate(SkillQualityIndexEntry entry)
			{
				CharacterClassDef attachCharacterClass = entry.SkillDef.attachCharacterClass;
				return (object)attachCharacterClass != null && attachCharacterClass.mClass == CharacterClassType.Warrior;
			}), 
			IndexBookPage.ClassSkill_Wizard => catalog.GetSkillQualityIndexEntries().Where(delegate(SkillQualityIndexEntry entry)
			{
				CharacterClassDef attachCharacterClass = entry.SkillDef.attachCharacterClass;
				return (object)attachCharacterClass != null && attachCharacterClass.mClass == CharacterClassType.Wizard;
			}), 
			IndexBookPage.ClassSkill_Dancer => catalog.GetSkillQualityIndexEntries().Where(delegate(SkillQualityIndexEntry entry)
			{
				CharacterClassDef attachCharacterClass = entry.SkillDef.attachCharacterClass;
				return (object)attachCharacterClass != null && attachCharacterClass.mClass == CharacterClassType.Dancer;
			}), 
			IndexBookPage.ClassSkill_Musician => catalog.GetSkillQualityIndexEntries().Where(delegate(SkillQualityIndexEntry entry)
			{
				CharacterClassDef attachCharacterClass = entry.SkillDef.attachCharacterClass;
				return (object)attachCharacterClass != null && attachCharacterClass.mClass == CharacterClassType.Musician;
			}), 
			IndexBookPage.ClassSkill_Xiake => catalog.GetSkillQualityIndexEntries().Where(delegate(SkillQualityIndexEntry entry)
			{
				CharacterClassDef attachCharacterClass = entry.SkillDef.attachCharacterClass;
				return (object)attachCharacterClass != null && attachCharacterClass.mClass == CharacterClassType.XiaKe;
			}), 
			IndexBookPage.ClassSkill_Inquisitor => catalog.GetSkillQualityIndexEntries().Where(delegate(SkillQualityIndexEntry entry)
			{
				CharacterClassDef attachCharacterClass = entry.SkillDef.attachCharacterClass;
				return (object)attachCharacterClass != null && attachCharacterClass.mClass == CharacterClassType.Inquisitor;
			}), 
			IndexBookPage.ClassSkill_MaterialArtist => catalog.GetSkillQualityIndexEntries().Where(delegate(SkillQualityIndexEntry entry)
			{
				CharacterClassDef attachCharacterClass = entry.SkillDef.attachCharacterClass;
				return (object)attachCharacterClass != null && attachCharacterClass.mClass == CharacterClassType.MaterialArtist;
			}), 
			IndexBookPage.Skill_Common => from entry in catalog.GetSkillQualityIndexEntries()
				where entry.SkillDef.attachCharacterClass == null
				select entry, 
			IndexBookPage.SkinSet => null, 
			IndexBookPage.None => null, 
			_ => null, 
		};
	}

	public IEnumerator FetchDiscoverData()
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			GeneralGameAsyncOperationResultOnly<PlayerIndexBookData> op = new GeneralGameAsyncOperationResultOnly<PlayerIndexBookData>(applicationState.ChillyRoomSDK.IndexBook.FetchIndexBookDataAsync);
			op.OnComplete(delegate
			{
				applicationState.discoverManager.ApplyIndexDiscover(op.OpResult);
				base.fortressState?.uiManager?.SetDirty();
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_fetching_index_reward);
			yield return op.WaitForEnd();
		}
	}

	public void ApplyIndexDiscover(PlayerIndexBookData networkResult)
	{
		NetworkConvertContext networkConvertContext = new NetworkConvertContext(applicationState);
		List<ICTAsset> list = networkResult.DiscoveredContent?.Select(networkConvertContext.FullIdToCTAsset<ICTAsset>).Where((ICTAsset a) => a != null).ToList() ?? new List<ICTAsset>();
		List<ICTAsset> list2 = networkResult.DiscoveredRewardedContent?.Select(networkConvertContext.FullIdToCTAsset<ICTAsset>).Where((ICTAsset a) => a != null).ToList() ?? new List<ICTAsset>();
		SetDiscoverObjects(list, list2);
		if (networkResult.PageProgressRewardedLayers == null)
		{
			return;
		}
		pageProgressRewardedLayers.Clear();
		foreach (KeyValuePair<ChillyRoom.Services.ContainerOfSoul.IndexBookPage, int> pageProgressRewardedLayer in networkResult.PageProgressRewardedLayers)
		{
			pageProgressRewardedLayers[(IndexBookPage)pageProgressRewardedLayer.Key] = pageProgressRewardedLayer.Value;
		}
	}
}
