using System.Collections.Generic;
using System.Linq;

public class MeridianReversalAbility : Ability
{
	private const string ActiveKey = "MeridianReversal.Active";

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.BattleStart;
		yield return GameEvent.Type.PlayerTurnStart;
		yield return GameEvent.Type.AfterSpellCast;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		Influence influence = GetInfluence(eve?.gameState);
		if (influence == null)
		{
			return;
		}
		if (eve.type == GameEvent.Type.BattleStart || eve.type == GameEvent.Type.PlayerTurnStart)
		{
			SetActive(influence, active: true);
			eve.gameState.UIManager?.SetDirty();
			return;
		}
		SpellCastInfo spellInfo = eve.spellInfo;
		if (eve.type == GameEvent.Type.AfterSpellCast && spellInfo != null && spellInfo.casterType == SpellCastInfo.CasterType.Player && !spellInfo.isTriggerCast && spellInfo.resourceDirectionReversalApplied)
		{
			SetActive(influence, active: false);
			eve.gameState.UIManager?.SetDirty();
			if (spellInfo.spellItem != null)
			{
				eve.gameState.actionManager.EnqueueAction(new SetCoolDownAction(spellInfo.spellItem, spellInfo.spellItem, 0));
			}
		}
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (eve != null)
		{
			return GetAllSubscribeGameEventType().Contains(eve.type);
		}
		return false;
	}

	public static bool ShouldReverse(SpellCastInfo castInfo, GameEvent.TriggerType triggerType)
	{
		if (castInfo?.gameState != null)
		{
			Item spellItem = castInfo.spellItem;
			if (spellItem != null && spellItem.OwnerBft?.isPlayer == true && !castInfo.isTriggerCast)
			{
				SpellCastInfo.CasterType casterType = castInfo.casterType;
				if (casterType != SpellCastInfo.CasterType.Enemy && casterType != SpellCastInfo.CasterType.Effect)
				{
					if (!IsActive(GetInfluence(castInfo.gameState)))
					{
						return false;
					}
					GameEvent eve = castInfo.GenerateSpellCastEvent(triggerType);
					if (!HasReversibleAutomaticCost(castInfo, triggerType))
					{
						return YinYangFeatureAbility.HasReversibleChange(castInfo.spellItem, eve);
					}
					return true;
				}
			}
		}
		return false;
	}

	public static bool ShouldReverseYinYang(GameEvent eve)
	{
		if (eve?.gameState != null)
		{
			Item casterItem = eve.CasterItem;
			if (casterItem != null && casterItem.OwnerBft?.isPlayer == true)
			{
				if (eve.spellInfo != null)
				{
					eve.spellInfo.ResolveResourceDirectionReversal(eve.triggerType);
					return eve.spellInfo.resourceDirectionReversalActive;
				}
				if (IsActive(GetInfluence(eve.gameState)))
				{
					return YinYangFeatureAbility.HasReversibleChange(eve.CasterItem, eve);
				}
				return false;
			}
		}
		return false;
	}

	public static bool HasReversibleAutomaticCost(SpellCastInfo castInfo, GameEvent.TriggerType triggerType)
	{
		ItemSpell itemSpell = castInfo?.focusSpell;
		if (itemSpell?.requirement == null || itemSpell.MovesHeadToCasterRightAsCost)
		{
			return false;
		}
		if (castInfo.spellItem.IsManualSelectCost(castInfo.gameState, triggerType))
		{
			return false;
		}
		if (!itemSpell.ConsumesAllAvailableCost)
		{
			return itemSpell.requirement.GetFinalCostValue(castInfo, triggerType) > 0;
		}
		return true;
	}

	public static bool IsActive(GameStateInfo gameState)
	{
		return IsActive(GetInfluence(gameState));
	}

	private static Influence GetInfluence(GameStateInfo gameState)
	{
		return gameState?.playerState?.bft?.influences?.FirstOrDefault((Influence influence) => influence != null && influence.def?.abilities?.OfType<MeridianReversalAbility>().Any() == true);
	}

	private static bool IsActive(Influence influence)
	{
		if (influence == null)
		{
			return false;
		}
		if (influence.tempValues == null)
		{
			influence.tempValues = new Dictionary<string, object>();
		}
		if (influence.tempValues.TryGetValue("MeridianReversal.Active", out var value))
		{
			if (value is bool)
			{
				return (bool)value;
			}
			return false;
		}
		return true;
	}

	private static void SetActive(Influence influence, bool active)
	{
		if (influence.tempValues == null)
		{
			influence.tempValues = new Dictionary<string, object>();
		}
		influence.tempValues["MeridianReversal.Active"] = active;
	}
}
