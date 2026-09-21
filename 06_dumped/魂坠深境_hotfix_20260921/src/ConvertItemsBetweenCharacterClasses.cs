using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class ConvertItemsBetweenCharacterClasses : Effect
{
	public ItemTargetSelector targetItemSelector;

	public CharacterClassDef classA;

	public CharacterClassDef classB;

	public int coinPerConvertedItem;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (targetItemSelector == null || classA == null || classB == null || eve.gameState?.applicationState == null)
		{
			return NodeCastResult.Failure;
		}
		List<Item> targetItems = targetItemSelector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return NodeCastResult.Failure;
		}
		int num = 0;
		foreach (Item item in targetItems.Where((Item item) => item != null).Distinct())
		{
			CharacterClassDef oppositeClass = GetOppositeClass(item.GetDef(), classA, classB);
			if (!(oppositeClass == null))
			{
				ItemDefinition itemDefinition = SelectConversionDefinition(item.GetDef(), oppositeClass.GetPoolItems(eve.gameState.applicationState), (eve.gameState.randomManager == null) ? null : new Func<double>(eve.gameState.randomManager.NextDouble));
				if (!(itemDefinition == null))
				{
					Effect.EnqueueAction(eve, new ConvertItemPreserveProgressAction(item, itemDefinition));
					num++;
				}
			}
		}
		if (num == 0)
		{
			return NodeCastResult.Failure;
		}
		if (coinPerConvertedItem != 0)
		{
			Effect.EnqueueAction(eve, new ChangeCoinAction(coinPerConvertedItem * num));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetItemSelector };
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(coinPerConvertedItem)
		};
	}

	public static CharacterClassDef GetOppositeClass(ItemDefinition sourceDefinition, CharacterClassDef classA, CharacterClassDef classB)
	{
		List<CharacterClassType> list = sourceDefinition?.generationInfo?.attachClass;
		if (list == null || classA == null || classB == null)
		{
			return null;
		}
		bool flag = list.Contains(classA.mClass);
		bool flag2 = list.Contains(classB.mClass);
		if (flag == flag2)
		{
			return null;
		}
		if (!flag)
		{
			return classA;
		}
		return classB;
	}

	public static ItemDefinition SelectConversionDefinition(ItemDefinition sourceDefinition, IEnumerable<ItemDefinition> targetPool, Func<double> nextRandom)
	{
		if (sourceDefinition == null || targetPool == null)
		{
			return null;
		}
		List<ItemDefinition> list = targetPool.Where((ItemDefinition definition) => definition != null).Distinct().ToList();
		if (list.Count == 0)
		{
			return null;
		}
		List<QualityLevel> qualityPriority = GetQualityPriority(sourceDefinition.qualityLevel);
		ItemTag sourceType = sourceDefinition.GetMajorTag();
		if (sourceType != null)
		{
			foreach (QualityLevel quality in qualityPriority)
			{
				List<ItemDefinition> list2 = list.Where((ItemDefinition definition) => definition.qualityLevel == quality && definition.GetMajorTag() == sourceType).ToList();
				if (list2.Count > 0)
				{
					return SelectRandom(list2, nextRandom);
				}
			}
		}
		foreach (QualityLevel quality2 in qualityPriority)
		{
			List<ItemDefinition> list3 = list.Where((ItemDefinition definition) => definition.qualityLevel == quality2).ToList();
			if (list3.Count > 0)
			{
				return SelectRandom(list3, nextRandom);
			}
		}
		return null;
	}

	public static List<QualityLevel> GetQualityPriority(QualityLevel sourceQuality)
	{
		List<QualityLevel> list = new List<QualityLevel> { sourceQuality };
		switch (sourceQuality)
		{
		case QualityLevel.Base:
			list.Add(QualityLevel.Common);
			break;
		case QualityLevel.Epic:
			list.Add(QualityLevel.Rare);
			break;
		}
		return list;
	}

	private static ItemDefinition SelectRandom(List<ItemDefinition> candidates, Func<double> nextRandom)
	{
		if (candidates.Count != 1 && nextRandom != null)
		{
			return candidates.SelectRandom(nextRandom);
		}
		return candidates[0];
	}
}
