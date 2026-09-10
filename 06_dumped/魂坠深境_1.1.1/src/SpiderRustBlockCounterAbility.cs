using System.Collections.Generic;

public class SpiderRustBlockCounterAbility : Ability
{
	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.CounterChangeResolveInfo;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve) && eve.resolveInfo != null && eve.deltaValue > 0 && eve.CasterEnchant?.attachItem != null && eve.TargetItem == eve.CasterEnchant.attachItem)
		{
			return (int)eve.CasterEnchant.value > 0;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			_ = eve.deltaValue;
			eve.resolveInfo.reject = true;
			eve.resolveInfo.rejectSource = eve.CasterEnchant.attachItem;
			eve.gameState.actionManager.EnqueueAction(new ChangeEnchantAction(eve.TargetItem, eve.SourceItem, eve.CasterEnchant.def, -1));
		}
	}
}
