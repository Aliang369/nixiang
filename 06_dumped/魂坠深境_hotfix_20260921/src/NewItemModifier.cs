using System.Collections.Generic;
using UnityEngine;

public class NewItemModifier : NewItemGenerator
{
	public class BlackBoardModifyValye
	{
		public ItemBlackBoard.ValueType type;

		public ValueGenerator valueGenerator;
	}

	[SerializeField]
	private NewItemGenerator itemGenerator;

	[SerializeField]
	private List<BlackBoardModifyValye> blackBoardModifiers;

	[SerializeField]
	private ValueGenerator cooldownGenerator;

	[SerializeField]
	private ValueGenerator deltaCostGenerator;

	[SerializeField]
	private ValueGenerator counterGenerator;

	[SerializeField]
	private ValueGenerator maxCounterGenerator;

	[SerializeField]
	private ValueGenerator upgradelevelGenerator;

	[SerializeField]
	private RuneGenerator runeGenerator;

	[SerializeField]
	private EnchantDef enchantDef;

	[SerializeField]
	private ValueGenerator enchantValue;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		List<Item> list = itemGenerator.GenerateNewItems(eve);
		if (list == null)
		{
			return null;
		}
		if (counterGenerator != null)
		{
			int? counter = counterGenerator.GenerateValue(eve);
			if (!counter.HasValue)
			{
				return null;
			}
			list.ForEach(delegate(Item i)
			{
				i.counter = counter.Value;
			});
		}
		if (maxCounterGenerator != null)
		{
			int? maxCounter = maxCounterGenerator.GenerateValue(eve);
			if (!maxCounter.HasValue)
			{
				return null;
			}
			list.ForEach(delegate(Item i)
			{
				i.maxCounter = maxCounter.Value;
			});
		}
		if (cooldownGenerator != null)
		{
			int? cooldown = cooldownGenerator.GenerateValue(eve);
			if (!cooldown.HasValue)
			{
				return null;
			}
			list.ForEach(delegate(Item i)
			{
				i.cooldown = cooldown.Value;
			});
		}
		if (deltaCostGenerator != null)
		{
			int? deltaCost = deltaCostGenerator.GenerateValue(eve);
			if (!deltaCost.HasValue)
			{
				return null;
			}
			list.ForEach(delegate(Item i)
			{
				i.deltaCost = (int)i.deltaCost + deltaCost.Value;
			});
		}
		if (upgradelevelGenerator != null)
		{
			int? upgradeLevel = upgradelevelGenerator.GenerateValue(eve);
			if (!upgradeLevel.HasValue)
			{
				return null;
			}
			foreach (Item item in list)
			{
				GameEvent eve2 = new GameEvent(GameEvent.Type.UpgradeItem, GameEvent.TriggerType.Real)
				{
					gameState = eve.gameState,
					CasterItem = item
				};
				int? num = upgradeLevel - item.upgradeLevel;
				if (num > 0 && item.CurrentUpgradeEffectNode != null)
				{
					for (int num2 = 0; num2 < num; num2++)
					{
						item.CurrentUpgradeEffectNode.Cast(eve2);
					}
				}
				else if (num < 0 && item.CurrentDowngradeEffectNode != null)
				{
					for (int num3 = 0; num3 < -num; num3++)
					{
						item.CurrentDowngradeEffectNode.Cast(eve2);
					}
				}
			}
			list.ForEach(delegate(Item i)
			{
				i.upgradeLevel = upgradeLevel.Value;
			});
		}
		if (runeGenerator != null)
		{
			List<Rune> list2 = runeGenerator.GenerateRunes(eve);
			if (list2 == null)
			{
				return null;
			}
			foreach (Item item2 in list)
			{
				foreach (Rune item3 in list2)
				{
					item2.AddRune(item3.CopyToItem(item2));
				}
			}
		}
		if (enchantDef != null)
		{
			int value = enchantValue?.GenerateValue(eve) ?? 1;
			Enchant enchant = new Enchant(enchantDef, value);
			foreach (Item item4 in list)
			{
				item4.AddEnchant(enchant);
			}
		}
		if (blackBoardModifiers != null)
		{
			foreach (BlackBoardModifyValye blackBoardModifier in blackBoardModifiers)
			{
				if (blackBoardModifier.valueGenerator == null)
				{
					continue;
				}
				int? num4 = blackBoardModifier.valueGenerator.GenerateValue(eve);
				if (!num4.HasValue)
				{
					return null;
				}
				foreach (Item item5 in list)
				{
					if (item5.blackboard == null)
					{
						item5.blackboard = new ItemBlackBoard();
					}
					item5.blackboard.SetValue(blackBoardModifier.type, num4.Value);
				}
			}
		}
		return list;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode> { itemGenerator, counterGenerator, maxCounterGenerator, cooldownGenerator, deltaCostGenerator, upgradelevelGenerator, runeGenerator };
		if (blackBoardModifiers != null)
		{
			foreach (BlackBoardModifyValye blackBoardModifier in blackBoardModifiers)
			{
				list.Add(blackBoardModifier.valueGenerator);
			}
		}
		return list;
	}
}
