using System;
using System.Collections.Generic;

public class StoryEventObserver : GlobalTriggerObserver
{
	public enum Type
	{
		SCJ_002_n = 1
	}

	public Type type;

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
		applicationState.eventManager.storyEvent.AddListener(delegate(Type type)
		{
			if (this.type == type)
			{
				onTrigger();
			}
		});
	}
}
