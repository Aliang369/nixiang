using System.Collections.Generic;

public class EternalAbility : Ability
{
	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.ConsumeResolveInfo;
		yield return GameEvent.Type.ExileResolveInfo;
		yield return GameEvent.Type.ShuffleItemResolve;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		if (eve.CasterAttachObject is Item item && eve.TargetItem == item)
		{
			if (eve.type != GameEvent.Type.ShuffleItemResolve)
			{
				return item.BlocksExternalAction(eve.GetEveObj(GameEvent.EveObjType.Source));
			}
			return true;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve) && eve.resolveInfo != null)
		{
			eve.resolveInfo.reject = true;
			eve.resolveInfo.rejectSource = eve.CasterAttachObject as Item;
		}
	}
}
