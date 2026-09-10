using System;
using System.Collections.Generic;

public class OnBreakthroughClassSkill : GlobalTriggerObserver
{
	public CharacterClassType targetCharacterClass;

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
		applicationState.eventManager.breakThroughSkillEvent.AddListener(OnBreakthroughSkill);
		void OnBreakthroughSkill(ProfileSkill skill)
		{
			if (targetCharacterClass == CharacterClassType.None || skill?.def?.attachCharacterClass?.mClass == targetCharacterClass)
			{
				onTrigger();
			}
		}
	}
}
