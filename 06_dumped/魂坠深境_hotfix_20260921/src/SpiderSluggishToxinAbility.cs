using System.Collections.Generic;

public class SpiderSluggishToxinAbility : Ability
{
	public int drawDelta = -1;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.TurnDrawResolve;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve) && eve.type == GameEvent.Type.TurnDrawResolve && eve.resolveInfo != null)
		{
			BattleFieldTransform targetBft = eve.TargetBft;
			if (targetBft != null && targetBft.isPlayer)
			{
				Item casterItem = eve.CasterItem;
				if (casterItem != null && casterItem.OwnerBft?.isPlayer == true)
				{
					ItemContainer owner = eve.CasterItem.owner;
					if (owner == null || owner.type != ItemContainer.ContainerType.DrawPile)
					{
						ItemContainer owner2 = eve.CasterItem.owner;
						if (owner2 == null)
						{
							return false;
						}
						return owner2.type == ItemContainer.ContainerType.Table;
					}
					return true;
				}
			}
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			eve.resolveInfo.deltaValue += drawDelta;
		}
	}
}
