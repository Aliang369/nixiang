using System;
using System.Collections.Generic;

public class AnyTrigger : GlobalTriggerObserver
{
	public List<GlobalTriggerObserver> triggers;

	public override List<IEffectNode> GetChildren()
	{
		return triggers?.ConvertAll((Converter<GlobalTriggerObserver, IEffectNode>)((GlobalTriggerObserver t) => t));
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
		foreach (GlobalTriggerObserver item in triggers.OrEmptyIfNull())
		{
			item.Init(applicationState, onTrigger);
		}
	}
}
