using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.DotManagers;

public class RedDotModule
{
	public List<RedDotModule> children = new List<RedDotModule>();

	public static PlayerCharacterState currentCharacter => applicationState.newGameConfigManager.currentNewGameConfig.GetCharacterState(applicationState);

	protected static ApplicationStateInfo applicationState => Bootstrap.applicationState;

	public bool CheckShouldShow()
	{
		if (!children.IsNullOrEmpty())
		{
			foreach (RedDotModule child in children)
			{
				if (child.CheckShouldShow())
				{
					return true;
				}
			}
		}
		try
		{
			return CheckImpl() == true;
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			return false;
		}
	}

	protected virtual bool? CheckImpl()
	{
		return null;
	}

	protected static bool IsCharacterUpgradeable(PlayerCharacterState playerCharacterState)
	{
		return playerCharacterState.CheckCanUpgrade();
	}

	public static bool IsCharacterUnlockable(PlayerCharacterState playerCharacterState)
	{
		return playerCharacterState.CheckCanUnlockCharacterByObsessionDebris();
	}

	public static bool TalentUnlockable(PlayerCharacterState characterState, TalentSlotCell cell)
	{
		if (!cell.IsUnlockAble(characterState))
		{
			return false;
		}
		if (!applicationState.matterManager.HasEnoughMatter(cell.UnlockCostMatters(characterState.def)))
		{
			return false;
		}
		return true;
	}

	public static bool HasAnyUnlockableTalent(PlayerCharacterState characterState)
	{
		TalentSlotCell[] talentSlotCells = characterState.def.talentTree.TalentSlotCells;
		foreach (TalentSlotCell cell in talentSlotCells)
		{
			if (TalentUnlockable(characterState, cell))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasAnyUnlockableTalent()
	{
		foreach (PlayerCharacterState characterState in applicationState.characterManager.characterStates)
		{
			if (HasAnyUnlockableTalent(characterState))
			{
				return true;
			}
		}
		return false;
	}
}
