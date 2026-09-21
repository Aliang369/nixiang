using System;
using System.Collections.Generic;

public class UnlockOnReachCharacterMaxLevel : GlobalTriggerObserver
{
	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
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
