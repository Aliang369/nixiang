using System;
using System.Collections.Generic;

public class GameEventObserver : GlobalTriggerObserver
{
	public CheckNode checkNode;

	public GameEvent.Type type;

	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
		applicationState.eventManager.gameEvent.AddListener(type, delegate(GameEvent.Type id, GameEvent ge)
		{
			TriggerGameEvent(ge, onTrigger);
		});
	}

	public void TriggerGameEvent(GameEvent eve, Action onTrigger)
	{
		if (eve.type == type && eve.type == type && (checkNode == null || checkNode.Check(eve) == true))
		{
			onTrigger();
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { checkNode };
	}
}
