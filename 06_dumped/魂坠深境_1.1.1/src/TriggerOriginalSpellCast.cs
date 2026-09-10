using System.Collections.Generic;

public class TriggerOriginalSpellCast : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		SpellCastInfo spellInfo = eve.spellInfo;
		if (spellInfo == null || spellInfo.spellItem == null)
		{
			return NodeCastResult.Failure;
		}
		Effect.EnqueueAction(eve, new ActionAction(delegate
		{
			if (spellInfo.spellItem.Companion)
			{
				ItemContainer owner = spellInfo.spellItem.owner;
				if (owner == null || owner.type != ItemContainer.ContainerType.Companion)
				{
					eve.gameState.actionManager.EnqueueAction(new SummonCompanionAction(spellInfo.spellItem, spellInfo.spellItem.OwnerBft));
					return;
				}
			}
			GameEvent eve2 = spellInfo.GenerateSpellCastEvent(GameEvent.TriggerType.Real);
			spellInfo.spellItem.GetOriginalSpell()?.Cast(eve2);
		}));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return null;
	}
}
