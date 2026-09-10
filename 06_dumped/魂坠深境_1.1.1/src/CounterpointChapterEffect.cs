using System.Collections.Generic;

public sealed class CounterpointChapterEffect : Effect
{
	public ItemTag chapterTag;

	public DealDamage normalDamage;

	public DealDamage counterpointDamage;

	public DealDamage upgradedNormalDamage;

	public DealDamage upgradedCounterpointDamage;

	public ValueGenerator chapterDelta;

	public CheckNode upgradedCheck;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item casterItem = eve.CasterItem;
		ItemContainer itemContainer = casterItem?.owner;
		int? num = casterItem?.ownerSlotIndex;
		if (itemContainer == null || !num.HasValue)
		{
			return NodeCastResult.Failure;
		}
		List<Item> allItems = itemContainer.GetAllItems();
		Item item = ((num.Value > 0) ? allItems[num.Value - 1] : null);
		Item item2 = ((num.Value + 1 < allItems.Count) ? allItems[num.Value + 1] : null);
		int num2 = ((eve.nodeTempValues.TryGetValue("TriggerChapterIndexKey", out var value) && value is int num3) ? num3 : ((int)casterItem.counter));
		bool flag = item != null && item2 != null && item.HasTag(chapterTag) && item2.HasTag(chapterTag) && (((int)item.counter > num2 && (int)item2.counter < num2) || ((int)item2.counter > num2 && (int)item.counter < num2));
		bool flag2 = upgradedCheck?.Check(eve) ?? casterItem.upgraded;
		DealDamage dealDamage = ((!flag) ? (flag2 ? upgradedNormalDamage : normalDamage) : (flag2 ? upgradedCounterpointDamage : counterpointDamage));
		if (flag2 && dealDamage == null)
		{
			DealDamage dealDamage2 = (flag ? counterpointDamage : normalDamage);
			int valueOrDefault = (dealDamage2?.baseDamageGenerator?.GenerateValue(eve)).GetValueOrDefault();
			dealDamage = new DealDamage
			{
				baseDamageGenerator = new PredefinedValueGenerator
				{
					value = valueOrDefault * 2
				},
				targetBFT = dealDamage2.targetBFT,
				overflow = dealDamage2.overflow,
				pierce = dealDamage2.pierce
			};
		}
		dealDamage?.Cast(eve);
		if (!flag)
		{
			return NodeCastResult.Success;
		}
		int num4 = chapterDelta?.GenerateValue(eve) ?? 1;
		Item[] array = new Item[2] { item, item2 };
		foreach (Item item3 in array)
		{
			GameEvent.ResolveInfo resolveInfo = eve.gameState.gameEventManager.TriggerAddChapterResolve(item3, casterItem, GameEvent.TriggerType.Real);
			if (!resolveInfo.reject)
			{
				Effect.EnqueueAction(eve, new ChangeCounterAction(resolveInfo.targetDelta + num4, item3, casterItem, withPopupText: true));
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { normalDamage, counterpointDamage, upgradedNormalDamage, upgradedCounterpointDamage, chapterDelta, upgradedCheck };
	}
}
