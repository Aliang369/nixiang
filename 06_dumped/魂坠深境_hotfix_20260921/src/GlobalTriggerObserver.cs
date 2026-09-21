using System;
using System.Collections.Generic;

[Serializable]
public abstract class GlobalTriggerObserver : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public GlobalTriggerObserverCondition condition;

	public abstract NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve);

	public abstract List<IEffectNode> GetChildren();

	public void Init(ApplicationStateInfo applicationState, Action onTrigger)
	{
		Action onTrigger2 = delegate
		{
			if (condition == null || condition.Check(applicationState))
			{
				onTrigger();
			}
		};
		InitImpl(applicationState, onTrigger2);
	}

	protected abstract void InitImpl(ApplicationStateInfo applicationState, Action onTrigger);
}
