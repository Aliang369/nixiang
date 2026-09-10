using System;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class EnemyBftUITextDisplay : BftUIDisplay.DisplayModule
{
	public TextMeshProUGUI nameText;

	public GameObject enemyAbilityArea;

	public TextMeshProUGUI abilityText;

	public Color abilityNotActiveColor;

	[NonSerialized]
	private Color abilityTextActiveColor;

	[NonSerialized]
	protected TextLinkObjectDisplayer abilityTextLinkObjectDisplayer;

	public override void Init()
	{
		abilityTextActiveColor = abilityText.color;
		abilityText.verticalAlignment = VerticalAlignmentOptions.Middle;
		abilityTextLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(UIDisplayModule.applicationState, abilityText, abilityText.rectTransform);
		UIDisplayModule.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.EnemyEnterPhase2, _OnEnterPhase2);
	}

	public override void Dispose()
	{
		UIDisplayModule.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.EnemyEnterPhase2, _OnEnterPhase2);
	}

	public override void Setup()
	{
		UpdateDisplay();
	}

	public override void UpdateDisplay()
	{
		CharacterAI characterAI = base.bftDisplay.bft.characterAI;
		if (nameText != null)
		{
			nameText.text = characterAI.EnemyName;
			TextMeshProUGUI textMeshProUGUI = nameText;
			textMeshProUGUI.text = textMeshProUGUI.text + " " + ScriptableObjectSingleton<TextDataPreset>.instance.term_level_arg0.FormatWith(((int)characterAI.enemyProgressLevel).ToString());
			if (characterAI.enemyEncounterType == EnemyEncounterType.Elite)
			{
				TextMeshProUGUI textMeshProUGUI2 = nameText;
				textMeshProUGUI2.text = textMeshProUGUI2.text + " " + ScriptableObjectSingleton<TextDataPreset>.instance.term_enemyType_elite;
			}
		}
		if (!(abilityText != null))
		{
			return;
		}
		if (characterAI.SecondPhaseAbilityEnable)
		{
			enemyAbilityArea.SetActive(value: true);
			abilityText.text = TextDataPreset.FormatEnemySecondPhaseAbilityDescription(characterAI, UIDisplayModule.gameState, out var generator);
			abilityTextLinkObjectDisplayer.SetGenerator(generator);
			if (!characterAI.inPhase2)
			{
				abilityText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_enemy_ability_acitve_in_phase_2 + abilityText.text;
			}
			abilityText.color = (characterAI.inPhase2 ? abilityTextActiveColor : abilityNotActiveColor);
		}
		else
		{
			enemyAbilityArea.SetActive(value: false);
		}
	}

	private void _OnEnterPhase2(GameEvent.Type type, GameEvent eve)
	{
		base.bftDisplay.SetDirty();
	}
}
