using System.Collections.Generic;

public class SpiderChronicDeathAbility : Ability
{
	public int drawDelta = -1;

	public int slotDelta = -1;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.TurnDrawResolve;
		yield return GameEvent.Type.FixedSlotCountResolve;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve) && eve.resolveInfo != null)
		{
			BattleFieldTransform targetBft = eve.TargetBft;
			if (targetBft != null && targetBft.isPlayer)
			{
				Item casterItem = eve.CasterItem;
				if (casterItem != null && casterItem.OwnerBft?.isPlayer == true)
				{
					ItemContainer owner = eve.CasterItem.owner;
					if (owner != null && owner.type == ItemContainer.ContainerType.Table)
					{
						if (eve.type != GameEvent.Type.TurnDrawResolve)
						{
							return eve.type == GameEvent.Type.FixedSlotCountResolve;
						}
						return true;
					}
				}
			}
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			if (eve.type == GameEvent.Type.TurnDrawResolve)
			{
				eve.resolveInfo.deltaValue += drawDelta;
			}
			else if (eve.type == GameEvent.Type.FixedSlotCountResolve)
			{
				eve.resolveInfo.deltaValue += slotDelta;
			}
		}
	}
}
