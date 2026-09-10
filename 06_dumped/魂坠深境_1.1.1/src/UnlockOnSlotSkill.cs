using System;
using System.Collections.Generic;

public class UnlockOnSlotSkill : GlobalTriggerObserver
{
	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
		applicationState.eventManager.addSkillGemSlotEvent.AddListener(OnAddSkillGemSlotEvent);
		void OnAddSkillGemSlotEvent(ProfileSkill info)
		{
			onTrigger();
			applicationState.eventManager.addSkillGemSlotEvent.RemoveListener(OnAddSkillGemSlotEvent);
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public override List<IEffectNode> GetChildren()
	{
		return null;
	}
}
