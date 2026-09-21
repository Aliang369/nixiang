using System;
using UnityEngine;

public class SpiderTorchNestWorldEvent : WorldEvent
{
	private enum NestChoiceState
	{
		Initial,
		Replenished,
		Damaged,
		Destroyed
	}

	[SerializeField]
	private WorldEventChoiceText destroyChoiceText;

	[SerializeField]
	private WorldEventChoiceText sprayChoiceText;

	[SerializeField]
	private WorldEventChoiceText replenishChoiceText;

	[SerializeField]
	private WorldEventChoiceText finalDestroyChoiceText;

	[SerializeField]
	private WorldEventChoiceText leaveChoiceText;

	[SerializeField]
	private ItemDefinition insecticideItem;

	[SerializeField]
	private bool useStagedChoices;

	[SerializeField]
	private int suppressLimit = 3;

	[SerializeField]
	private int destroyCost = 2;

	[SerializeField]
	private int replenishAmount = 1;

	[SerializeField]
	private int finalDestroyCost = 2;

	[SerializeField]
	private bool reduceEnemyLevelOnDestroy;

	[SerializeField]
	[TextArea]
	private string initialDescription;

	[SerializeField]
	[TextArea]
	private string weakenedDescription;

	[SerializeField]
	[TextArea]
	private string replenishedDescription;

	[SerializeField]
	[TextArea]
	private string destroyedDescription;

	public override bool KeepTitleAfterChoice => true;

	private string StateKey => $"spider.nest.{gameState?.contractState?.stage}.{gameState?.dungeonMapManager?.currentLocation?.globalProgress}.{gameState?.dungeonMapManager?.currentLocation?.draftMapIndex}";

	protected override void Init()
	{
		base.Init();
		currentDisplayInfo.description = (useStagedChoices ? GetDescription(GetChoiceState()) : (string.IsNullOrEmpty(initialDescription) ? EventDescription : initialDescription));
		SetupChoices();
	}

	private void SetupChoices()
	{
		currentDisplayInfo.ClearChoice();
		if (!useStagedChoices)
		{
			SetupLegacyChoices();
			return;
		}
		switch (GetChoiceState())
		{
		case NestChoiceState.Initial:
		{
			currentDisplayInfo.AddChoice(new WorldEventChoice
			{
				buttonText = destroyChoiceText.choiceName,
				descriptionText = GetDestroyChoiceDescription(),
				executeAbleChecker = () => HasInsecticide(destroyCost),
				executeAction = DamageNest,
				stayAfterChoose = true
			});
			WorldEventChoice worldEventChoice = new WorldEventChoice
			{
				buttonText = replenishChoiceText.choiceName,
				descriptionText = replenishChoiceText.choiceDescription,
				executeAbleChecker = () => GetInsecticide() != null && (int)GetInsecticide().counter != (int)GetInsecticide().maxCounter,
				executeAction = ReplenishPotion,
				stayAfterChoose = true
			};
			if ((int?)GetInsecticide()?.counter == (int?)GetInsecticide()?.maxCounter)
			{
				worldEventChoice.descriptionText = worldEventChoice.descriptionText + Environment.NewLine + ScriptableObjectSingleton<TextDataPreset>.instance.term_insecticide_use_full_hint;
			}
			currentDisplayInfo.AddChoice(worldEventChoice);
			break;
		}
		case NestChoiceState.Damaged:
			currentDisplayInfo.AddChoice(new WorldEventChoice
			{
				buttonText = finalDestroyChoiceText.choiceName,
				descriptionText = finalDestroyChoiceText.choiceDescription,
				executeAbleChecker = () => HasInsecticide(finalDestroyCost),
				executeAction = DestroyNestCompletely,
				stayAfterChoose = true
			});
			break;
		}
		currentDisplayInfo.showLeaveButton = true;
		SetPanelDirty();
	}

	private void SetupLegacyChoices()
	{
		if (!IsLegacyDestroyed())
		{
			currentDisplayInfo.AddChoice(new WorldEventChoice
			{
				buttonText = destroyChoiceText.choiceName,
				descriptionText = destroyChoiceText.choiceDescription,
				executeAbleChecker = () => HasInsecticide(suppressLimit),
				executeAction = LegacyDestroyNest,
				stayAfterChoose = true
			});
			if (GetSuppressCount() < suppressLimit)
			{
				currentDisplayInfo.AddChoice(new WorldEventChoice
				{
					buttonText = sprayChoiceText.choiceName,
					descriptionText = sprayChoiceText.choiceDescription,
					executeAbleChecker = () => HasInsecticide(1),
					executeAction = LegacySprayNest,
					stayAfterChoose = true
				});
			}
		}
		currentDisplayInfo.showLeaveButton = true;
		SetPanelDirty();
	}

	private void LegacySprayNest()
	{
		Item insecticide = GetInsecticide();
		if (insecticide != null && (int)insecticide.counter >= 1)
		{
			EnqueueAction(new ChangeCounterAction(-1, insecticide, this));
			SpiderContractRuleDef spiderRule = GetSpiderRule();
			int oldActivity = spiderRule?.GetNestActivityCurrent(gameState) ?? 0;
			spiderRule?.ChangeNestActivity(gameState, -1);
			int newActivity = spiderRule?.GetNestActivityCurrent(gameState) ?? oldActivity;
			EnqueueAction(new ActionAction(delegate
			{
				ShowNestActivityReduced(oldActivity, newActivity);
			}));
			SetSuppressCount(Mathf.Min(suppressLimit, GetSuppressCount() + 1));
			currentDisplayInfo.description = weakenedDescription;
			SetupChoices();
		}
	}

	private void LegacyDestroyNest()
	{
		if (!IsLegacyDestroyed() && HasInsecticide(suppressLimit))
		{
			Item insecticide = GetInsecticide();
			EnqueueAction(new ChangeCounterAction(-suppressLimit, insecticide, this));
			SpiderContractRuleDef spiderRule = GetSpiderRule();
			int oldActivity = spiderRule?.GetNestActivityCurrent(gameState) ?? 0;
			spiderRule?.ChangeNestActivity(gameState, -suppressLimit);
			int newActivity = spiderRule?.GetNestActivityCurrent(gameState) ?? oldActivity;
			EnqueueAction(new ActionAction(delegate
			{
				ShowNestActivityReduced(oldActivity, newActivity);
			}));
			if (reduceEnemyLevelOnDestroy)
			{
				spiderRule?.ReduceFutureEnemyLevel(gameState);
			}
			SetLegacyDestroyed();
			currentDisplayInfo.description = destroyedDescription;
			SetupChoices();
		}
	}

	private void ReplenishPotion()
	{
		if (GetInsecticide() != null)
		{
			GetSpiderRule()?.ReplenishPotion(gameState, replenishAmount);
			EnqueueAction(new PlayHealVFXAction(gameState.playerState.bft));
			SetChoiceState(NestChoiceState.Replenished);
			currentDisplayInfo.description = GetDescription(NestChoiceState.Replenished);
			SetupChoices();
		}
	}

	private void DamageNest()
	{
		if (GetChoiceState() == NestChoiceState.Initial && HasInsecticide(destroyCost))
		{
			Item insecticide = GetInsecticide();
			EnqueueAction(new ChangeCounterAction(-destroyCost, insecticide, this));
			SpiderContractRuleDef spiderRule = GetSpiderRule();
			int oldActivity = spiderRule?.GetNestActivityCurrent(gameState) ?? 0;
			spiderRule?.ChangeNestActivity(gameState, -GetNestHeartActivityReduction());
			int newActivity = spiderRule?.GetNestActivityCurrent(gameState) ?? oldActivity;
			EnqueueAction(new ActionAction(delegate
			{
				ShowNestActivityReduced(oldActivity, newActivity);
			}));
			NestChoiceState choiceState = (reduceEnemyLevelOnDestroy ? NestChoiceState.Damaged : NestChoiceState.Destroyed);
			SetChoiceState(choiceState);
			currentDisplayInfo.description = GetDescription(choiceState);
			SetupChoices();
		}
	}

	private void DestroyNestCompletely()
	{
		if (GetChoiceState() == NestChoiceState.Damaged && HasInsecticide(finalDestroyCost))
		{
			EnqueueAction(new ChangeCounterAction(-finalDestroyCost, GetInsecticide(), this));
			GetSpiderRule()?.ReduceFutureEnemyLevel(gameState);
			SetChoiceState(NestChoiceState.Destroyed);
			currentDisplayInfo.description = GetDescription(NestChoiceState.Destroyed);
			SetupChoices();
		}
	}

	private Item GetInsecticide()
	{
		return GetSpiderRule()?.GetPotion(gameState);
	}

	private bool HasInsecticide(int cost)
	{
		return (int?)GetInsecticide()?.counter >= cost;
	}

	private int GetNestHeartActivityReduction()
	{
		return GetSpiderRule()?.GetNestHeartActivityReduction(gameState) ?? 1;
	}

	private string GetDestroyChoiceDescription()
	{
		string text = destroyChoiceText?.choiceDescription;
		if (!string.IsNullOrEmpty(text))
		{
			return text.Replace("降低 1 点", $"降低 {GetNestHeartActivityReduction()} 点");
		}
		return text;
	}

	private void ShowNestActivityReduced(int oldActivity, int newActivity)
	{
		gameState?.UIManager?.dungeonMapPanel?.PlayNestActivityReduced(oldActivity, newActivity);
	}

	private string GetDescription(NestChoiceState choiceState)
	{
		return choiceState switch
		{
			NestChoiceState.Replenished => replenishedDescription, 
			NestChoiceState.Damaged => weakenedDescription, 
			NestChoiceState.Destroyed => destroyedDescription, 
			_ => string.IsNullOrEmpty(EventDescription) ? initialDescription : EventDescription, 
		};
	}

	private int GetSuppressCount()
	{
		return gameState.contractRuntimeStateInfo.GetInt(StateKey + ".suppress");
	}

	private void SetSuppressCount(int value)
	{
		gameState.contractRuntimeStateInfo.SetInt(StateKey + ".suppress", value);
	}

	private bool IsLegacyDestroyed()
	{
		return gameState.contractRuntimeStateInfo.GetInt(StateKey + ".destroyed") != 0;
	}

	private void SetLegacyDestroyed()
	{
		gameState.contractRuntimeStateInfo.SetInt(StateKey + ".destroyed", 1);
	}

	private NestChoiceState GetChoiceState()
	{
		if (gameState?.contractRuntimeStateInfo == null)
		{
			return NestChoiceState.Initial;
		}
		if (gameState.contractRuntimeStateInfo.GetInt(StateKey + ".destroyed") != 0)
		{
			return NestChoiceState.Destroyed;
		}
		return (NestChoiceState)Mathf.Clamp(gameState.contractRuntimeStateInfo.GetInt(StateKey + ".choiceState"), 0, 3);
	}

	private void SetChoiceState(NestChoiceState choiceState)
	{
		gameState?.contractRuntimeStateInfo?.SetInt(StateKey + ".choiceState", (int)choiceState);
	}

	private SpiderContractRuleDef GetSpiderRule()
	{
		return gameState?.contractState?.selectedContract?.contractDef?.contractRule as SpiderContractRuleDef;
	}
}
