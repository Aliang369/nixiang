using System;
using System.Collections.Generic;

public sealed class EvilGazeMaskBeforeDamageAbility : Ability
{
	public ItemSkinDef evilSkin;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.BeforeDamage;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		Item item = eve?.TargetItem;
		Damage damage = eve?.damage;
		bool flag = item != null && evilSkin != null && item.skin == evilSkin && item.def?.GetItemSkinConfig(item.skin)?.itemSkinDef == evilSkin;
		if (((eve != null && eve.type == GameEvent.Type.BeforeDamage) & flag) && damage != null && damage.targetItem == item && damage.subCounter > 0)
		{
			return !damage.isOverflowDamage;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			BattleFieldTransform ownerBft = eve.TargetItem.OwnerBft;
			if (ownerBft != null)
			{
				int num = Math.Max(1, eve.TargetItem.GetCurrentDefendCounter() / 10);
				eve.gameState.actionManager.EnqueueAction(new ChangeBftHealthAction(ownerBft, -num, eve.TargetItem, null, realHealthOnly: true));
			}
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(10)
		};
	}
}
