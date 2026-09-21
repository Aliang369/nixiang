using System.Collections.Generic;
using System.Linq;

public sealed class YinYangConversionCounterRewardAbility : Ability
{
	public CheckNode upgradedCheck;

	public ValueGenerator drawCount;

	public EnchantDef powerfulEnchant;

	public ItemTag attackTag;

	public ValueGenerator powerfulCount;

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (eve.type != GameEvent.Type.ConvertYinYangPower)
		{
			return;
		}
		Item item = eve.CasterItem;
		EnqueueAction(eve, new ChangeCounterAction(1, item, item, withPopupText: true, consumeDefenceWhenDepleted: false, delegate(int _, int actualDelta, int after, int maximum)
		{
			if (actualDelta > 0 && maximum > 0 && after >= maximum)
			{
				EnqueueAction(eve, new SetCounterAction(0, item, item));
				int num = drawCount?.GenerateValue(eve) ?? 1;
				for (int i = 0; i < num; i++)
				{
					EnqueueAction(eve, new DrawFromPileAction(item.OwnerBft, item, 0, DrawFromPileAction.DrawMode.Random, delegate(Item drawn)
					{
						if (drawn != null)
						{
							EnqueueAction(eve, new ActionAction(delegate
							{
								if (!eve.gameState.trigramsManager.GetCurrentState().HasFlag(TrigramsManager.YinYangStateType.Balance))
								{
									return;
								}
								List<Item> list = (from candidate in item.OwnerBft.tableContainer.GetAllItems().Concat(item.OwnerBft.drawPileItemContainer.GetAllItems()).Concat(item.OwnerBft.wastePileItemContainer.GetAllItems())
									where candidate?.HasTag(attackTag) ?? false
									select candidate).Distinct().ToList();
								bool? flag = upgradedCheck?.Check(eve);
								int num2;
								if (!flag.HasValue)
								{
									Item item2 = item;
									num2 = ((item2 != null && item2.upgradeLevel > 0) ? 1 : 0);
								}
								else
								{
									num2 = ((flag == true) ? 1 : 0);
								}
								bool flag2 = (byte)num2 != 0;
								int delta = powerfulCount?.GenerateValue(eve) ?? ((!flag2) ? 1 : 3);
								foreach (Item item3 in list)
								{
									EnqueueAction(eve, new ChangeEnchantAction(item3, item, powerfulEnchant, delta));
								}
							}));
						}
					}));
				}
			}
		}));
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.ConvertYinYangPower;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int value = drawCount?.GenerateValue(eve) ?? 1;
		bool? flag = upgradedCheck?.Check(eve);
		int num;
		if (!flag.HasValue)
		{
			Item casterItem = eve.CasterItem;
			num = ((casterItem != null && casterItem.upgradeLevel > 0) ? 1 : 0);
		}
		else
		{
			num = ((flag == true) ? 1 : 0);
		}
		bool flag2 = (byte)num != 0;
		int value2 = powerfulCount?.GenerateValue(eve) ?? ((!flag2) ? 1 : 3);
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(value),
			refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>(new List<EnchantDef> { powerfulEnchant }),
			refEnchants = new NodeDisplayInfo.Element<List<Enchant>>(new List<Enchant>
			{
				new Enchant(powerfulEnchant, value2)
			})
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { drawCount, powerfulCount, upgradedCheck };
	}
}
