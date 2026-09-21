using System;
using System.Collections.Generic;

public sealed class HarmonizeYinYangEffect : Effect
{
	public CheckNode upgradedCheck;

	public ValueGenerator drawThreshold;

	public ValueGenerator drawCount;

	public DrawFromPile drawEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		TrigramsManager trigramsManager = eve.gameState?.trigramsManager;
		if (trigramsManager == null)
		{
			return NodeCastResult.Failure;
		}
		int num = (int)trigramsManager.yinPower - (int)trigramsManager.yangPower;
		int num2 = Math.Abs(num) / 2;
		if (num2 <= 0)
		{
			return NodeCastResult.Success;
		}
		TrigramsManager.YinYangType yinYangType = ((num > 0) ? TrigramsManager.YinYangType.Yin : TrigramsManager.YinYangType.Yang);
		GameEvent.ResolveInfo resolveInfo = eve.gameState.gameEventManager.YinYangConvertRequirementResolve(eve.CasterItem, num2, yinYangType, eve.triggerType);
		if (resolveInfo.reject)
		{
			return NodeCastResult.Success;
		}
		int actual = Math.Max(0, num2 + resolveInfo.deltaValue);
		actual = Math.Min(actual, num2);
		if (actual <= 0)
		{
			return NodeCastResult.Success;
		}
		Effect.EnqueueAction(eve, new TrigramsManager.ConvertYinYangPowerAction(actual, yinYangType, eve.CasterItem));
		Effect.EnqueueAction(eve, new ActionAction(delegate
		{
			int num3 = drawThreshold?.GenerateValue(eve) ?? 3;
			if (actual >= num3)
			{
				bool? flag = upgradedCheck?.Check(eve);
				int num4;
				if (!flag.HasValue)
				{
					Item casterItem = eve.CasterItem;
					num4 = ((casterItem != null && casterItem.upgradeLevel > 0) ? 1 : 0);
				}
				else
				{
					num4 = ((flag == true) ? 1 : 0);
				}
				bool flag2 = (byte)num4 != 0;
				int num5 = drawCount?.GenerateValue(eve) ?? ((!flag2) ? 1 : 2);
				for (int i = 0; i < num5; i++)
				{
					drawEffect?.Cast(eve);
				}
			}
		}));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { drawThreshold, drawCount, drawEffect, upgradedCheck };
	}
}
