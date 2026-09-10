using System;
using System.Collections.Generic;

public class GameEventTriggerAbility : Ability
{
	public enum TriggerCondition
	{
		None = 0,
		CasterIsTarget = 1,
		CasterIsSource = 2,
		CasterNotTarget = 3,
		CasterNotSource = 4,
		SourceOwnerIsCasterOwner = 5,
		TargetOwnerIsCasterOwner = 6,
		SourceOwnByOpponent = 7,
		TargetOwnByOpponent = 8,
		CasterIsSpellSource = 9,
		AddItemToOwner = 10,
		AimItemIsTarget = 11,
		AimItemIsSource = 12,
		AddItemToOpponent = 14,
		CasterAttachIsSource = 16,
		CasterAttachIsTarget = 17,
		CasterAttachNotSource = 18,
		CasterAttachNotTarget = 19,
		SpellIsOverride = 20,
		CasterIsDestination = 21,
		DestinationOwnerIsCasterOwner = 22,
		TargetIsPlayer = 23
	}

	public GameEvent.Type type;

	public TriggerCondition condition;

	public Effect effect;

	public ItemSkinDef excludeSkinDef;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, effect };
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return type;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve) && (eve.type != GameEvent.Type.MoveItemToHead || !(eve.CasterItem?.def?.id == "afa80466af3115f4e823f85b3fb091b2") || eve.gameState.actionManager.TryRegisterShadowEscapeChainItem(eve.CasterItem, eve.CasterItem.owner)))
		{
			TriggerEffect(eve, effect);
		}
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (eve.type != type)
		{
			return false;
		}
		if (excludeSkinDef != null && eve.CasterItem?.skin == excludeSkinDef)
		{
			return false;
		}
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		switch (condition)
		{
		case TriggerCondition.None:
			return true;
		case TriggerCondition.CasterIsTarget:
			return eve.GetEveObj(GameEvent.EveObjType.Caster) == eve.GetEveObj(GameEvent.EveObjType.Target);
		case TriggerCondition.CasterIsSource:
			return eve.GetEveObj(GameEvent.EveObjType.Caster) == eve.GetEveObj(GameEvent.EveObjType.Source);
		case TriggerCondition.CasterNotTarget:
			return eve.GetEveObj(GameEvent.EveObjType.Caster) != eve.GetEveObj(GameEvent.EveObjType.Target);
		case TriggerCondition.CasterNotSource:
			return eve.GetEveObj(GameEvent.EveObjType.Caster) != eve.GetEveObj(GameEvent.EveObjType.Source);
		case TriggerCondition.SourceOwnerIsCasterOwner:
			return eve.CasterOwner == eve.SourceOwner;
		case TriggerCondition.TargetOwnerIsCasterOwner:
			return eve.CasterOwner == eve.TargetOwner;
		case TriggerCondition.SourceOwnByOpponent:
			return eve.OpponentBFT == eve.SourceOwner;
		case TriggerCondition.TargetOwnByOpponent:
			return eve.OpponentBFT == eve.TargetOwner;
		case TriggerCondition.CasterIsSpellSource:
			if (eve.spellInfo != null)
			{
				return eve.CasterItem == eve.spellInfo?.spellItem;
			}
			return false;
		case TriggerCondition.AddItemToOwner:
			if (eve.addItemInfo?.targetContainer?.owner != null)
			{
				return eve.addItemInfo.targetContainer.owner == eve.CasterOwner;
			}
			return false;
		case TriggerCondition.AimItemIsTarget:
			return eve.SourceItem == eve.CasterItem.aimItem;
		case TriggerCondition.AimItemIsSource:
			return eve.TargetItem == eve.CasterItem.aimItem;
		case TriggerCondition.AddItemToOpponent:
			return eve.addItemInfo?.targetContainer.owner == eve.OpponentBFT;
		case TriggerCondition.CasterAttachIsSource:
			return eve.CasterAttachObject == eve.GetEveObj(GameEvent.EveObjType.Source);
		case TriggerCondition.CasterAttachIsTarget:
			return eve.CasterAttachObject == eve.GetEveObj(GameEvent.EveObjType.Target);
		case TriggerCondition.CasterAttachNotSource:
			return eve.CasterAttachObject != eve.GetEveObj(GameEvent.EveObjType.Source);
		case TriggerCondition.CasterAttachNotTarget:
			return eve.CasterAttachObject != eve.GetEveObj(GameEvent.EveObjType.Target);
		case TriggerCondition.SpellIsOverride:
			if (eve.CasterItem != null)
			{
				return eve.CasterItem.IsCastingOverrideSpell(eve.spellInfo);
			}
			return false;
		case TriggerCondition.CasterIsDestination:
			return eve.GetEveObj(GameEvent.EveObjType.Caster) == eve.GetEveObj(GameEvent.EveObjType.Destination);
		case TriggerCondition.DestinationOwnerIsCasterOwner:
			if (eve.CasterOwner != null)
			{
				return eve.GetEveObj<BattleFieldTransform>(GameEvent.EveObjType.Get_DestinationBftOwner) == eve.CasterOwner;
			}
			return false;
		case TriggerCondition.TargetIsPlayer:
			if (eve.TargetOwner != null)
			{
				return eve.TargetOwner == eve.gameState?.playerState?.bft;
			}
			return false;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
