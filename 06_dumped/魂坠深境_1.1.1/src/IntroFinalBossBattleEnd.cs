using System;
using System.Collections.Generic;

public class IntroFinalBossBattleEnd : GlobalTriggerObserver
{
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
		applicationState.eventManager.IntroFinalBossBattleEnd.AddListener(delegate
		{
			onTrigger();
		});
	}
}
