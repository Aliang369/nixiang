using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OnUnlockFullTalentTree : GlobalTriggerObserver
{
	public CharacterClassType targetCharacterClass;

	public int finishedCharacterCount = 1;

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public override List<IEffectNode> GetChildren()
	{
		return null;
	}

	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
		applicationState.eventManager.unlockTalentEvent.AddListener(OnUnlockTalent);
		CheckAndTrigger();
		void CheckAndTrigger()
		{
			if (((targetCharacterClass == CharacterClassType.None) ? applicationState.characterManager.characterStates : applicationState.characterManager.GetClassAllCharacters(applicationState.characterManager.GetCharacterClassState(targetCharacterClass).classDef)).Count(IsTalentTreeFullyUnlocked) >= Mathf.Max(1, finishedCharacterCount))
			{
				onTrigger();
			}
		}
		void OnUnlockTalent(TalentDef talent)
		{
			CheckAndTrigger();
		}
	}

	private static bool IsTalentTreeFullyUnlocked(PlayerCharacterState character)
	{
		List<TalentDef> list = character?.def?.talentTree?.TalentSlotCells?.Where((TalentSlotCell cell) => cell?.talent != null).Select((TalentSlotCell cell) => cell.talent).Distinct()
			.ToList();
		if (list != null && list.Count > 0)
		{
			return list.TrueForAll(character.IsTalentUnlocked);
		}
		return false;
	}
}
