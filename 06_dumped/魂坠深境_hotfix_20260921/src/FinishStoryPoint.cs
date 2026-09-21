using System;
using System.Collections.Generic;

public class FinishStoryPoint : GlobalTriggerObserver
{
	public StoryPoint storyPoint;

	public GlobalTriggerObserver fallbackObserver;

	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
		applicationState.eventManager.storyPointFinished.AddListener(OnStoryPointFinishedConfig);
		if (fallbackObserver != null)
		{
			fallbackObserver.Init(applicationState, OnFallback);
		}
		void OnFallback()
		{
			if (storyPoint.IsFinished(applicationState))
			{
				onTrigger();
			}
		}
		void OnStoryPointFinishedConfig(StoryPoint point)
		{
			if (!(point != storyPoint))
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
