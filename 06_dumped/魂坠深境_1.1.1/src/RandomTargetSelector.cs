using System.Collections.Generic;
using GameRandom;
using UnityEngine;

public class RandomTargetSelector : ItemTargetSelector
{
	[SerializeField]
	private ItemTargetSelector poolSelector;

	[SerializeField]
	private ValueGenerator countGenerator;

	[SerializeField]
	private bool excludeSelf;

	[SerializeField]
	private bool repeatCountIfTooMuch;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		GameRandomManager gameRandomManager = eve.gameState?.randomManager;
		if (gameRandomManager == null)
		{
			return null;
		}
		List<Item> targetItems = poolSelector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		if (targetItems.Count == 0)
		{
			return new List<Item>();
		}
		int num = 1;
		if (countGenerator != null)
		{
			int? num2 = countGenerator.GenerateValue(eve);
			if (!num2.HasValue)
			{
				return null;
			}
			num = num2.Value;
		}
		eve.amount = num;
		if (excludeSelf)
		{
			targetItems.Remove(eve.CasterItem);
		}
		if (num >= targetItems.Count)
		{
			if (repeatCountIfTooMuch)
			{
				return targetItems.SelectRandomCount(num, gameRandomManager.NextDouble);
			}
			return targetItems;
		}
		return targetItems.SelectRandomCountUnique(num, gameRandomManager.NextDouble);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			times = new NodeDisplayInfo.Element<int?>(countGenerator?.GenerateValue(eve))
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { poolSelector, countGenerator };
	}
}
