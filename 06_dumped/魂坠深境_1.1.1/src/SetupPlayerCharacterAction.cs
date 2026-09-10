using System;
using System.Collections.Generic;
using System.Linq;

public class SetupPlayerCharacterAction : ExecuteAction
{
	public PlayerCharacterState character;

	public BootStrapLoadData startData;

	public SetupPlayerCharacterAction(PlayerCharacterState character, BootStrapLoadData startData)
	{
		this.character = character;
		this.startData = startData;
	}

	protected override void Update()
	{
		NewGameConfigData.EncounterConfig encounterSetupConfig = startData.newGameConfigData.encounterSetupConfig;
		if (startData.newGameConfigData.IsLimitedDeploymentRun)
		{
			character = startData.newGameConfigData.CreateCharacterStateForRun(gameState.applicationState);
		}
		if (encounterSetupConfig?.trialCharacterDef != null)
		{
			character = PlayerCharacterState.CreateTrialPreview(encounterSetupConfig.trialCharacterDef, encounterSetupConfig.trialMastery);
		}
		else if ((object)encounterSetupConfig?.importedTestSetup?.Character != null)
		{
			character = new PlayerCharacterState(encounterSetupConfig.importedTestSetup.Character);
		}
		if (character == null)
		{
			character = gameState.applicationState.characterManager.GetOrCreateCharacterState(startData.newGameConfigData.currentCharacter);
		}
		if (character == null)
		{
			throw new NullReferenceException("SetupPlayerCharacterAction failed to resolve a valid player character state.");
		}
		if (encounterSetupConfig?.trialCharacterDef != null && encounterSetupConfig.trialSkills.Count > 0)
		{
			SetupTrialSkillPreset(character, encounterSetupConfig);
		}
		if (!startData.newGameConfigData.IsLimitedDeploymentRun && encounterSetupConfig?.trialCharacterDef == null && encounterSetupConfig?.importedTestSetup == null)
		{
			gameState.applicationState.globalSkillManager.ValidateAllSkills();
		}
		CharacterGameInitConfig characterGameInitConfig = character.GenerateCharacterConfigInit(gameState.applicationState);
		if (encounterSetupConfig?.trialCharacterDef != null && encounterSetupConfig.trialInfluenceDefs.Count > 0)
		{
			characterGameInitConfig.initInfluence = encounterSetupConfig.trialInfluenceDefs.Select((InfluenceDef ifd) => new Influence(ifd)).ToList();
		}
		if (encounterSetupConfig?.trialCharacterDef != null && encounterSetupConfig.trialSkills.Count > characterGameInitConfig.activateSkillSlotCount)
		{
			characterGameInitConfig.activateSkillSlotCount = encounterSetupConfig.trialSkills.Count;
		}
		List<Item> list = (encounterSetupConfig?.replaceCharacterDeck?.Select((GameRecord.ItemRecord ir) => ir.DisplayItem) ?? characterGameInitConfig.initDeck).ToList();
		list.AddRange(gameState.contractState?.selectedContract?.contractDef?.GenerateInitialDeckItems() ?? Enumerable.Empty<Item>());
		if (encounterSetupConfig?.importedTestSetup != null)
		{
			list = encounterSetupConfig.importedTestSetup.DeckRecords.Select((GameRecord.ItemRecord ir) => ir.DisplayItem).ToList();
			foreach (GameSkill skill in encounterSetupConfig.importedTestSetup.Skills)
			{
				EnqueueAction(new ChangeSkillAction(skill, ChangeSkillAction.Type.AddOutSlot));
			}
		}
		gameState.playerState.Setup(characterGameInitConfig, character);
		gameState.playerState.SetupSkillPreset(character.skillPreset);
		gameState.trigramsManager.SetupCharacter(characterGameInitConfig, startData.IsNewGame);
		foreach (Item item in list)
		{
			if (!gameState.IsCharacterTrialGame)
			{
				gameState.applicationState.discoverManager.Discover(item.GetDef());
				gameState.recordManager.RecordDiscoveredObjectForUpload(item.GetDef());
			}
			EnqueueAction(new SpawnItemAction(item, gameState.playerState.DeckContainer));
		}
		foreach (Influence item2 in characterGameInitConfig.initInfluence)
		{
			gameState.actionManager.EnqueueAction(new AddInfluenceAction(item2, gameState.playerState.bft));
		}
		EnqueueAction(new ChangeSlotCountAction(gameState.playerState.TableContainer, characterGameInitConfig.itemSlotCount, temp: false));
		EnqueueAction(new SetCoinAction(characterGameInitConfig.initCoin));
		EnqueueAction(new SetCrystalAction(characterGameInitConfig.initCrystal));
		EnqueueAction(new SetCharacterSkillLimitAction(characterGameInitConfig.activateSkillSlotCount));
		if (encounterSetupConfig?.trialCharacterDef != null && encounterSetupConfig.trialSkills.Count > 0)
		{
			EnqueueActivateTrialSkills(encounterSetupConfig.trialSkills.Count);
		}
		EnqueueAction(delegate
		{
			gameState.UIManager.playerBftUIDisplay.Setup(gameState.playerState.bft);
		});
		bool characterInnateSkillInitialActivateUnlocked = character.CharacterInnateSkillInitialActivateUnlocked;
		foreach (GameSkillSlot skillSlot in gameState.playerState.prepareSkillsPreset.gameSkillSlots)
		{
			if (skillSlot.gameSkill == null || (encounterSetupConfig?.trialCharacterDef != null && !skillSlot.characterInnateSkillSlot) || !(skillSlot.characterInnateSkillSlot ? characterInnateSkillInitialActivateUnlocked : skillSlot.gameSkill.SkillArchetype.GetPropFlag(SkillDef.SkillPropertyType.InitialActivate)))
			{
				continue;
			}
			EnqueueAction(new ActionAction(delegate
			{
				foreach (GameSkillSlot item3 in gameState.playerState.activeSkills_inSlot)
				{
					if (item3.gameSkill == null)
					{
						EnqueueAction(new ActivateSkillAction(skillSlot.gameSkill, item3));
						break;
					}
				}
			}));
		}
	}

	private void SetupTrialSkillPreset(PlayerCharacterState trialCharacter, NewGameConfigData.EncounterConfig encounterConfig)
	{
		for (int i = 0; i < encounterConfig.trialSkills.Count; i++)
		{
			ProfileSkill profileSkill = new ProfileSkill(encounterConfig.trialSkills[i]);
			if (encounterConfig.trialSkillsMaxQuality)
			{
				profileSkill.SetArchetypeIndex(QualityLevel.Epic);
			}
			trialCharacter.skillPreset.configSkillSlots[i].SetSkill(profileSkill);
		}
	}

	private void EnqueueActivateTrialSkills(int trialSkillCount)
	{
		EnqueueAction(new ActionAction(delegate
		{
			foreach (GameSkill item in (from slot in gameState.playerState.prepareSkillsPreset.gameSkillSlots.Where((GameSkillSlot slot) => !slot.characterInnateSkillSlot && slot.gameSkill != null).Take(trialSkillCount)
				select slot.gameSkill).ToList())
			{
				GameSkillSlot skillSlot = gameState.playerState.activeSkills_inSlot.First((GameSkillSlot slot) => slot.gameSkill == null);
				EnqueueAction(new ActivateSkillAction(item, skillSlot));
			}
		}));
	}
}
