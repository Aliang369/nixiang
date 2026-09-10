using System.Linq;

public class CastFireControllerWarmingCyclone : SequenceSpells, IAttackIntentNode, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	private const string WarmingCycloneFireSeedGuid = "97fb47d81f913754aa2ac35e19f09a57";

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		SpellCastInfo spellInfo = eve.spellInfo;
		if (spellInfo == null || !spellInfo.triggerFromProtectedExile)
		{
			return base.OnCast(eve);
		}
		if (nodes == null)
		{
			return NodeCastResult.Success;
		}
		if (CastEffectInActionQueue)
		{
			GameEvent neve = eve.ShallowCopy();
			foreach (Effect spell in nodes)
			{
				if (!IsSelfFireSeedChange(spell))
				{
					Effect.EnqueueAction(eve, new ActionAction(delegate
					{
						spell.Cast(neve);
					}));
				}
			}
		}
		else
		{
			foreach (Effect node in nodes)
			{
				if (!IsSelfFireSeedChange(node))
				{
					node.Cast(eve);
				}
			}
		}
		return NodeCastResult.Success;
	}

	private static bool IsSelfFireSeedChange(Effect spell)
	{
		if (!(spell is ChangeEnchant changeEnchant))
		{
			return false;
		}
		if (!(changeEnchant.targetSelector is SingleTargetSelector { target: GameEvent.EventItemType.CasterItem }))
		{
			return false;
		}
		return (changeEnchant.enchantGenerator as PredefinedEnchantGenerator)?.enchantDef?.id == "97fb47d81f913754aa2ac35e19f09a57";
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = nodes?.OfType<DealDamage>().FirstOrDefault()?.GetNodeDisplayInfo(eve);
		if (nodeDisplayInfo == null)
		{
			return null;
		}
		NodeDisplayInfo nodeDisplayInfo2 = nodes?.OfType<ChangeEnchant>().FirstOrDefault()?.GetNodeDisplayInfo(eve);
		if (nodeDisplayInfo2?.refEnchantDefs != null)
		{
			nodeDisplayInfo.refEnchantDefs = nodeDisplayInfo2.refEnchantDefs;
		}
		if (nodeDisplayInfo2?.refEnchants != null)
		{
			nodeDisplayInfo.refEnchants = nodeDisplayInfo2.refEnchants;
		}
		return nodeDisplayInfo;
	}
}
