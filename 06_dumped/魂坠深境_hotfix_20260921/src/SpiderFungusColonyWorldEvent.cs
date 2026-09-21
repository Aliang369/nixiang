using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class SpiderFungusColonyWorldEvent : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText glowingFungusChoiceText;

	[SerializeField]
	private WorldEventChoiceText edibleFungusChoiceText;

	[SerializeField]
	private WorldEventChoiceText leaveChoiceText;

	[SerializeField]
	private InfluenceDef nightVisionInfluence;

	[SerializeField]
	private List<ItemDefinition> sporeCurseItems;

	[SerializeField]
	private int healHealth;

	[NonSerialized]
	private bool glowingFungusChosen;

	[NonSerialized]
	private bool edibleFungusChosen;

	protected override void Init()
	{
		base.Init();
		glowingFungusChosen = false;
		edibleFungusChosen = false;
		SetupChoices();
	}

	private void SetupChoices()
	{
		currentDisplayInfo.ClearChoice();
		WorldEventChoice worldEventChoice = new WorldEventChoice
		{
			buttonText = glowingFungusChoiceText.choiceName,
			descriptionText = glowingFungusChoiceText.choiceDescription,
			executeAbleChecker = () => !glowingFungusChosen,
			executeAction = ChooseGlowingFungus,
			stayAfterChoose = true
		};
		if (nightVisionInfluence != null)
		{
			worldEventChoice.AddDisplayInfluence(new Influence(nightVisionInfluence));
		}
		currentDisplayInfo.AddChoice(worldEventChoice);
		currentDisplayInfo.AddChoice(new WorldEventChoice
		{
			buttonText = edibleFungusChoiceText.choiceName,
			descriptionText = FormatHealText(edibleFungusChoiceText.choiceDescription),
			executeAbleChecker = () => !edibleFungusChosen,
			executeAction = ChooseEdibleFungus,
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

	private void ChooseGlowingFungus()
	{
		if (!glowingFungusChosen)
		{
			glowingFungusChosen = true;
			if (nightVisionInfluence != null)
			{
				EnqueueAction(new AddInfluenceAction(new Influence(nightVisionInfluence), gameState.playerState.bft));
			}
			AddRandomSporeCurse();
			currentDisplayInfo.description = glowingFungusChoiceText.chosenDescription;
			SetupChoices();
		}
	}

	private void ChooseEdibleFungus()
	{
		if (!edibleFungusChosen)
		{
			edibleFungusChosen = true;
			EnqueueAction(new HealAction(this, gameState.playerState.bft, healHealth));
			AddRandomSporeCurse();
			currentDisplayInfo.description = FormatHealText(edibleFungusChoiceText.chosenDescription);
			SetupChoices();
		}
	}

	private void AddRandomSporeCurse()
	{
		ItemDefinition itemDefinition = (from item in sporeCurseItems.OrEmptyIfNull()
			where item != null
			select item).ToList().SelectRandom(gameState.randomManager.NextDouble);
		if (itemDefinition != null)
		{
			EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
			{
				items = new List<Item>
				{
					new Item(itemDefinition)
				}
			});
		}
	}

	private string FormatHealText(string raw)
	{
		if (string.IsNullOrEmpty(raw))
		{
			return raw;
		}
		try
		{
			return raw.FormatWith(healHealth);
		}
		catch (FormatException)
		{
			return raw;
		}
	}
}
