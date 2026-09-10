using System.Linq;
using UnityEngine;

public class ItemSpellDamageValueGenerator : ValueGenerator
{
	[SerializeField]
	private GameEvent.EventItemType itemType = GameEvent.EventItemType.CasterItem;

	public override int? GenerateValue(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(itemType);
		DealDamage dealDamage = eventItem?.CurrentSpell?.spellNode?.GetDescendantNodeOfType<DealDamage>(includeSelf: true).FirstOrDefault((DealDamage node) => node.targetBFT != GameEvent.EventBFTType.Owner);
		if (dealDamage == null)
		{
			return null;
		}
		GameEvent.TriggerType triggerType = ((eve.triggerType == GameEvent.TriggerType.Real && eve.gameState?.actionManager == null) ? GameEvent.TriggerType.InBattleDisplay : eve.triggerType);
		SpellCastInfo spellInfo = ((eve.spellInfo?.spellItem == eventItem) ? eve.spellInfo : new DisplaySpellCastInfo(eve.gameState, triggerType == GameEvent.TriggerType.InBattleDisplay)
		{
			spellItem = eventItem
		});
		GameEvent eve2 = Effect.GenerateDisplayEvent(eve.gameState, eventItem, spellInfo, triggerType);
		return dealDamage.GetPreviewCastDamage(eve2);
	}
}
