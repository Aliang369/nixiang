using System;
using System.Collections.Generic;
using UnityEngine;

public class ElementTurbulence : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText fireChoiceText;

	[SerializeField]
	[LocalizeText("事件文本")]
	private string fireExtraText;

	[SerializeField]
	private InfluenceDef fireBless;

	[SerializeField]
	private int fireEnchantStack;

	[SerializeField]
	private WorldEventChoiceText iceChoiceText;

	[SerializeField]
	[LocalizeText("事件文本")]
	private string iceExtraText;

	[SerializeField]
	private InfluenceDef iceBless;

	[SerializeField]
	private WorldEventChoiceText thunderChoiceText;

	[SerializeField]
	[LocalizeText("事件文本")]
	private string thunderExtraText;

	[SerializeField]
	private InfluenceDef thunderBless;

	[SerializeField]
	private float healthRate = 0.3f;

	[NonSerialized]
	private WorldEventChoice choice1;

	[NonSerialized]
	private WorldEventChoice choice2;

	[NonSerialized]
	private WorldEventChoice choice3;

	protected override void Init()
	{
		base.Init();
		choice1 = new WorldEventChoice();
		Influence influence = new Influence(fireBless);
		string descriptionText = string.Format(fireChoiceText.choiceDescription, influence.def.InfluenceName);
		choice1.descriptionText = descriptionText;
		choice1.buttonText = fireChoiceText.choiceName;
		choice1.AddDisplayInfluence(influence);
		choice1.executeAction = ExecuteAction1;
		currentDisplayInfo.AddChoice(choice1);
		choice2 = new WorldEventChoice();
		Influence influence2 = new Influence(iceBless);
		string descriptionText2 = string.Format(iceChoiceText.choiceDescription, influence2.def.InfluenceName);
		choice2.descriptionText = descriptionText2;
		choice2.buttonText = iceChoiceText.choiceName;
		choice2.AddDisplayInfluence(influence2);
		choice2.executeAction = ExecuteAction2;
		currentDisplayInfo.AddChoice(choice2);
		choice3 = new WorldEventChoice();
		Influence influence3 = new Influence(thunderBless);
		string descriptionText3 = string.Format(thunderChoiceText.choiceDescription, influence3.def.InfluenceName);
		choice3.descriptionText = descriptionText3;
		choice3.buttonText = thunderChoiceText.choiceName;
		choice3.AddDisplayInfluence(influence3);
		choice3.executeAction = ExecuteAction3;
		currentDisplayInfo.AddChoice(choice3);
	}

	private void ExecuteAction1()
	{
		RefreshAfterChoose(fireChoiceText.chosenDescription);
		EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			influences = new List<Influence> { choice1.displayInfluences[0] }
		}));
	}

	private void ExecuteAction2()
	{
		RefreshAfterChoose(iceChoiceText.chosenDescription);
		EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			influences = new List<Influence> { choice2.displayInfluences[0] }
		}));
	}

	private void ExecuteAction3()
	{
		RefreshAfterChoose(thunderChoiceText.chosenDescription);
		EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			influences = new List<Influence> { choice3.displayInfluences[0] }
		}));
	}
}
