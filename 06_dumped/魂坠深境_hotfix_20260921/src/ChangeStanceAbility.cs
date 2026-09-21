using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeStanceAbility : Ability
{
	public enum AbilityAccessTime
	{
		BeforeChangeStance,
		AfterChangeStance,
		ChangeStanceValueResolve,
		ChangeStanceValueWithChangeTypeResolve
	}

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
		CasterAttachNotTarget = 19
	}

	public Effect stanceTriggerEffect;

	public TriggerCondition condition;

	public bool anyStance;

	public Stance.ChangeType stanceChangeType;

	public AbilityAccessTime abilityAccessTime;

	public bool disableItemAnim;

	private static bool shouldCheckAccessTime = true;

	public override void TriggerGameEvent(GameEvent eve)
	{
		GameEvent.Type type = CheckGameEventType();
		bool flag = false;
		if (shouldCheckAccessTime)
		{
			Enum.TryParse<Stance.ChangeType>(eve.stringValue, out var result);
			flag = stanceChangeType == result;
			if (result == Stance.ChangeType.Shift && CheckStackStanceAltPotentialFlag(eve) && stanceChangeType == Stance.ChangeType.Stack)
			{
				flag = true;
			}
		}
		if (anyStance)
		{
			flag = true;
		}
		bool flag2 = CheckTriggerGameEventAble(eve);
		if (!flag || eve.type != type || !flag2)
		{
			return;
		}
		if (IsPlayerAnimAble(eve))
		{
			if (eve.CasterItem != null)
			{
				if (!disableItemAnim)
				{
					TriggerItemAnim(eve);
				}
			}
			else if (eve.CasterStance != null)
			{
				TriggerStanceAnim(eve);
			}
		}
		stanceTriggerEffect?.Cast(eve);
	}

	private GameEvent.Type CheckGameEventType()
	{
		shouldCheckAccessTime = true;
		switch (abilityAccessTime)
		{
		case AbilityAccessTime.BeforeChangeStance:
			return GameEvent.Type.BeforeChangeStance;
		case AbilityAccessTime.AfterChangeStance:
			return GameEvent.Type.AfterChangeStance;
		case AbilityAccessTime.ChangeStanceValueResolve:
			shouldCheckAccessTime = false;
			return GameEvent.Type.ChangeStanceValueResolve;
		case AbilityAccessTime.ChangeStanceValueWithChangeTypeResolve:
			return GameEvent.Type.ChangeStanceValueWithChangeTypeResolve;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	protected new void TriggerStanceAnim(GameEvent eve)
	{
		GameStateInfo gameState = eve.gameState;
		Stance eveObj = eve.GetEveObj<Stance>(GameEvent.EveObjType.Origin);
		Stance eveObj2 = eve.GetEveObj<Stance>(GameEvent.EveObjType.Destination);
		gameState.UIManager.stanceDisplay.ApplyStance(eveObj2);
		gameState.applicationState.runningGameState.StartCoroutine(WaitForFrame);
		if ((eveObj != null || eveObj2 != null) && animPriority != AbilityAnimPriority.None)
		{
			if (gameState.UIManager.stanceDisplay != null)
			{
				gameState.UIManager.stanceDisplay.TriggerAbilityAnim(eveObj2, animPriority);
			}
			else
			{
				Debug.LogWarning("StanceDisplay is not initialized, cannot trigger stance ability animation.");
			}
		}
	}

	private IEnumerator WaitForFrame()
	{
		yield return new WaitForEndOfFrame();
	}

	protected void TriggerItemAnim(GameEvent eve)
	{
		GameStateInfo gameState = eve.gameState;
		Item eveObj = eve.GetEveObj<Item>(GameEvent.EveObjType.Caster);
		if (eveObj != null && animPriority != AbilityAnimPriority.None)
		{
			BattleFieldTransform ownerBft = eveObj.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer && eveObj.owner?.type == ItemContainer.ContainerType.Table)
			{
				gameState.UIManager.playerItemArea.TriggerStanceItemAnim(eveObj, animPriority, eve);
			}
		}
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.BeforeChangeStance;
		yield return GameEvent.Type.AfterChangeStance;
		yield return GameEvent.Type.ChangeStanceValueResolve;
		yield return GameEvent.Type.ChangeStanceValueWithChangeTypeResolve;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		list.Add(triggerCheckNode);
		if (stanceTriggerEffect != null)
		{
			list.Add(stanceTriggerEffect);
		}
		return list;
	}

	public KeywordDef GetChangeTypeKeyword(GameEvent eve, string str, Stance.ChangeType changeType)
	{
		return eve.gameState.applicationState.assetCatalog.keywordCatalog.assets.Find((KeywordDef k) => k.textId == str);
	}

	private bool CheckStackStanceAltPotentialFlag(GameEvent eve)
	{
		if (eve.gameState?.globalVariableStateInfo == null)
		{
			return false;
		}
		return eve.gameState.globalVariableStateInfo.GetVariableValue("StackStanceAltPotentialFlag") > 0;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
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
			return eve.addItemInfo?.targetContainer.owner == eve.CasterItem.OwnerBft;
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
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
