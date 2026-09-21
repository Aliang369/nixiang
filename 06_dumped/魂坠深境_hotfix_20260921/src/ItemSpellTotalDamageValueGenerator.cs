using System.Collections.Generic;
using System.Linq;

public sealed class ItemSpellTotalDamageValueGenerator : ValueGenerator
{
	public ItemTargetSelector targetSelector;

	public override int? GenerateValue(GameEvent eve)
	{
		Item item = targetSelector?.GetSingleItem(eve);
		List<DealDamage> list = item?.CurrentSpell?.spellNode?.GetDescendantNodeOfType<DealDamage>(includeSelf: true);
		if (list == null)
		{
			return 0;
		}
		long num = 0L;
		foreach (DealDamage item2 in list.Where((DealDamage node) => node.targetBFT != GameEvent.EventBFTType.Owner))
		{
			SpellCastInfo spellInfo = ((eve.spellInfo?.spellItem == item) ? eve.spellInfo : new DisplaySpellCastInfo(eve.gameState, eve.triggerType == GameEvent.TriggerType.InBattleDisplay)
			{
				spellItem = item
			});
			GameEvent eve2 = Effect.GenerateDisplayEvent(eve.gameState, item, spellInfo, eve.triggerType);
			num += item2.GetPreviewCastDamage(eve2).GetValueOrDefault();
		}
		return CombatValueLimit.Clamp(num);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}
}
