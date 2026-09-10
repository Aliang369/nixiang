using System;
using System.Collections.Generic;

public class UnlockOnReachGlobalMaxLevel : GlobalTriggerObserver
{
	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
		applicationState.eventManager.changeExpResultExecuteEvent.AddListener(OnChangeExp);
		void OnChangeExp(LevelLine.ChangeExpResult result)
		{
			LevelLine levelLineOfDef = result.levelLineDef.GetLevelLineOfDef(applicationState);
			if (levelLineOfDef.IsGlobalLevelLine(applicationState) && result.levelUps.Count > 0 && levelLineOfDef.IsMaxLevel)
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
