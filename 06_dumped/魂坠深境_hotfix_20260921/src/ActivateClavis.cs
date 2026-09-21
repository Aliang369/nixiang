using System;
using System.Collections.Generic;

public class ActivateClavis : GlobalTriggerObserver
{
	public ClavisDef clavis;

	public GlobalTriggerObserver fallbackObserver;

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
		applicationState.eventManager.ActivateClavisEvent.AddListener(OnClavisActivate);
		if (fallbackObserver != null)
		{
			fallbackObserver.Init(applicationState, OnFallback);
		}
		void OnClavisActivate(ClavisDef cl)
		{
			if (!(cl != clavis))
			{
				onTrigger();
			}
		}
		void OnFallback()
		{
			if (applicationState.clavisManager.IsActivated(clavis))
			{
				onTrigger();
			}
		}
	}
}
