using System;
using System.Collections.Generic;

public class EnterGameConfig : GlobalTriggerObserver
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
		applicationState.eventManager.enterFortressGameConfigPanel.AddListener(OnEnterGameConfig);
		void OnEnterGameConfig(NewGameConfigData _)
		{
			onTrigger();
		}
	}
}
