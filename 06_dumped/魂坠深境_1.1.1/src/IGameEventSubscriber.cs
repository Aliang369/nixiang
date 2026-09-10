using System.Collections.Generic;

public interface IGameEventSubscriber
{
	IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType();

	bool CheckTriggerGameEventAble(GameEvent eve);

	void TriggerGameEvent(GameEvent eve);
}
