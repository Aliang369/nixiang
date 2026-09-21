public class IceArmorTurnTriggerAbility : TurnTriggerAbility
{
	private const string IceArmorItemGuid = "7c944459b959a6d45bff374199049f13";

	private const string BearMageIceArmorSkinGuid = "aac55f1ef3dcfe24abfa9b3b5519d8ce";

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			TriggerEffect(eve, effect);
		}
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		Item item = eve?.CasterItem;
		if (!(item?.def?.id != "7c944459b959a6d45bff374199049f13"))
		{
			ItemContainer owner = item.owner;
			if (owner != null && owner.type == ItemContainer.ContainerType.Table)
			{
				if (!IsSelfTurnEvent(eve, item.OwnerBft))
				{
					return false;
				}
				if (IsBearMageIceArmor(item))
				{
					if (eve.type != GameEvent.Type.PlayerTurnStart && eve.type != GameEvent.Type.EnemyTurnStart && eve.type != GameEvent.Type.PlayerTurnEnd)
					{
						return eve.type == GameEvent.Type.EnemyTurnEnd;
					}
					return true;
				}
				if (eve.type != GameEvent.Type.PlayerTurnEnd)
				{
					return eve.type == GameEvent.Type.EnemyTurnEnd;
				}
				return true;
			}
		}
		return false;
	}

	private static bool IsSelfTurnEvent(GameEvent eve, BattleFieldTransform owner)
	{
		if (owner == null)
		{
			return false;
		}
		if (!owner.isPlayer || (eve.type != GameEvent.Type.PlayerTurnStart && eve.type != GameEvent.Type.PlayerTurnEnd))
		{
			if (owner.isEnemy)
			{
				if (eve.type != GameEvent.Type.EnemyTurnStart)
				{
					return eve.type == GameEvent.Type.EnemyTurnEnd;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool IsBearMageIceArmor(Item item)
	{
		return item?.skin?.id == "aac55f1ef3dcfe24abfa9b3b5519d8ce";
	}
}
