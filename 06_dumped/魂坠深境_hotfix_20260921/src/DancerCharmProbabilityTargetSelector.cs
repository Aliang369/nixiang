using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public sealed class DancerCharmProbabilityTargetSelector : ItemTargetSelector
{
	private const int DefaultEnemyProbability = 75;

	public ItemTargetSelector enemyPool;

	public ItemTargetSelector ownerPool;

	public ValueGenerator enemyProbabilityGenerator;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		if (eve?.gameState?.randomManager == null)
		{
			return null;
		}
		List<Item> list = enemyPool?.GetTargetItems(eve);
		List<Item> list2 = ownerPool?.GetTargetItems(eve);
		if (list == null || list2 == null)
		{
			return null;
		}
		list = list.Where((Item item2) => item2 != null).ToList();
		list2 = list2.Where((Item item2) => item2 != null).ToList();
		if (list.Count == 0 && list2.Count == 0)
		{
			return new List<Item>();
		}
		if (eve.IsDisplayEvent())
		{
			Item item = list.FirstOrDefault() ?? list2.FirstOrDefault();
			if (item != null)
			{
				return new List<Item> { item };
			}
			return new List<Item>();
		}
		int enemyProbability = GetEnemyProbability(eve);
		bool flag = eve.gameState.randomManager.Next(0, 100) < enemyProbability;
		List<Item> list3 = (flag ? list : list2);
		if (list3.Count == 0)
		{
			list3 = (flag ? list2 : list);
		}
		if (list3.Count == 0)
		{
			return new List<Item>();
		}
		return new List<Item> { list3.SelectRandom(eve.gameState.randomManager.NextDouble) };
	}

	private int GetEnemyProbability(GameEvent eve)
	{
		return Mathf.Clamp(enemyProbabilityGenerator?.GenerateValue(eve) ?? 75, 0, 100);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(GetEnemyProbability(eve))
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { enemyProbabilityGenerator, enemyPool, ownerPool };
	}
}
