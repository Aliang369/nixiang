using System;
using System.Collections.Generic;
using System.Linq;

public class SpawnItems : Effect
{
	public enum InsertType
	{
		InsertToHead,
		InsertToEnd,
		FixedIndex,
		PivotIndex,
		RandomInsert
	}

	public NewItemGenerator itemGenerator;

	public GameEvent.EventBFTType targetBFT;

	public GameEvent.ContainerType targetContainerType;

	public ValueGenerator deltaInitCounterGenerator;

	public ValueGenerator repeatAddTimesGenerator;

	public ValueGenerator addSlotIndexGenerator;

	public InsertType insertType;

	public ItemTargetSelector insertPivot;

	public HitNFXConfig hitEffect;

	public HitNFX3DConfig hitEffect3D;

	public TransformNFX3DConfig transformEffect3D;

	public bool withoutTempItemEnchant;

	public bool suppressAfterTriggerChapterEffectsUntilQueueClean;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (itemGenerator == null)
		{
			return NodeCastResult.Failure;
		}
		AddSpawnItemsInfo addInfo = GetAddInfo(eve, real: true);
		for (int i = 0; i < addInfo.repeatTimes; i++)
		{
			foreach (AddSpawnItemsInfo.AddSingleItemInfo item3 in addInfo.itemInfoEachTime)
			{
				ItemContainer targetContainer = eve.GetOwnerContainer(targetBFT, targetContainerType);
				Item item = item3.GenerateFinalItem(addInfo).Copy();
				GameEvent eveCopy = eve.ShallowCopy();
				int fixedIndex = addSlotIndexGenerator?.GenerateValue(eveCopy) ?? (-1);
				Func<int> insertIndexer = delegate
				{
					switch (insertType)
					{
					case InsertType.InsertToHead:
						return 0;
					case InsertType.InsertToEnd:
						return -1;
					case InsertType.FixedIndex:
						return fixedIndex;
					case InsertType.PivotIndex:
					{
						Item item2 = insertPivot?.GetSingleItem(eveCopy);
						int valueOrDefault = (addSlotIndexGenerator?.GenerateValue(eveCopy)).GetValueOrDefault();
						if ((item2 == null || item2.owner != targetContainer) && eve.spellInfo?.triggerCastAnchorItem?.owner == targetContainer)
						{
							item2 = eve.spellInfo.triggerCastAnchorItem;
						}
						if (item2 == null || item2.owner != targetContainer)
						{
							return -1;
						}
						return (item2.ownerSlotIndex + valueOrDefault) ?? (-1);
					}
					case InsertType.RandomInsert:
						if (targetContainer.ItemsCount != 0)
						{
							return eve.gameState.randomManager.Next() % targetContainer.ItemsCount;
						}
						return 0;
					default:
						throw new ArgumentOutOfRangeException();
					}
				};
				if (!withoutTempItemEnchant && targetContainerType != GameEvent.ContainerType.DeckContainer)
				{
					AddSpawnItemEnchant(item);
				}
				transformEffect3D?.Play(eve, eve.GetEventBFT(GameEvent.EventBFTType.Owner), eve.GetEventBFT(targetBFT));
				Effect.EnqueueAction(eve, new SpawnItemAction(item, targetContainer, withAnim: true, eve.Caster, insertIndexer, suppressAfterTriggerChapterEffectsUntilQueueClean, eve.spellInfo));
				Effect.EnqueueAction(eve, new ActionAction(delegate
				{
					hitEffect?.Play(eve, item);
					hitEffect3D?.Play(eve, eve.GetEventBFT(targetBFT));
				}));
			}
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		if (itemGenerator == null)
		{
			return null;
		}
		AddSpawnItemsInfo addInfo = GetAddInfo(eve, real: false);
		List<Item> items = addInfo?.GenerateFinalItems();
		if (items == null)
		{
			return new NodeDisplayInfo
			{
				refItems = new NodeDisplayInfo.Element<List<Item>>(null),
				addItemInfo = new NodeDisplayInfo.Element<AddSpawnItemsInfo>(null),
				times = new NodeDisplayInfo.Element<int?>(null),
				deltaValue = new NodeDisplayInfo.Element<int?>(null)
			};
		}
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		nodeDisplayInfo.refItems = new NodeDisplayInfo.Element<List<Item>>(items);
		nodeDisplayInfo.addItemInfo = new NodeDisplayInfo.Element<AddSpawnItemsInfo>(addInfo);
		nodeDisplayInfo.times = new NodeDisplayInfo.Element<int?>(items.Count);
		nodeDisplayInfo.deltaValue = new NodeDisplayInfo.Element<int?>(addInfo.deltaCounter);
		if (items.Count > 0 && items[0].withCounter && items.TrueForAll((Item t) => (int)t.counter == (int)items[0].counter))
		{
			nodeDisplayInfo.value = new NodeDisplayInfo.Element<int?>(items[0].counter);
		}
		return nodeDisplayInfo;
	}

	private AddSpawnItemsInfo GetAddInfo(GameEvent eve, bool real)
	{
		List<Item> list = itemGenerator.GenerateNewItems(eve);
		List<AddSpawnItemsInfo.AddSingleItemInfo> list2 = null;
		if (list != null)
		{
			list2 = list.ConvertAll((Item i) => new AddSpawnItemsInfo.AddSingleItemInfo(i));
		}
		int addTimes = GetAddTimes(eve);
		AddSpawnItemsInfo addSpawnItemsInfo = new AddSpawnItemsInfo
		{
			sourceItem = eve.CasterItem,
			sourceSpellInfo = eve.spellInfo,
			repeatTimes = addTimes,
			deltaCounter = GetAddDeltaCounter(eve),
			itemInfoEachTime = list2
		};
		if (list == null)
		{
			return addSpawnItemsInfo;
		}
		GameEvent.ResolveInfo resolveInfo = eve.gameState?.gameEventManager?.TriggerAddItemCounterResolveInfo(eve.CasterItem, list2.ConvertAll((AddSpawnItemsInfo.AddSingleItemInfo i) => i.item), GameEvent.TriggerType.Real, eve.spellInfo);
		if (resolveInfo != null)
		{
			addSpawnItemsInfo.repeatTimes += resolveInfo.deltaTimes;
			addSpawnItemsInfo.deltaCounter += resolveInfo.deltaValue;
			addSpawnItemsInfo.deltaPercentCounter += resolveInfo.deltaPercent;
			for (int num = 0; num < addSpawnItemsInfo.itemInfoEachTime.Count; num++)
			{
				AddSpawnItemsInfo.AddSingleItemInfo iiet = addSpawnItemsInfo.itemInfoEachTime[num];
				int item = resolveInfo.itemValuePairs.FindOrDefault(((Item, int) iv) => iv.Item1 == iiet.item, (null, 0)).Item2;
				iiet.deltaCounter += item;
				addSpawnItemsInfo.itemInfoEachTime[num] = iiet;
			}
		}
		return addSpawnItemsInfo;
	}

	private int GetAddTimes(GameEvent eve)
	{
		return repeatAddTimesGenerator?.GenerateValue(eve) ?? 1;
	}

	private int GetAddDeltaCounter(GameEvent eve)
	{
		int num = 0;
		if (deltaInitCounterGenerator != null)
		{
			num += (deltaInitCounterGenerator?.GenerateValue(eve)).GetValueOrDefault();
		}
		return num;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemGenerator, repeatAddTimesGenerator, deltaInitCounterGenerator, addSlotIndexGenerator, insertPivot };
	}

	internal static void AddSpawnItemEnchant(Item item)
	{
		EnchantDef enchantDef = ScriptableObjectSingleton<GameDataManager>.instance.spawnItemEnchantDef;
		if (item != null && !(enchantDef == null) && !item.Enchants.Any((Enchant enchant) => enchant?.def == enchantDef))
		{
			item.AddEnchant(new Enchant(enchantDef));
		}
	}
}
