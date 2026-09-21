using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class DancerCharmTargetOwnerHalfMaxHealthDamageEffect : Effect
{
	public ItemTargetSelector targetSelector;

	public bool overflow = true;

	public bool pierce;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = ResolveTarget(eve)?.OwnerBft;
		if (battleFieldTransform == null)
		{
			return NodeCastResult.Failure;
		}
		Effect.EnqueueAction(eve, new DamageAction(new DamageCommand
		{
			baseDamage = Mathf.Max(0, (int)battleFieldTransform.maxHealth / 2),
			source = (eve.CasterItem ?? eve.CasterSkill ?? eve.Caster),
			targetBFT = battleFieldTransform,
			overflow = overflow,
			pierce = pierce,
			spellCastInfo = ((eve.type == GameEvent.Type.NodeCastSpell) ? eve.spellInfo : null)
		}));
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		ProtectedInt? protectedInt = ResolveTarget(eve)?.OwnerBft?.maxHealth;
		int? value = (protectedInt.HasValue ? new int?((int)protectedInt.Value / 2) : ((int?)null));
		return new NodeDisplayInfo
		{
			baseValue = new NodeDisplayInfo.Element<int?>(value),
			value = new NodeDisplayInfo.Element<int?>(value),
			damage = new NodeDisplayInfo.Element<bool>(value: true)
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}

	private Item ResolveTarget(GameEvent eve)
	{
		object obj = eve?.TargetItem;
		if (obj == null)
		{
			obj = eve?.NodeTempItem;
			if (obj == null)
			{
				obj = eve?.SelectedItems?.FirstOrDefault((Item item) => item != null);
				if (obj == null)
				{
					ItemTargetSelector itemTargetSelector = targetSelector;
					if (itemTargetSelector == null)
					{
						return null;
					}
					List<Item> targetItems = itemTargetSelector.GetTargetItems(eve);
					if (targetItems == null)
					{
						return null;
					}
					obj = targetItems.FirstOrDefault();
				}
			}
		}
		return (Item)obj;
	}
}
