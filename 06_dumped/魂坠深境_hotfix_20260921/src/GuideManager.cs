using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class GuideManager : GlobalGameManager
{
	public bool blockPlayerInput;

	public bool skipDeathPendingFinalPanel;

	public bool blockEnemyDrawItems;

	public bool showEndTurnButton;

	[SaveField(null, null)]
	public List<string> finishedGuides;

	[SaveField(null, null)]
	public List<InfluenceDef> guideBless;

	[SaveField(null, null)]
	public List<EnemyAssetRef> guideEnemies;

	[SaveField(null, null)]
	public List<ItemDefinition> guideLoots;

	[SaveField(null, null)]
	public List<RuneDef> guideRunes;

	[SaveField(null, null)]
	public List<ItemDefinition> guideItems;

	[SaveField(null, null)]
	public List<ItemDefinition> itemSkinRecommendedItems;

	[SaveField(null, null)]
	public bool introFinished;

	[SaveField(null, null)]
	public bool skipGuide;

	[SaveField(null, null)]
	public List<string> autoPopupedHints;

	[SaveField(null, null)]
	private List<string> showedGuideLoadingHint;

	public GuideModule_Intro RunningIntroModule { get; private set; }

	public bool IsAfterIntroFirstGame
	{
		get
		{
			if (!base.applicationState.guideManager.skipGuide && base.applicationState.recordManager.FinishedGameCount == 0)
			{
				return !ScriptableObjectSingleton<GuideData>.instance.afterFirstGameStoryPoint.IsFinished(base.applicationState);
			}
			return false;
		}
	}

	public bool IsAfterIntroProtect
	{
		get
		{
			if (IsAfterIntroFirstGame)
			{
				GameStateInfo gameStateInfo = base.gameState;
				if (gameStateInfo != null && gameStateInfo.difficultyState.contractDifficulty == DifficultyLevel.Normal)
				{
					GameStateInfo gameStateInfo2 = base.gameState;
					if (gameStateInfo2 == null)
					{
						return false;
					}
					return gameStateInfo2.contractState.stage <= 2;
				}
			}
			return false;
		}
	}

	public bool IsPostFirstGameTalentFocusMode
	{
		get
		{
			if (skipGuide || base.applicationState?.globalVariableManager == null)
			{
				return false;
			}
			StoryPoint storyPoint = ScriptableObjectSingleton<GuideData>.instance?.afterFirstGameStoryPoint;
			if (storyPoint != null && storyPoint.IsFinished(base.applicationState))
			{
				return !base.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressStoreHousePanel);
			}
			return false;
		}
	}

	public bool IsSecondFormalGameBeforeShapeRewardProtect
	{
		get
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo == null || !gameStateInfo.secondFormalGameFixedRun || base.gameState.contractState.stage != 1)
			{
				return false;
			}
			ItemDefinition itemDef = ScriptableObjectSingleton<GuideData>.instance.silentCrownItemDef;
			ItemSkinDef skinDef = ScriptableObjectSingleton<GuideData>.instance.silentCrownCybSkinDef;
			if (itemDef == null || skinDef == null || base.gameState.playerState?.DeckContainer == null)
			{
				return false;
			}
			return base.gameState.playerState.DeckContainer.GetAllItems().All((Item item) => item == null || item.def != itemDef || item.skin != skinDef);
		}
	}

	public bool ShouldRejectDeathForGuide
	{
		get
		{
			if (!IsAfterIntroProtect)
			{
				return IsSecondFormalGameBeforeShapeRewardProtect;
			}
			return true;
		}
	}

	public bool ShouldStartSecondFormalGameFixedRun(NewGameConfigData config)
	{
		if (skipGuide || config == null || config.currentCharacter == null)
		{
			return false;
		}
		if (base.applicationState.recordManager.FinishedGameCount != 1 || config.ToIntro || config.encounterSetupConfig != null)
		{
			return false;
		}
		Contract obj = config.contract ?? base.applicationState.contractManager.defaultGameContract;
		if (obj == null || obj.contractDef?.contractType != ContractDef.ContractType.BaseGame)
		{
			return false;
		}
		if (config.currentCharacter != ScriptableObjectSingleton<FortressDataManager>.instance.defaultCharacter)
		{
			return false;
		}
		StoryPoint afterFirstGameTalentStoryPoint = ScriptableObjectSingleton<GuideData>.instance.afterFirstGameTalentStoryPoint;
		if (afterFirstGameTalentStoryPoint != null)
		{
			return afterFirstGameTalentStoryPoint.IsFinished(base.applicationState);
		}
		return false;
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (finishedGuides == null)
		{
			finishedGuides = new List<string>();
		}
		if (showedGuideLoadingHint == null)
		{
			showedGuideLoadingHint = new List<string>();
		}
		if (autoPopupedHints == null)
		{
			autoPopupedHints = new List<string>();
		}
		showEndTurnButton = true;
		if (applicationState.bootStrapLoadData.completeIntroBeforeFirstGame)
		{
			CompleteIntroForTest();
		}
		if (applicationState.bootStrapLoadData.skipGuide)
		{
			skipGuide = true;
		}
		if (applicationState.bootStrapLoadData.skipGuide)
		{
			introFinished = true;
			foreach (GuideModule guideModule in ScriptableObjectSingleton<GuideData>.instance.guideModules)
			{
				if (!finishedGuides.Contains(guideModule.id))
				{
					finishedGuides.Add(guideModule.id);
				}
			}
		}
		if (!applicationState.bootStrapLoadData.skipGuide)
		{
			foreach (GuideModule guideModule2 in ScriptableObjectSingleton<GuideData>.instance.guideModules)
			{
				if (guideModule2 is GuideModule_SpiderContract || !finishedGuides.Contains(guideModule2.id))
				{
					guideModule2.Init(applicationState);
					guideModule2.RunModule();
				}
			}
		}
		else
		{
			foreach (GuideModule guideModule3 in ScriptableObjectSingleton<GuideData>.instance.guideModules)
			{
				if (guideModule3 is GuideModule_SpiderContract)
				{
					guideModule3.Init(applicationState);
					guideModule3.RunModule();
				}
				else if (!finishedGuides.Contains(guideModule3.id))
				{
					guideModule3.Init(applicationState);
					guideModule3.Finish();
				}
			}
		}
		bool loadedLegacyGuideItems = guideItems != null;
		if (guideLoots == null)
		{
			guideLoots = ScriptableObjectSingleton<GuideData>.instance.guideLoots.ToList();
		}
		if (guideEnemies == null)
		{
			guideEnemies = ScriptableObjectSingleton<GuideData>.instance.guideEnemies.ToList();
		}
		if (guideItems == null)
		{
			guideItems = ScriptableObjectSingleton<GuideData>.instance.guideWarriorCharacterItems.ToList();
		}
		InitItemSkinRecommendations(loadedLegacyGuideItems);
		if (guideBless == null)
		{
			guideBless = ScriptableObjectSingleton<GuideData>.instance.guideBless.ToList();
		}
		if (guideRunes == null)
		{
			guideRunes = ScriptableObjectSingleton<GuideData>.instance.guideRunes.ToList();
		}
		if (applicationState.bootStrapLoadData.skipGuide)
		{
			CleanGuideGeneration();
		}
		blockPlayerInput = false;
	}

	public string GenerateGuideHint()
	{
		List<LocalizeString>[] array = new List<LocalizeString>[3]
		{
			ScriptableObjectSingleton<GuideData>.instance.loadingHintTextA1,
			ScriptableObjectSingleton<GuideData>.instance.loadingHintTextA2,
			ScriptableObjectSingleton<GuideData>.instance.loadingHintTextA3
		};
		List<LocalizeString>[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			List<LocalizeString> list = array2[i].Where((LocalizeString t) => !showedGuideLoadingHint.Contains(t.str)).ToList();
			if (list.Any())
			{
				LocalizeString localizeString = list.SelectRandom();
				showedGuideLoadingHint.Add(localizeString.str);
				return localizeString.str;
			}
		}
		return array.SelectMany((List<LocalizeString> hintPool) => hintPool).Concat(ScriptableObjectSingleton<GuideData>.instance.loadingHintTextB).ToList()
			.SelectRandom()
			.str;
	}

	public void RunIntroModule(GameStateInfo gameStateInfo)
	{
		ScriptableObjectSingleton<GuideData>.instance.introGuide.Init(base.applicationState);
		ScriptableObjectSingleton<GuideData>.instance.introGuide.RunModule();
		RunningIntroModule = ScriptableObjectSingleton<GuideData>.instance.introGuide;
	}

	public void CompleteIntroForTest()
	{
		introFinished = true;
		skipGuide = false;
		GuideModule_Intro introGuide = ScriptableObjectSingleton<GuideData>.instance.introGuide;
		if (introGuide != null && !finishedGuides.Contains(introGuide.id))
		{
			finishedGuides.Add(introGuide.id);
		}
	}

	public bool IsGuideModuleFinished<T>() where T : GuideModule
	{
		return finishedGuides.Contains(ScriptableObjectSingleton<GuideData>.instance.GetGuideModule<T>().id);
	}

	private void InitItemSkinRecommendations(bool loadedLegacyGuideItems)
	{
		if (itemSkinRecommendedItems != null)
		{
			return;
		}
		itemSkinRecommendedItems = new List<ItemDefinition>();
		if (!loadedLegacyGuideItems || guideItems.IsNullOrEmpty())
		{
			return;
		}
		List<ItemDefinition> guideWarriorCharacterItems = ScriptableObjectSingleton<GuideData>.instance.guideWarriorCharacterItems;
		int startIndex = 0;
		List<ItemDefinition> list = new List<ItemDefinition>();
		foreach (ItemDefinition item in guideItems)
		{
			int num = guideWarriorCharacterItems.FindIndex(startIndex, (ItemDefinition tutorialItem) => tutorialItem == item);
			if (num >= 0)
			{
				list.Add(item);
				startIndex = num + 1;
			}
			else if (item != null)
			{
				itemSkinRecommendedItems.AddUnique(item);
			}
		}
		guideItems = list;
	}

	public void CleanGuideGeneration()
	{
		guideBless.Clear();
		guideEnemies.Clear();
		guideLoots.Clear();
		guideRunes.Clear();
		guideItems.Clear();
		itemSkinRecommendedItems.Clear();
	}

	public void ReapplySkipGuideAfterProfileFetch()
	{
		if (!base.applicationState.bootStrapLoadData.skipGuide)
		{
			return;
		}
		skipGuide = true;
		introFinished = true;
		if (finishedGuides == null)
		{
			finishedGuides = new List<string>();
		}
		foreach (GuideModule guideModule in ScriptableObjectSingleton<GuideData>.instance.guideModules)
		{
			if (!finishedGuides.Contains(guideModule.id))
			{
				finishedGuides.Add(guideModule.id);
			}
		}
	}
}
