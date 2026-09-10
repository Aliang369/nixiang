using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class NewGameConfigData
{
	public class GeneratePrepareConfig
	{
		[SaveField(null, null)]
		public string gameGuid;

		[SaveField(null, null)]
		public uint? gameRandomSeed;

		[SaveField(null, null)]
		public List<ForerunnerLegacy> forerunnerLegacyThisGame;

		[SaveField(null, null)]
		public List<PVPDefender> pvpDefenderThisGame;

		[SaveField(null, null)]
		public BossBlessWeightSnapshot bossBlessWeightSnapshot;

		public BossBlessWeightSnapshot BossBlessWeightSnapshot
		{
			get
			{
				return bossBlessWeightSnapshot;
			}
			set
			{
				SetBossBlessWeightSnapshot(value);
			}
		}

		public void SetBossBlessWeightSnapshot(BossBlessWeightSnapshot snapshot)
		{
			bossBlessWeightSnapshot = snapshot?.Clone();
		}
	}

	public class EncounterConfig
	{
		[SaveField(null, null)]
		public RoomRef encounterRoom;

		[SaveField(null, null)]
		public WorldEvent encounterWorldEventTemplete;

		[SaveField(null, null)]
		public EnemyAssetRef encounterEnemy;

		[SaveField(null, null)]
		public int encounterEnemySetupIndex;

		[SaveField(null, null)]
		public EnemyEncounterType enemySpawnType;

		[SaveField(null, null)]
		public int initEnemyLevel;

		[SaveField(null, null)]
		public List<GameRecord.ItemRecord> replaceCharacterDeck;

		[SaveField(null, null)]
		public PVPDefender encounterPVPDefender;

		[SaveField(null, null)]
		public ImportedTestSetup importedTestSetup;

		[SaveField(null, null)]
		public PlayerCharacterDef trialCharacterDef;

		[SaveField(null, null)]
		public CharacterMastery trialMastery;

		[SaveField(null, null)]
		public List<SkillDef> trialSkills;

		[SaveField(null, null)]
		public bool trialSkillsMaxQuality;

		[SaveField(null, null)]
		public List<InfluenceDef> trialInfluenceDefs;

		[SaveField(null, null)]
		public bool useTestLocationEvent;

		[SaveField(null, null)]
		public PathLocation.LocationType testLocationType;

		[SaveField(null, null)]
		public PathLocation.LocationVariantType testLocationVariantType;
	}

	[SaveField(null, null)]
	public Contract contract;

	[SaveField(null, null)]
	public FixedContractRouteDef fixedContractRoute;

	[SaveField(null, null)]
	public DailyChallengeRunContext dailyChallengeContext;

	[SaveField(null, null)]
	public PlayerCharacterDef currentCharacter;

	[SaveField(null, null)]
	public CharacterUsageMode characterUsageMode;

	[SaveField(null, null)]
	public GeneratePrepareConfig startGameInfo;

	[SaveField(null, null)]
	public EncounterConfig encounterSetupConfig;

	[NonSerialized]
	private PlayerCharacterState limitedDeploymentCharacterCache;

	public static ApplicationStateInfo applicationState => Bootstrap.applicationState;

	public FixedContractRouteDef ResolvedFixedContractRoute
	{
		get
		{
			FixedContractRouteDef fixedContractRouteDef = fixedContractRoute;
			if ((object)fixedContractRouteDef == null)
			{
				Contract obj = contract;
				if (obj == null)
				{
					return null;
				}
				ContractDef contractDef = obj.contractDef;
				if ((object)contractDef == null)
				{
					return null;
				}
				fixedContractRouteDef = contractDef.fixedContractRoute;
			}
			return fixedContractRouteDef;
		}
	}

	public bool ToIntro => contract.contractDef.contractType == ContractDef.ContractType.Intro;

	public bool IsDailyChallenge => dailyChallengeContext?.isDailyChallenge ?? false;

	public bool IsLimitedDeploymentRun
	{
		get
		{
			if (characterUsageMode == CharacterUsageMode.LimitedDeployment)
			{
				return LimitedCharacterDeploymentService.IsTargetCharacter(currentCharacter);
			}
			return false;
		}
	}

	public void SetBossBlessWeightSnapshot(BossBlessWeightSnapshot snapshot)
	{
		if (startGameInfo == null)
		{
			startGameInfo = new GeneratePrepareConfig();
		}
		startGameInfo.SetBossBlessWeightSnapshot(snapshot);
	}

	private NewGameConfigData()
	{
	}

	public static NewGameConfigData Intro(AssetCatalog catalog)
	{
		return new NewGameConfigData
		{
			currentCharacter = ScriptableObjectSingleton<FortressDataManager>.instance.defaultCharacter,
			contract = ScriptableObjectSingleton<FortressDataManager>.instance.introContractDef.GenerateContract(catalog, DifficultyLevel.Normal),
			startGameInfo = GenerateLocalPrepareInfo()
		};
	}

	public static NewGameConfigData TestMenuNewGameConfig(TestMenuManager testMenuManager)
	{
		NewGameConfigData newGameConfigData = new NewGameConfigData();
		newGameConfigData.currentCharacter = testMenuManager.testCharacter;
		ContractDef contractDef = testMenuManager.selectedContract ?? ScriptableObjectSingleton<FortressDataManager>.instance.defaultContractDef;
		newGameConfigData.contract = contractDef.GenerateContract(testMenuManager.testAssetCatalog, testMenuManager.difficulty);
		newGameConfigData.fixedContractRoute = testMenuManager.fixedContractRoute;
		newGameConfigData.startGameInfo = GenerateLocalPrepareInfo();
		TestMenuConfigData testMenuConfigData = ScriptableObjectSingleton<GameLoadDataManager>.instance.testMenuConfigData;
		newGameConfigData.encounterSetupConfig = new EncounterConfig();
		newGameConfigData.encounterSetupConfig.replaceCharacterDeck = testMenuConfigData.initDeck.Select((ItemDefinition itd) => new GameRecord.ItemRecord(new Item(itd))).ToList();
		newGameConfigData.encounterSetupConfig.encounterRoom = testMenuConfigData.selectedRoom;
		newGameConfigData.encounterSetupConfig.initEnemyLevel = testMenuConfigData.initEnemyLevel;
		newGameConfigData.currentCharacter = testMenuConfigData.selectedCharacter;
		newGameConfigData.contract.difficultyLevel = testMenuConfigData.selectedDifficulty;
		newGameConfigData.encounterSetupConfig.encounterWorldEventTemplete = testMenuConfigData.selectEncounterOption.worldEvent;
		newGameConfigData.encounterSetupConfig.useTestLocationEvent = testMenuConfigData.selectEncounterOption.useLocationEvent;
		newGameConfigData.encounterSetupConfig.testLocationType = testMenuConfigData.selectEncounterOption.locationType;
		newGameConfigData.encounterSetupConfig.testLocationVariantType = testMenuConfigData.selectEncounterOption.locationVariantType;
		if (testMenuConfigData.initImportedTestSetup != null)
		{
			newGameConfigData.encounterSetupConfig.importedTestSetup = testMenuConfigData.initImportedTestSetup;
		}
		if (testMenuConfigData.selectEncounterOption.enemyInfoData != null)
		{
			newGameConfigData.encounterSetupConfig.encounterEnemy = new EnemyAssetRef(testMenuConfigData.selectEncounterOption.enemyInfoData);
			int num = testMenuConfigData.selectEncounterOption.enemyInfoData.setups.IndexOf(testMenuConfigData.selectEncounterOption.enemySetup);
			if (num >= 0)
			{
				newGameConfigData.encounterSetupConfig.encounterEnemySetupIndex = num;
			}
		}
		if (testMenuConfigData.selectEncounterOption.worldEvent != null)
		{
			newGameConfigData.encounterSetupConfig.encounterWorldEventTemplete = testMenuConfigData.selectEncounterOption.worldEvent;
		}
		newGameConfigData.encounterSetupConfig.enemySpawnType = testMenuConfigData.selectEncounterOption.enemyEncounterType;
		return newGameConfigData;
	}

	public static NewGameConfigData ToPVPDefenderBattleTest(ApplicationStateInfo asi, PVPDefender preset)
	{
		NewGameConfigData newGameConfigData = new NewGameConfigData();
		newGameConfigData.currentCharacter = asi.newGameConfigManager.currentNewGameConfig.currentCharacter;
		newGameConfigData.contract = ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpDefenderTestContractDef.GenerateContract(asi.assetCatalog, DifficultyLevel.Normal);
		newGameConfigData.startGameInfo = GenerateLocalPrepareInfo();
		newGameConfigData.encounterSetupConfig = new EncounterConfig();
		newGameConfigData.encounterSetupConfig.encounterRoom = ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpModeTestBattleRoomRef;
		newGameConfigData.encounterSetupConfig.initEnemyLevel = 1;
		newGameConfigData.encounterSetupConfig.encounterPVPDefender = preset;
		return newGameConfigData;
	}

	public static NewGameConfigData ToCharacterTrial(ApplicationStateInfo asi, PlayerCharacterDef trialCharacter, CharacterMastery mastery = null)
	{
		NewGameConfigData newGameConfigData = new NewGameConfigData();
		newGameConfigData.currentCharacter = trialCharacter;
		newGameConfigData.startGameInfo = GenerateLocalPrepareInfo();
		CharacterTrialConfig.TrialEntry entry = ScriptableObjectSingleton<CharacterTrialConfig>.instance.GetEntry(trialCharacter);
		newGameConfigData.contract = ScriptableObjectSingleton<CharacterTrialConfig>.instance.GenerateTrialContract(asi.assetCatalog, entry.difficulty);
		newGameConfigData.encounterSetupConfig = new EncounterConfig();
		newGameConfigData.encounterSetupConfig.trialCharacterDef = trialCharacter;
		newGameConfigData.encounterSetupConfig.trialMastery = mastery ?? entry.mastery;
		newGameConfigData.encounterSetupConfig.replaceCharacterDeck = entry.ToDeckRecords();
		newGameConfigData.encounterSetupConfig.trialSkills = entry.skills;
		newGameConfigData.encounterSetupConfig.trialSkillsMaxQuality = entry.upgradeSkillsToMaxQuality;
		newGameConfigData.encounterSetupConfig.trialInfluenceDefs = entry.influences;
		newGameConfigData.encounterSetupConfig.encounterRoom = entry.room;
		newGameConfigData.encounterSetupConfig.encounterEnemy = entry.enemy;
		newGameConfigData.encounterSetupConfig.encounterEnemySetupIndex = entry.enemySetupIndex;
		newGameConfigData.encounterSetupConfig.enemySpawnType = EnemyEncounterType.Elite;
		newGameConfigData.encounterSetupConfig.initEnemyLevel = entry.enemyLevel;
		return newGameConfigData;
	}

	public static NewGameConfigData DefaultStartGameConfig(ApplicationStateInfo asi)
	{
		return new NewGameConfigData
		{
			currentCharacter = ScriptableObjectSingleton<FortressDataManager>.instance.defaultCharacter,
			contract = asi.contractManager.defaultGameContract
		};
	}

	public static NewGameConfigData CreateDailyChallengeRunConfig(NewGameConfigData source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (!source.IsDailyChallenge)
		{
			throw new InvalidOperationException("Only daily challenge configs can be detached as run configs.");
		}
		return new NewGameConfigData
		{
			contract = source.contract,
			dailyChallengeContext = source.dailyChallengeContext,
			currentCharacter = source.currentCharacter,
			characterUsageMode = source.characterUsageMode
		};
	}

	public static NewGameConfigData DefaultStartGameConfig(AssetCatalog catalog)
	{
		return new NewGameConfigData
		{
			currentCharacter = ScriptableObjectSingleton<FortressDataManager>.instance.defaultCharacter,
			contract = ScriptableObjectSingleton<FortressDataManager>.instance.defaultContractDef.GenerateContract(catalog, DifficultyLevel.Normal),
			startGameInfo = GenerateLocalPrepareInfo()
		};
	}

	public static GeneratePrepareConfig GenerateLocalPrepareInfo(ApplicationStateInfo asi)
	{
		GeneratePrepareConfig generatePrepareConfig = new GeneratePrepareConfig();
		generatePrepareConfig.gameRandomSeed = (uint)DateTime.Now.Ticks;
		generatePrepareConfig.gameGuid = Guid.NewGuid().ToString();
		generatePrepareConfig.forerunnerLegacyThisGame = new List<ForerunnerLegacy>();
		if (asi.forerunnerManager.localForerunnerLegacy != null)
		{
			generatePrepareConfig.forerunnerLegacyThisGame.Add(asi.forerunnerManager.localForerunnerLegacy);
		}
		List<ForerunnerLegacy> list = ScriptableObjectSingleton<MockNetworkDataManager>.instance.shareForerunnerDataSetups.ConvertAll((MockNetworkDataManager.MockShareForerunnerDataSetup t) => t.ToData()).SelectRandomCountUnique(ScriptableObjectSingleton<GameDataManager>.instance.forerunnerLegacyPerGame);
		if (!list.IsNullOrEmpty())
		{
			generatePrepareConfig.forerunnerLegacyThisGame.AddRange(list);
		}
		List<PVPDefender> list2 = (from p in ScriptableObjectSingleton<MockNetworkDataManager>.instance.pvpDefenderDatas.SelectRandomCountUnique(ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpDefenderPerGame)
			select p.ToData(asi)).ToList();
		if (!list2.IsNullOrEmpty())
		{
			generatePrepareConfig.pvpDefenderThisGame = list2;
		}
		return generatePrepareConfig;
	}

	public static GeneratePrepareConfig GenerateLocalPrepareInfo()
	{
		return new GeneratePrepareConfig
		{
			gameRandomSeed = (uint)DateTime.Now.Ticks,
			gameGuid = Guid.NewGuid().ToString(),
			forerunnerLegacyThisGame = new List<ForerunnerLegacy>(),
			pvpDefenderThisGame = new List<PVPDefender>()
		};
	}

	public PlayerCharacterState GetCharacterState(ApplicationStateInfo applicationState)
	{
		return applicationState.characterManager.GetOrCreateCharacterState(currentCharacter);
	}

	public bool IsLimitedDeploymentSelected(ApplicationStateInfo applicationState)
	{
		if (!IsLimitedDeploymentRun)
		{
			return false;
		}
		PlayerCharacterState characterState = GetCharacterState(applicationState);
		return LimitedCharacterDeploymentService.CanSelect(applicationState, characterState, this);
	}

	public PlayerCharacterState GetDisplayCharacterState(ApplicationStateInfo applicationState)
	{
		if (!IsLimitedDeploymentSelected(applicationState))
		{
			return GetCharacterState(applicationState);
		}
		if (limitedDeploymentCharacterCache == null || limitedDeploymentCharacterCache.def != currentCharacter)
		{
			limitedDeploymentCharacterCache = LimitedCharacterDeploymentService.CreateTemporaryCharacter(applicationState, currentCharacter);
		}
		return limitedDeploymentCharacterCache;
	}

	public PlayerCharacterState GetSkillConfigCharacterState(ApplicationStateInfo applicationState)
	{
		PlayerCharacterDef characterDef = (IsLimitedDeploymentRun ? ScriptableObjectSingleton<FortressDataManager>.instance.defaultCharacter : currentCharacter);
		return applicationState.characterManager.GetOrCreateCharacterState(characterDef);
	}

	public PlayerCharacterState CreateCharacterStateForRun(ApplicationStateInfo applicationState)
	{
		if (!IsLimitedDeploymentRun)
		{
			return GetCharacterState(applicationState);
		}
		return LimitedCharacterDeploymentService.CreateTemporaryCharacter(applicationState, currentCharacter);
	}

	public void SetCharacter(PlayerCharacterDef characterDef)
	{
		currentCharacter = characterDef;
		characterUsageMode = CharacterUsageMode.Owned;
		limitedDeploymentCharacterCache = null;
		ClampDifficultyToCharacterProgress();
		applicationState.playerInfoManager.showUnlockTalent = true;
		applicationState.savingManager.SetProfileDirtyToSave();
	}

	public bool SetLimitedDeploymentCharacter(PlayerCharacterDef characterDef)
	{
		PlayerCharacterState orCreateCharacterState = applicationState.characterManager.GetOrCreateCharacterState(characterDef);
		if (!LimitedCharacterDeploymentService.CanSelect(applicationState, orCreateCharacterState, this))
		{
			return false;
		}
		currentCharacter = characterDef;
		characterUsageMode = CharacterUsageMode.LimitedDeployment;
		limitedDeploymentCharacterCache = null;
		ClampDifficultyToCharacterProgress();
		applicationState.playerInfoManager.showUnlockTalent = true;
		applicationState.savingManager.SetProfileDirtyToSave();
		return true;
	}

	public void ResetToDefaultCharacter(ApplicationStateInfo applicationState)
	{
		currentCharacter = ScriptableObjectSingleton<FortressDataManager>.instance.defaultCharacter;
		characterUsageMode = CharacterUsageMode.Owned;
		limitedDeploymentCharacterCache = null;
		applicationState?.savingManager?.SetProfileDirtyToSave();
	}

	public bool CanSelectDifficulty(ApplicationStateInfo applicationState, DifficultyLevel level)
	{
		if (applicationState == null || contract?.contractDef == null)
		{
			return false;
		}
		if (contract.contractDef.IsDifficultyIsolated)
		{
			return level == contract.difficultyLevel;
		}
		CharacterClassDef characterClassDef = currentCharacter?.attachClass;
		if (characterClassDef == null)
		{
			return applicationState.difficultyManager.IsDifficultyUnlocked(level);
		}
		ContractDef.ContractType contractType = contract.contractDef.contractType;
		bool num = !IsDailyChallenge && (contractType == ContractDef.ContractType.Challenge || contractType == ContractDef.ContractType.HardChallenge || contractType == ContractDef.ContractType.WeekShuffle);
		DifficultyLevel mainlineDifficulty = applicationState.idolManager.GetMainlineDifficulty(characterClassDef.mClass);
		DifficultyLevel difficultyLevel = ((!num) ? applicationState.idolManager.GetMaxSelectableMainlineDifficulty(characterClassDef.mClass) : ((mainlineDifficulty < DifficultyLevel.Normal) ? DifficultyLevel.Normal : mainlineDifficulty));
		return level <= difficultyLevel;
	}

	public bool ClearDailyChallengeContext()
	{
		if (dailyChallengeContext == null)
		{
			return false;
		}
		dailyChallengeContext = null;
		return true;
	}

	private void ClampDifficultyToCharacterProgress()
	{
		Contract obj = contract;
		if ((obj == null || obj.contractDef?.IsDifficultyIsolated != true) && !(currentCharacter?.attachClass == null))
		{
			DifficultyLevel maxSelectableMainlineDifficulty = applicationState.idolManager.GetMaxSelectableMainlineDifficulty(currentCharacter.attachClass.mClass);
			if (contract.difficultyLevel > maxSelectableMainlineDifficulty)
			{
				contract.difficultyLevel = maxSelectableMainlineDifficulty;
			}
		}
	}

	public CharacterMastery GetMastery(ApplicationStateInfo applicationState)
	{
		if (IsLimitedDeploymentRun)
		{
			return LimitedCharacterDeploymentService.GetFixedMastery(currentCharacter);
		}
		return GetCharacterState(applicationState)?.currentMastery;
	}
}
