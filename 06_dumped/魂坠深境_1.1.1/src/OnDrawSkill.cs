using System;
using System.Collections.Generic;

public class OnDrawSkill : GlobalTriggerObserver
{
	public bool filterByQuality;

	public QualityLevel qualityLevel;

	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
		applicationState.eventManager.drawSkillEvent.AddListener(DrawSkill);
		void DrawSkill(ProfileSkill sk)
		{
			if (!filterByQuality || sk.QualityLevel == qualityLevel)
			{
				onTrigger();
			}
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
