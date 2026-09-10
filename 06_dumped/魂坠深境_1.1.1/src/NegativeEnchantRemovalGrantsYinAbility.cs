using System;
using System.Collections.Generic;

public sealed class NegativeEnchantRemovalGrantsYinAbility : Ability
{
	private const string RemainderKey = "DetoxNegativeEnchantRemainder";

	public CheckNode upgradedCheck;

	public ValueGenerator layersPerPower;

	public ValueGenerator yinGain;

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve) || eve.type != GameEvent.Type.EnchantValueChange || eve.deltaValue >= 0)
		{
			return false;
		}
		Item eveObj = eve.GetEveObj<Item>(GameEvent.EveObjType.Destination);
		if (eveObj != null && eveObj.owner?.type == ItemContainer.ContainerType.Table && eveObj.OwnerBft == eve.CasterItem?.OwnerBft)
		{
			Enchant targetEnchant = eve.TargetEnchant;
			if (targetEnchant == null)
			{
				return false;
			}
			return targetEnchant.def?.type == EnchantDef.EnchantType.Negative;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		int num = layersPerPower?.GenerateValue(eve) ?? 2;
		bool? flag = upgradedCheck?.Check(eve);
		bool num2;
		if (!flag.HasValue)
		{
			Item casterItem = eve.CasterItem;
			if (casterItem == null)
			{
				goto IL_007b;
			}
			num2 = casterItem.upgradeLevel > 0;
		}
		else
		{
			num2 = flag == true;
		}
		if (num2)
		{
			num = 1;
		}
		goto IL_007b;
		IL_007b:
		int num3 = yinGain?.GenerateValue(eve) ?? 1;
		if (num > 0 && num3 > 0)
		{
			Item casterItem2 = eve.CasterItem;
			Item item = casterItem2;
			if (item.tempValues == null)
			{
				item.tempValues = new Dictionary<string, object>();
			}
			int num4 = ((casterItem2.tempValues.TryGetValue("DetoxNegativeEnchantRemainder", out var value) && value is int num5) ? num5 : 0) + Math.Abs(eve.deltaValue);
			int num6 = num4 / num;
			casterItem2.tempValues["DetoxNegativeEnchantRemainder"] = num4 % num;
			if (num6 > 0)
			{
				EnqueueAction(eve, new TrigramsManager.ChangYinYangPowerAction(num6 * num3, 0, TrigramsManager.DurationType.Temp, casterItem2));
			}
		}
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.EnchantValueChange;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { layersPerPower, yinGain, upgradedCheck };
	}
}
