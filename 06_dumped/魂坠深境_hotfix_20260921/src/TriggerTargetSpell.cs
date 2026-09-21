using System.Collections.Generic;

public class TriggerTargetSpell : Effect
{
	public enum TargetingType
	{
		Default,
		Random
	}

	public TargetingType targetingType;

	public ItemTargetSelector toTriggerItemSelector;

	public EnchantDefGenerator negativeEnchantReplacementGenerator;

	public bool suppressChangeEnchantAndSpawnItemEffects;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	public bool playTargetIconEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = toTriggerItemSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in list)
		{
			if (item == null)
			{
				continue;
			}
			ItemContainer owner = item.owner;
			bool flag = owner != null && owner.type == ItemContainer.ContainerType.ExilePile;
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			if (playTargetIconEffect)
			{
				PlayerTableDisplayStateModule displayModule = ItemDisplayBase.GetActiveItemSlotDisplay(item)?.GetModule<PlayerTableDisplayStateModule>();
				if (displayModule != null)
				{
					Effect.EnqueueAction(eve, new EnumeratorAction(() => displayModule.PlayIconEffect(4)));
				}
			}
			bool flag2 = suppressChangeEnchantAndSpawnItemEffects || (eve.spellInfo?.suppressChangeEnchantAndSpawnItemEffects ?? false);
			Effect.EnqueueAction(eve, new TriggerCastItemAction(item, eve.CasterItem, targetingType, flag, flag, flag, flag, negativeEnchantReplacementGenerator, flag2));
			hitEffect?.Play(eve, item);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { toTriggerItemSelector, negativeEnchantReplacementGenerator };
	}
}
