using System.Linq;

public static class LimitedCharacterDeploymentService
{
	public readonly struct LocalOneWayProgress(bool consumed, bool viewed)
	{
		public readonly bool consumed = consumed;

		public readonly bool viewed = viewed;
	}

	private const string FirekeeperCharacterId = "2810595260b066f419ca9777065d8509";

	private const string EnhancedFireballMasteryId = "9b400fd6523cc404d940da80dac8e328";

	private const string MeteorWandSkillId = "05c233ac2ea6d544799673bbbb26dae1";

	public static bool IsTargetCharacter(PlayerCharacterDef character)
	{
		if (character != null)
		{
			return character.id == "2810595260b066f419ca9777065d8509";
		}
		return false;
	}

	public static CharacterMastery GetFixedMastery(PlayerCharacterDef character)
	{
		return character?.characterMasteries?.FirstOrDefault((CharacterMastery mastery) => mastery != null && mastery.id == "9b400fd6523cc404d940da80dac8e328");
	}

	public static SkillDef GetFixedSkill(ApplicationStateInfo applicationState)
	{
		return applicationState?.assetCatalog?.SkillDefCatalog?.GetAsset("05c233ac2ea6d544799673bbbb26dae1");
	}

	public static bool IsAvailable(ApplicationStateInfo applicationState, PlayerCharacterState characterState)
	{
		if (applicationState?.globalVariableManager == null || characterState?.def == null)
		{
			return false;
		}
		if (!IsTargetCharacter(characterState.def) || !characterState.locked)
		{
			return false;
		}
		if (!applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressStoreHousePanel))
		{
			return false;
		}
		if (applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.LimitedDeploymentFirekeeperConsumed))
		{
			return false;
		}
		if ((applicationState.matterManager?.GetMatterInfo(characterState.def.obsessionDebris)?.count).GetValueOrDefault() < ScriptableObjectSingleton<FortressDataManager>.instance.obsessionDebrisUnlockCount && GetFixedMastery(characterState.def) != null)
		{
			SkillDef fixedSkill = GetFixedSkill(applicationState);
			if ((object)fixedSkill == null)
			{
				return false;
			}
			return fixedSkill.skillArchetypes?.Any((SkillDef.SkillArchetype archetype) => archetype.qualityLevel == QualityLevel.Rare) == true;
		}
		return false;
	}

	public static bool IsAllowedContract(NewGameConfigData config)
	{
		ContractDef.ContractType? contractType = config?.contract?.contractDef?.contractType;
		if (contractType.HasValue)
		{
			ContractDef.ContractType valueOrDefault = contractType.GetValueOrDefault();
			if ((uint)valueOrDefault <= 3u)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanSelect(ApplicationStateInfo applicationState, PlayerCharacterState characterState, NewGameConfigData config)
	{
		if (IsAvailable(applicationState, characterState))
		{
			return IsAllowedContract(config);
		}
		return false;
	}

	public static PlayerCharacterState CreateTemporaryCharacter(ApplicationStateInfo applicationState, PlayerCharacterDef character)
	{
		CharacterMastery fixedMastery = GetFixedMastery(character);
		SkillDef fixedSkill = GetFixedSkill(applicationState);
		if (!(character == null) && !(fixedMastery == null) && !(fixedSkill == null))
		{
			return PlayerCharacterState.CreateLimitedDeploymentPreview(character, fixedMastery, fixedSkill);
		}
		return null;
	}

	public static bool ShouldShowNewHint(ApplicationStateInfo applicationState, PlayerCharacterState characterState)
	{
		if (IsAvailable(applicationState, characterState))
		{
			return !applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.LimitedDeploymentFirekeeperViewed);
		}
		return false;
	}

	public static void MarkViewed(ApplicationStateInfo applicationState, PlayerCharacterState characterState)
	{
		if (ShouldShowNewHint(applicationState, characterState))
		{
			applicationState.globalVariableManager.SetVariableValue(ApplicationGlobalVariableManager.LimitedDeploymentFirekeeperViewed, 1);
			applicationState.savingManager?.SetProfileDirtyToSave();
		}
	}

	public static int GetSelectorSortRank(ApplicationStateInfo applicationState, PlayerCharacterState characterState)
	{
		if (characterState == null)
		{
			return int.MaxValue;
		}
		if (!characterState.locked)
		{
			return 0;
		}
		if (IsAvailable(applicationState, characterState))
		{
			return 1;
		}
		if (characterState.obsessionDebrisCount <= 0)
		{
			return 3;
		}
		return 2;
	}

	public static void MarkConsumed(ApplicationStateInfo applicationState)
	{
		if (applicationState?.globalVariableManager != null)
		{
			applicationState.globalVariableManager.SetVariableValue(ApplicationGlobalVariableManager.LimitedDeploymentFirekeeperConsumed, 1);
			applicationState.savingManager?.SetProfileDirtyToSave();
		}
	}

	public static LocalOneWayProgress CaptureLocalOneWayProgress(ApplicationStateInfo applicationState)
	{
		ApplicationGlobalVariableManager applicationGlobalVariableManager = applicationState?.globalVariableManager;
		return new LocalOneWayProgress(applicationGlobalVariableManager?.GetVariableFlag(ApplicationGlobalVariableManager.LimitedDeploymentFirekeeperConsumed) ?? false, applicationGlobalVariableManager?.GetVariableFlag(ApplicationGlobalVariableManager.LimitedDeploymentFirekeeperViewed) ?? false);
	}

	public static void RestoreLocalOneWayProgress(ApplicationStateInfo applicationState, LocalOneWayProgress localProgress)
	{
		ApplicationGlobalVariableManager applicationGlobalVariableManager = applicationState?.globalVariableManager;
		if (applicationGlobalVariableManager != null)
		{
			if (localProgress.consumed)
			{
				applicationGlobalVariableManager.SetVariableValue(ApplicationGlobalVariableManager.LimitedDeploymentFirekeeperConsumed, 1);
			}
			if (localProgress.viewed)
			{
				applicationGlobalVariableManager.SetVariableValue(ApplicationGlobalVariableManager.LimitedDeploymentFirekeeperViewed, 1);
			}
		}
	}
}
