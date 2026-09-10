using System.Collections.Generic;

public class SpiderGuardSwarmAbility : Ability
{
	public int tempHealthPerCounter = 1;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.EnemyTurnStart;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve))
		{
			Item casterItem = eve.CasterItem;
			if (casterItem != null && casterItem.OwnerBft?.isEnemy == true)
			{
				ItemContainer owner = eve.CasterItem.owner;
				if (owner != null && owner.type == ItemContainer.ContainerType.Table && (int)eve.CasterItem.counter > 0)
				{
					return tempHealthPerCounter > 0;
				}
			}
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			int delta = (int)eve.CasterItem.counter * tempHealthPerCounter;
			eve.gameState.actionManager.EnqueueAction(new ChangeTempHealthAction(delta, eve.CasterItem.OwnerBft));
		}
	}
}
