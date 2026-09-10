using System.Collections.Generic;
using System.Linq;

public class MoveEnchantToItem : Effect
{
	public ItemTargetSelector fromSelector;

	public ItemTargetSelector targetSelector;

	public bool negativeOnly;

	public Effect afterMoveEffect;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item item = fromSelector?.GetSingleItem(eve);
		Item item2 = targetSelector?.GetSingleItem(eve);
		if (item == null || item2 == null)
		{
			return NodeCastResult.Failure;
		}
		transformVFXEffect?.Play(eve, eve.CasterPivot, item2);
		foreach (Enchant item3 in item.Enchants.Where((Enchant enchant) => !negativeOnly || enchant.def.type == EnchantDef.EnchantType.Negative).ToList())
		{
			Effect.EnqueueAction(eve, new ChangeEnchantAction(item2, eve.CasterItem, item3.def, item3.value, popup: true, eve.spellInfo));
			Effect.EnqueueAction(eve, new ChangeEnchantAction(item, eve.CasterItem, item3.def, -(int)item3.value, popup: true, eve.spellInfo));
		}
		if (afterMoveEffect != null)
		{
			GameEvent nextEvent = eve.ShallowCopy();
			Effect.EnqueueAction(eve, new ActionAction(delegate
			{
				afterMoveEffect.Cast(nextEvent);
			}));
		}
		hitEffect?.Play(eve, item2);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { fromSelector, targetSelector, afterMoveEffect };
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo();
	}
}
