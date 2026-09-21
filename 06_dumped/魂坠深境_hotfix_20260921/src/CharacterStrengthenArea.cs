using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class CharacterStrengthenArea : CellUI<PlayerCharacterState>, IDisplayModulesController
{
	public TextMeshProUGUI characterHealthText;

	public TextMeshProUGUI characterCrystalText;

	public TextMeshProUGUI characterCoinText;

	public TextMeshProUGUI characterWisdomText;

	public TextMeshProUGUI drawCount;

	public TextMeshProUGUI slotCount;

	[SerializeField]
	private TextMeshProUGUI titleText;

	public GameObject infoArea;

	public bool isCurrent;

	public bool showText;

	private PlayerCharacterState characterState => base.setupData;

	protected override void Init()
	{
		InputModule module = GetModule<InputModule>();
		module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, (Action<InputModuleBase>)delegate(InputModuleBase inputModuleBase)
		{
			if (!(infoArea == null) && inputModuleBase.pointerClicked)
			{
				infoArea.gameObject.SetActive(value: true);
			}
		});
	}

	protected override void UpdateDisplay()
	{
		UpdateTitle();
		if (isCurrent)
		{
			UpdateCurrent();
		}
		else if (characterState.isPreview)
		{
			UpdatePreviewBase();
		}
		else
		{
			UpdateConfig();
		}
		void UpdateConfig()
		{
			CharacterGameInitConfig characterGameInitConfig = characterState.GenerateCharacterConfigInit(CellUI<PlayerCharacterState>.applicationState);
			if (showText)
			{
				characterHealthText.text = $"{CellUI<PlayerCharacterState>.textData.term_health}:{characterGameInitConfig.initHealth}";
				characterCrystalText.text = $"{CellUI<PlayerCharacterState>.textData.term_crystal}:{characterGameInitConfig.initCrystal}";
				characterCoinText.text = $"{CellUI<PlayerCharacterState>.textData.term_coin}:{characterGameInitConfig.initCoin}";
				characterWisdomText.text = $"{CellUI<PlayerCharacterState>.textData.term_wisdom}:{characterGameInitConfig.initWillPower}";
				drawCount.text = $"{CellUI<PlayerCharacterState>.textData.term_draw}:{characterGameInitConfig.turnDrawCount}";
				slotCount.text = $"{CellUI<PlayerCharacterState>.textData.term_slot}:{characterGameInitConfig.itemSlotCount}";
			}
			else
			{
				characterHealthText.text = characterGameInitConfig.initHealth.ToString();
				characterCrystalText.text = characterGameInitConfig.initCrystal.ToString();
				characterCoinText.text = characterGameInitConfig.initCoin.ToString();
				characterWisdomText.text = characterGameInitConfig.initWillPower.ToString();
				drawCount.text = characterGameInitConfig.turnDrawCount.ToString();
				slotCount.text = characterGameInitConfig.itemSlotCount.ToString();
			}
		}
		void UpdateCurrent()
		{
			int persistentFixedSlotCount = CellUI<PlayerCharacterState>.gameState.playerState.TableContainer.persistentFixedSlotCount;
			int finalFixedSlotCount = CellUI<PlayerCharacterState>.gameState.playerState.TableContainer.GetFinalFixedSlotCount();
			string text = finalFixedSlotCount.ToString();
			if (persistentFixedSlotCount != finalFixedSlotCount)
			{
				text = text.ColorText(Color.yellow);
			}
			characterCrystalText.text = $"{CellUI<PlayerCharacterState>.gameState.playerState.crystalCount}";
			characterCoinText.text = $"{CellUI<PlayerCharacterState>.gameState.playerState.coinCount}";
			drawCount.text = $"{CellUI<PlayerCharacterState>.gameState.playerState.bft.turnDrawItemCount}";
			slotCount.text = text;
			BattleFieldTransform bft = CellUI<PlayerCharacterState>.gameState.playerState.bft;
			int num = bft.health;
			int num2 = bft.maxHealth;
			characterHealthText.text = $"{num}/{num2}";
			characterWisdomText.text = $"{CellUI<PlayerCharacterState>.gameState.playerState.MaxWillPower - CellUI<PlayerCharacterState>.gameState.playerState.RemainWillPower}/{CellUI<PlayerCharacterState>.gameState.playerState.MaxWillPower}";
		}
		void UpdatePreviewBase()
		{
			if (showText)
			{
				characterHealthText.text = $"{CellUI<PlayerCharacterState>.textData.term_health}:{characterState.def.initHealth}";
				characterCrystalText.text = $"{CellUI<PlayerCharacterState>.textData.term_crystal}:{characterState.def.initCrystal}";
				characterCoinText.text = $"{CellUI<PlayerCharacterState>.textData.term_coin}:{characterState.def.initCoin}";
				characterWisdomText.text = $"{CellUI<PlayerCharacterState>.textData.term_wisdom}:{characterState.def.initWillPower}";
				drawCount.text = $"{CellUI<PlayerCharacterState>.textData.term_draw}:{characterState.def.initDrawCount}";
				slotCount.text = $"{CellUI<PlayerCharacterState>.textData.term_slot}:{characterState.def.itemSlotCount}";
			}
			else
			{
				characterHealthText.text = characterState.def.initHealth.ToString();
				characterCrystalText.text = characterState.def.initCrystal.ToString();
				characterCoinText.text = characterState.def.initCoin.ToString();
				characterWisdomText.text = characterState.def.initWillPower.ToString();
				drawCount.text = characterState.def.initDrawCount.ToString();
				slotCount.text = characterState.def.itemSlotCount.ToString();
			}
		}
	}

	private void UpdateTitle()
	{
		if (!(titleText == null))
		{
			TextMeshProUGUI textMeshProUGUI = titleText;
			PlayerCharacterState playerCharacterState = characterState;
			textMeshProUGUI.text = ((playerCharacterState != null && playerCharacterState.isPreview) ? CellUI<PlayerCharacterState>.textData.term_character_initial_stats_title : CellUI<PlayerCharacterState>.textData.term_character_stats_title);
		}
	}
}
