using System;
using System.Collections.Generic;
using System.Linq;

public class SpawnRandomChapterAndAdvanceEffect : Effect
{
	public NewItemGenerator itemGenerator;

	public ItemTagGenerator chapterTagGenerator;

	public ValueGenerator advanceCountGenerator;

	public bool insertLeftOfSource;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		ItemContainer targetContainer = eve.GetOwnerContainer(GameEvent.EventBFTType.Owner, GameEvent.ContainerType.TableContainer);
		List<Item> source = itemGenerator?.GenerateNewItems(eve);
		ItemTag chapterTag = chapterTagGenerator?.GenerateSingleItemTag(eve);
		int advanceCount = (advanceCountGenerator?.GenerateValue(eve)).GetValueOrDefault();
		if (targetContainer == null || source.IsNullOrEmpty() || chapterTag == null || advanceCount <= 0)
		{
			return NodeCastResult.Failure;
		}
		Item template = source.FirstOrDefault((Item item3) => item3 != null);
		if (template == null)
		{
			return NodeCastResult.Failure;
		}
		AddSpawnItemsInfo addSpawnItemsInfo = new AddSpawnItemsInfo
		{
			sourceItem = eve.CasterItem,
			sourceSpellInfo = eve.spellInfo,
			repeatTimes = 1,
			itemInfoEachTime = new List<AddSpawnItemsInfo.AddSingleItemInfo>
			{
				new AddSpawnItemsInfo.AddSingleItemInfo(template)
			}
		};
		GameEvent.ResolveInfo resolveInfo = eve.gameState?.gameEventManager?.TriggerAddItemCounterResolveInfo(eve.CasterItem, new List<Item> { template }, GameEvent.TriggerType.Real, eve.spellInfo);
		if (resolveInfo != null)
		{
			addSpawnItemsInfo.repeatTimes += resolveInfo.deltaTimes;
			addSpawnItemsInfo.deltaCounter += resolveInfo.deltaValue;
			addSpawnItemsInfo.deltaPercentCounter += resolveInfo.deltaPercent;
			int item = resolveInfo.itemValuePairs.FindOrDefault(((Item, int) pair) => pair.Item1 == template, (null, 0)).Item2;
			addSpawnItemsInfo.itemInfoEachTime[0] = new AddSpawnItemsInfo.AddSingleItemInfo(template, item);
		}
		if (addSpawnItemsInfo.repeatTimes <= 0)
		{
			return NodeCastResult.Success;
		}
		Item item2 = addSpawnItemsInfo.itemInfoEachTime[0].GenerateFinalItem(addSpawnItemsInfo);
		SpawnItems.AddSpawnItemEnchant(item2);
		Func<int> insertIndexer = delegate
		{
			if (insertLeftOfSource)
			{
				int? num = eve.CasterItem?.ownerSlotIndex;
				if (num.HasValue)
				{
					return num.GetValueOrDefault();
				}
			}
			return 0;
		};
		Effect.EnqueueAction(eve, new SpawnItemAction(item2, targetContainer, withAnim: true, eve.Caster, insertIndexer, suppressAfterTriggerChapterEffectsUntilQueueClean: false, eve.spellInfo, delegate(Item spawnedItem)
		{
			if (spawnedItem != null)
			{
				eve.gameState.actionManager.EnqueueAction(new AdvanceGeneratedChapterAction(spawnedItem, eve.CasterItem, targetContainer, chapterTag, advanceCount));
			}
		}));
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo obj = itemGenerator?.GetNodeDisplayInfo(eve) ?? new NodeDisplayInfo();
		obj.value = new NodeDisplayInfo.Element<int?>(advanceCountGenerator?.GenerateValue(eve));
		obj.refTags = new NodeDisplayInfo.Element<List<ItemTag>>(chapterTagGenerator?.GenerateItemTags(eve));
		return obj;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemGenerator, chapterTagGenerator, advanceCountGenerator };
	}
}
