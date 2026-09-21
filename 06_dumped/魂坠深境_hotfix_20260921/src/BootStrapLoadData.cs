using System.Linq;

public class BootStrapLoadData
{
	public enum TargetContext
	{
		GameMap,
		Fortress,
		TestMenu,
		GameBattle,
		MainMenu,
		AfterLoginLoad,
		Intro
	}

	public enum EnterFortressType
	{
		Default,
		PvpEnemyPanel,
		CharacterInfoPanel
	}

	public enum FortressReturnTarget
	{
		None,
		ChallengeConstellation,
		DailyChallenge
	}

	public TargetContext targetContext;

	public EnterFortressType enterFortressType;

	public string profilePlayerId;

	public PlayerCharacterDef enterFortressCharacter;

	public Level loadingTargetLevel;

	public NewGameConfigData newGameConfigData;

	public bool bootStrapInit;

	public bool dropCurrentGame;

	public bool skipFortressAutoPopups;

	public FortressReturnTarget fortressReturnTarget;

	public ContractDef fortressReturnContractDef;

	public bool? toIntro;

	public bool? toGameScene;

	public bool skipGuide;

	public bool skipStory;

	public bool startUnlockAll;

	public bool completeIntroBeforeFirstGame;

	public bool IsNewGame => newGameConfigData != null;

	public bool HasFortressReturnIntent => fortressReturnTarget != FortressReturnTarget.None;

	public static BootStrapLoadData ContinueGame(string playerId, Level continueLevel)
	{
		return new BootStrapLoadData
		{
			profilePlayerId = playerId,
			targetContext = TargetContext.GameMap,
			loadingTargetLevel = continueLevel
		};
	}

	public static BootStrapLoadData LoadToFortress(string playerId, EnterFortressType enterFortressType = EnterFortressType.Default)
	{
		return new BootStrapLoadData
		{
			profilePlayerId = playerId,
			targetContext = TargetContext.Fortress,
			enterFortressType = enterFortressType
		};
	}

	public static BootStrapLoadData LoadToFortressAfterGameRecord(string playerId, GameRecord record)
	{
		BootStrapLoadData bootStrapLoadData = LoadToFortress(playerId);
		if (record != null && record.IsDailyChallenge)
		{
			bootStrapLoadData.fortressReturnTarget = FortressReturnTarget.DailyChallenge;
			return bootStrapLoadData;
		}
		ContractDef contractDef = record?.contract?.contractDef;
		if (contractDef == null)
		{
			return bootStrapLoadData;
		}
		ContractDef.ContractType contractType = contractDef.contractType;
		if (contractType == ContractDef.ContractType.Challenge || contractType == ContractDef.ContractType.HardChallenge || contractType == ContractDef.ContractType.WeekShuffle)
		{
			bootStrapLoadData.fortressReturnTarget = FortressReturnTarget.ChallengeConstellation;
			bootStrapLoadData.fortressReturnContractDef = contractDef;
		}
		return bootStrapLoadData;
	}

	public static BootStrapLoadData LoadToFortressCharacterInfo(string playerId, PlayerCharacterDef character)
	{
		BootStrapLoadData bootStrapLoadData = LoadToFortress(playerId, EnterFortressType.CharacterInfoPanel);
		bootStrapLoadData.enterFortressCharacter = character;
		bootStrapLoadData.skipFortressAutoPopups = true;
		return bootStrapLoadData;
	}

	public static BootStrapLoadData ToStartNewGame(NewGameConfigData customConfigData, string playerId, Level newGameLevel)
	{
		return new BootStrapLoadData
		{
			profilePlayerId = playerId,
			targetContext = TargetContext.GameMap,
			loadingTargetLevel = newGameLevel,
			newGameConfigData = customConfigData
		};
	}

	public static BootStrapLoadData ToTestMenu()
	{
		return new BootStrapLoadData
		{
			targetContext = TargetContext.TestMenu,
			profilePlayerId = "Default"
		};
	}

	public static BootStrapLoadData ToMainMenu()
	{
		return new BootStrapLoadData
		{
			profilePlayerId = null,
			targetContext = TargetContext.MainMenu
		};
	}

	public static BootStrapLoadData ToIntro(string playerId, AssetCatalog catalog)
	{
		return new BootStrapLoadData
		{
			profilePlayerId = playerId,
			targetContext = TargetContext.Intro,
			loadingTargetLevel = ScriptableObjectSingleton<GuideData>.instance.introLevel
		};
	}

	public static BootStrapLoadData ToFirstGameAfterCompletedIntroForTest(string playerId, AssetCatalog catalog)
	{
		return new BootStrapLoadData
		{
			profilePlayerId = playerId,
			targetContext = TargetContext.GameMap,
			newGameConfigData = NewGameConfigData.DefaultStartGameConfig(catalog),
			completeIntroBeforeFirstGame = true
		};
	}

	public static BootStrapLoadData ToAfterLoginLoad(string playerId, bool dropCurrentGame, bool skipGuide = false)
	{
		return new BootStrapLoadData
		{
			profilePlayerId = playerId,
			targetContext = TargetContext.AfterLoginLoad,
			dropCurrentGame = dropCurrentGame,
			skipGuide = skipGuide
		};
	}

	public static BootStrapLoadData ToPvpDefenderBattleTest(ApplicationStateInfo asi, PVPDefender preset)
	{
		return new BootStrapLoadData
		{
			profilePlayerId = Bootstrap.instance.CurrentAccountID,
			targetContext = TargetContext.GameBattle,
			loadingTargetLevel = ScriptableObjectSingleton<GuideData>.instance.introLevel,
			newGameConfigData = NewGameConfigData.ToPVPDefenderBattleTest(asi, preset)
		};
	}

	public static BootStrapLoadData ToCharacterTrialBattle(ApplicationStateInfo asi, PlayerCharacterDef trialCharacter, CharacterMastery mastery = null)
	{
		return new BootStrapLoadData
		{
			profilePlayerId = Bootstrap.instance.CurrentAccountID,
			targetContext = TargetContext.GameBattle,
			loadingTargetLevel = ScriptableObjectSingleton<GuideData>.instance.introLevel,
			newGameConfigData = NewGameConfigData.ToCharacterTrial(asi, trialCharacter, mastery)
		};
	}

	public static BootStrapLoadData FromBattleParams(bool randomInitItem, bool newGame, bool newProfile, TargetContext target, bool withIntro, bool completeIntroBeforeFirstGame = false)
	{
		if (target == TargetContext.MainMenu)
		{
			return ToMainMenu();
		}
		if (newProfile)
		{
			SavingUtil.CreateProfileData("Default");
		}
		if (completeIntroBeforeFirstGame)
		{
			return ToFirstGameAfterCompletedIntroForTest("Default", TestMenuManager.instance.testAssetCatalog);
		}
		if (withIntro)
		{
			return ToIntro("Default", TestMenuManager.instance.testAssetCatalog);
		}
		BootStrapLoadData bootStrapLoadData = new BootStrapLoadData();
		bootStrapLoadData.profilePlayerId = "Default";
		bootStrapLoadData.targetContext = target;
		bootStrapLoadData.skipGuide = TestTogglePanel.instance.skipGuide;
		bootStrapLoadData.skipStory = TestTogglePanel.instance.skipStory;
		bootStrapLoadData.startUnlockAll = TestTogglePanel.instance.startUnlockAll;
		if (newGame)
		{
			bootStrapLoadData.newGameConfigData = NewGameConfigData.TestMenuNewGameConfig(TestMenuManager.instance);
		}
		return bootStrapLoadData;
	}

	public EncounterSetupInfo BuildInitEncounterSetupInfo(ApplicationStateInfo asi)
	{
		if (newGameConfigData.encounterSetupConfig == null)
		{
			return null;
		}
		NewGameConfigData.EncounterConfig encounterSetupConfig = newGameConfigData.encounterSetupConfig;
		if (encounterSetupConfig.useTestLocationEvent)
		{
			return BuildTestLocationEncounterSetupInfo(asi, encounterSetupConfig);
		}
		EncounterSetupInfo encounterSetupInfo = new EncounterSetupInfo();
		if (encounterSetupConfig.encounterEnemy != null)
		{
			if (!encounterSetupConfig.encounterEnemy.IsLoaded(asi.assetCatalog))
			{
				encounterSetupConfig.encounterEnemy.LoadSync(asi.assetCatalog);
			}
			encounterSetupInfo.fixedEnemyInfoData = encounterSetupConfig.encounterEnemy.GetAssetPreData(asi.assetCatalog);
			encounterSetupInfo.fixedEnemySetupIndex = encounterSetupConfig.encounterEnemySetupIndex;
			encounterSetupInfo.fixedEnemySetup = encounterSetupInfo.fixedEnemyInfoData.setups[encounterSetupConfig.encounterEnemySetupIndex];
			encounterSetupInfo.enemySpawnType = encounterSetupConfig.enemySpawnType;
		}
		encounterSetupInfo.pvpDefender = encounterSetupConfig.encounterPVPDefender;
		encounterSetupInfo.level = ScriptableObjectSingleton<DungeonGenerationManager>.instance.cityLevel;
		encounterSetupInfo.roomInfoData = encounterSetupConfig.encounterRoom.GetAssetPreData(asi.assetCatalog);
		encounterSetupInfo.fixedEventTemplate = encounterSetupConfig.encounterWorldEventTemplete ?? ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate;
		encounterSetupInfo.enemyLevel = encounterSetupConfig.initEnemyLevel;
		return encounterSetupInfo;
	}

	private EncounterSetupInfo BuildTestLocationEncounterSetupInfo(ApplicationStateInfo asi, NewGameConfigData.EncounterConfig encounterSetupConfig)
	{
		GameStateInfo runningGameState = asi.runningGameState;
		DungeonMap dungeonMap = runningGameState?.dungeonMapManager?.currentDungeonMap;
		if (runningGameState == null || dungeonMap == null)
		{
			return null;
		}
		PathLocation pathLocation = dungeonMap.locations.FirstOrDefault((PathLocation l) => l.type == encounterSetupConfig.testLocationType) ?? dungeonMap.initLocation ?? dungeonMap.locations.FirstOrDefault();
		if (pathLocation == null)
		{
			return null;
		}
		pathLocation.type = encounterSetupConfig.testLocationType;
		pathLocation.variantType = encounterSetupConfig.testLocationVariantType;
		pathLocation.enemyDifficultyLevel = encounterSetupConfig.initEnemyLevel;
		dungeonMap.currentLocation = pathLocation;
		LocationGenerator locationGenerator = ScriptableObjectSingleton<DungeonGenerationManager>.instance.GetLocationGenerator(pathLocation.type);
		if (locationGenerator == null)
		{
			return null;
		}
		pathLocation.attachEncounterSetupInfo = locationGenerator.GenerateRoomSetupInfo(runningGameState, dungeonMap, pathLocation);
		return pathLocation.attachEncounterSetupInfo;
	}
}
