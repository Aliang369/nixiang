using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class SpiderScavengeWorldEvent : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText scavengeChoiceText;

	[SerializeField]
	private WorldEventChoiceText continueChoiceText;

	[SerializeField]
	private WorldEventChoiceText leaveChoiceText;

	[SerializeField]
	private WorldEventChoiceText battleChoiceText;

	[SerializeField]
	private List<BonusGenerationInfo> rewardPool;

	[SerializeField]
	private ItemDefinition counterRewardItem;

	[SerializeField]
	private int counterRewardAmount;

	[SerializeField]
	private int maxScavengeCount = 3;

	[SerializeField]
	private int initialBattleChancePercent;

	[SerializeField]
	private int battleChanceAddPercent;

	[SerializeField]
	private int maxBattleChancePercent = 100;

	[SerializeField]
	private List<EnemyAssetRef> enemyPool;

	[SerializeField]
	private int enemySetupIndex = -1;

	[SerializeField]
	private EnemyEncounterType enemyEncounterType;

	[SerializeField]
	private bool enableBattleOnScavenge;

	[SerializeField]
	private bool useContinuationDisplay;

	[SerializeField]
	[TextArea]
	private string exhaustedDescription;

	[NonSerialized]
	private int currentBattleChancePercent;

	[NonSerialized]
	private int searchedCount;

	[NonSerialized]
	private HashSet<int> usedRewardIndexes;

	[NonSerialized]
	private bool battleTriggered;

	public override bool KeepTitleAfterChoice => useContinuationDisplay;

	protected override void Init()
	{
		base.Init();
		currentBattleChancePercent = Mathf.Clamp(initialBattleChancePercent, 0, 100);
		searchedCount = 0;
		usedRewardIndexes = new HashSet<int>();
		battleTriggered = false;
		SetupScavengeChoices(scavengeChoiceText);
	}

	private void SetupScavengeChoices(WorldEventChoiceText scavengeText)
	{
		currentDisplayInfo.ClearChoice();
		currentDisplayInfo.AddChoice(new WorldEventChoice
		{
			buttonText = scavengeText.choiceName,
			descriptionText = FormatChanceText(scavengeText.choiceDescription),
			executeAbleChecker = CanScavenge,
			executeAction = Scavenge,
			stayAfterChoose = true
		});
		currentDisplayInfo.AddChoice(new WorldEventChoice
		{
			buttonText = leaveChoiceText.choiceName,
			descriptionText = leaveChoiceText.choiceDescription,
			executeAction = delegate
			{
				FinishedWorldEvent();
			}
		});
		SetPanelDirty();
	}

	private void Scavenge()
	{
		if (!CanScavenge())
		{
			return;
		}
		int num = currentBattleChancePercent;
		BattleBonus battleBonus = GenerateRewardBonus();
		searchedCount++;
		GrantCounterReward();
		bool flag = gameState.randomManager.NextFloat(0f, 100f) < (float)num;
		battleTriggered = flag && enableBattleOnScavenge && HasConfiguredBattle();
		if (flag && !battleTriggered)
		{
			SpiderContractRuleDef spiderRule = GetSpiderRule();
			int oldActivity = spiderRule?.GetNestActivityCurrent(gameState) ?? 0;
			spiderRule?.ChangeNestActivity(gameState, 1);
			int newActivity = spiderRule?.GetNestActivityCurrent(gameState) ?? oldActivity;
			EnqueueAction(new ActionAction(delegate
			{
				gameState.UIManager.dungeonMapPanel?.PlayNestActivityIncreased(oldActivity, newActivity);
			}));
		}
		currentBattleChancePercent = Mathf.Clamp(currentBattleChancePercent + battleChanceAddPercent, 0, maxBattleChancePercent);
		if (battleBonus == null || battleBonus.bonuses.IsNullOrEmpty())
		{
			ResolveAfterReward();
			return;
		}
		EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
		{
			items = (from itemBonus in battleBonus.bonuses.OfType<GetSingleItemBonus>()
				select itemBonus.item into item
				where item != null
				select item).ToList()
		});
		ResolveAfterReward();
	}

	private BattleBonus GenerateRewardBonus()
	{
		List<BonusGenerationInfo> pool = rewardPool ?? new List<BonusGenerationInfo>();
		List<int> list = (from index in Enumerable.Range(0, pool.Count)
			where pool[index] != null && !usedRewardIndexes.Contains(index)
			select index).ToList();
		if (list.IsNullOrEmpty())
		{
			return null;
		}
		int num = list.SelectRandom(gameState.randomManager.NextDouble);
		usedRewardIndexes.Add(num);
		List<BattleBonusObject> list2 = pool[num]?.GenerateBonusObject(gameState);
		if (!list2.IsNullOrEmpty())
		{
			return new BattleBonus(list2);
		}
		return null;
	}

	private bool CanScavenge()
	{
		if (searchedCount >= maxScavengeCount)
		{
			return false;
		}
		if (CanGrantCounterReward())
		{
			return true;
		}
		List<BonusGenerationInfo> pool = rewardPool ?? new List<BonusGenerationInfo>();
		return Enumerable.Range(0, pool.Count).Any((int index) => pool[index] != null && !usedRewardIndexes.Contains(index));
	}

	private void ResolveAfterReward()
	{
		currentDisplayInfo.ClearChoice();
		if (battleTriggered)
		{
			currentDisplayInfo.description = battleChoiceText.chosenDescription;
			currentDisplayInfo.AddChoice(new WorldEventChoice
			{
				buttonText = battleChoiceText.choiceName,
				descriptionText = battleChoiceText.choiceDescription,
				executeAction = EnterConfiguredBattle
			});
		}
		else if (!CanScavenge())
		{
			currentDisplayInfo.description = exhaustedDescription;
			currentDisplayInfo.AddChoice(new WorldEventChoice
			{
				buttonText = leaveChoiceText.choiceName,
				descriptionText = leaveChoiceText.choiceDescription,
				executeAction = delegate
				{
					FinishedWorldEvent();
				}
			});
		}
		else
		{
			currentDisplayInfo.description = ((useContinuationDisplay || searchedCount > 1) ? continueChoiceText.chosenDescription : scavengeChoiceText.chosenDescription);
			SetupScavengeChoices(continueChoiceText);
		}
		SetPanelDirty();
	}

	private bool HasConfiguredBattle()
	{
		if (battleChoiceText != null)
		{
			return enemyPool.OrEmptyIfNull().Any((EnemyAssetRef enemy) => enemy?.IsValidRef(gameState.applicationState.assetCatalog) ?? false);
		}
		return false;
	}

	private void EnterConfiguredBattle()
	{
		EncounterSetupInfo setupInfo = gameState.currentEncounter.setupInfo;
		setupInfo.fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate;
		setupInfo.enemySpawnType = enemyEncounterType;
		SetupBattleReward(setupInfo);
		EnemyAssetRef enemyAssetRef = (from enemy in enemyPool.OrEmptyIfNull()
			where enemy?.IsValidRef(gameState.applicationState.assetCatalog) ?? false
			select enemy).ToList().SelectRandom(gameState.randomManager.NextDouble);
		if (enemyAssetRef != null)
		{
			setupInfo.fixedEnemyInfoData = enemyAssetRef.GetAssetPreData(gameState.applicationState.assetCatalog);
			if (enemySetupIndex >= 0 && setupInfo.fixedEnemyInfoData.setups.Count > enemySetupIndex)
			{
				setupInfo.fixedEnemySetupIndex = enemySetupIndex;
				setupInfo.fixedEnemySetup = setupInfo.fixedEnemyInfoData.setups[enemySetupIndex];
			}
		}
		FinishedWorldEvent(exitRoom: false);
		EnqueueAction(new TransitReloadRoomAction(setupInfo));
	}

	private void SetupBattleReward(EncounterSetupInfo setupInfo)
	{
		if (enemyEncounterType == EnemyEncounterType.Elite)
		{
			BattleRewardRule obj = gameState.contractState?.selectedContract?.contractDef?.GetBattleRewardRule(gameState) ?? BattleRewardRule.Default;
			setupInfo.roomBonus = new BattleBonus();
			obj.AddEliteFightBonus(gameState, setupInfo.roomBonus);
		}
	}

	private string FormatChanceText(string raw)
	{
		if (string.IsNullOrEmpty(raw))
		{
			return raw;
		}
		try
		{
			return raw.FormatWith("", GetNestAlertRiskText());
		}
		catch (FormatException)
		{
			return raw;
		}
	}

	private bool CanGrantCounterReward()
	{
		if (counterRewardAmount != 0)
		{
			return GetCounterRewardItem() != null;
		}
		return false;
	}

	private void GrantCounterReward()
	{
		Item item = GetCounterRewardItem();
		if (item != null && counterRewardAmount != 0)
		{
			EnqueueAction(new ChangeCounterAction(counterRewardAmount, item, this, withPopupText: true));
		}
	}

	private Item GetCounterRewardItem()
	{
		SpiderContractRuleDef spiderRule = GetSpiderRule();
		if (spiderRule != null && counterRewardItem != null && (counterRewardItem == spiderRule.insecticideOne || counterRewardItem == spiderRule.insecticideTwo || counterRewardItem == spiderRule.insecticideThree))
		{
			return spiderRule.GetPotion(gameState);
		}
		return gameState?.playerState?.bft?.GetAllRealItems().FirstOrDefault((Item item) => item?.GetDef() == counterRewardItem);
	}

	private string GetNestAlertRiskText()
	{
		if (currentBattleChancePercent <= initialBattleChancePercent)
		{
			return "有小概率";
		}
		if (currentBattleChancePercent <= initialBattleChancePercent + battleChanceAddPercent)
		{
			return "有可能";
		}
		return "很可能";
	}

	private SpiderContractRuleDef GetSpiderRule()
	{
		return gameState?.contractState?.selectedContract?.contractDef?.contractRule as SpiderContractRuleDef;
	}
}
