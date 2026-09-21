using System;
using System.Collections.Generic;

public class OnUnlockTalent : GlobalTriggerObserver
{
	public TalentDef.TalentType? filterTalentType;

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
		void OnUnlockTalent(TalentDef talent)
		{
			if (!filterTalentType.HasValue || filterTalentType == talent.talentType)
			{
				onTrigger();
			}
		}
	}
}
