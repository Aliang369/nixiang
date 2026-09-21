using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterMastery : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber, IAssetIcon
{
	public abstract class MasteryNode : IEffectNode, ITextDisplay, IChildren<IEffectNode>
	{
		public abstract void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null);

		public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return null;
		}

		public virtual List<IEffectNode> GetChildren()
		{
			return new List<IEffectNode>();
		}
	}

	public class SequenceNode : MasteryNode
	{
		public List<MasteryNode> nodes;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			foreach (MasteryNode node in nodes)
			{
				node.Modify(eve, config, applicationState);
			}
		}

		public override List<IEffectNode> GetChildren()
		{
			return nodes.ConvertAll((Converter<MasteryNode, IEffectNode>)((MasteryNode n) => n));
		}
	}

	public class ReplaceItem : MasteryNode
	{
		public int count;

		public ItemDefGenerator sourceItemGenerator;

		public NewItemGenerator replaceItemGenerator;

		public bool isResetCounter;

		public int resetCounter;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			ItemDefinition sourceItem = sourceItemGenerator.GenerateSingleItemDef(eve);
			if (sourceItem == null)
			{
				return;
			}
			for (int i = 0; i < count; i++)
			{
				int num = config.initDeck.FindIndex((Item it) => it.GetDef() == sourceItem);
				if (num < 0)
				{
					continue;
				}
				List<Item> list = replaceItemGenerator.GenerateNewItems(eve);
				if (!list.IsNullOrEmpty() && list.Count == 1)
				{
					Item item = list[0];
					if (isResetCounter)
					{
						item.counter = resetCounter;
					}
					config.initDeck[num] = item;
				}
			}
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				value = new NodeDisplayInfo.Element<int?>(count),
				baseValue = new NodeDisplayInfo.Element<int?>(resetCounter),
				outputBool = new NodeDisplayInfo.Element<bool?>(isResetCounter)
			};
		}

		public override List<IEffectNode> GetChildren()
		{
			return new List<IEffectNode> { sourceItemGenerator, replaceItemGenerator };
		}
	}

	public class ReplaceAllItems : MasteryNode
	{
		public ItemDefGenerator sourceItemGenerator;

		public NewItemGenerator replaceItemGenerator;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			ItemDefinition itemDefinition = sourceItemGenerator?.GenerateSingleItemDef(eve);
			if (itemDefinition == null || config?.initDeck == null)
			{
				return;
			}
			for (int i = 0; i < config.initDeck.Count; i++)
			{
				if (!(config.initDeck[i]?.GetDef() != itemDefinition))
				{
					List<Item> list = replaceItemGenerator?.GenerateNewItems(eve);
					if (!list.IsNullOrEmpty() && list.Count == 1)
					{
						config.initDeck[i] = list[0];
					}
				}
			}
		}

		public override List<IEffectNode> GetChildren()
		{
			return new List<IEffectNode> { sourceItemGenerator, replaceItemGenerator };
		}
	}

	public class ReplaceAll : MasteryNode
	{
		public List<ItemDefinition> newItem;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			config.initDeck = new List<Item>();
			for (int i = 0; i < newItem.Count; i++)
			{
				config.initDeck.Add(new Item(newItem[i]));
			}
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				refItems = new NodeDisplayInfo.Element<List<Item>>(newItem.ConvertAll((ItemDefinition d) => new Item(d)))
			};
		}
	}

	public class ReplaceItemsWithSingleItem : MasteryNode
	{
		public int sourceCount;

		public ItemDefinition source;

		public ItemDefinition replacement;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			if (sourceCount <= 0 || source == null || replacement == null || config?.initDeck == null)
			{
				return;
			}
			List<int> list = (from entry in (from entry in config.initDeck.Select((Item item, int index) => new { item, index })
					where entry.item?.GetDef() == source
					select entry).Take(sourceCount)
				select entry.index).ToList();
			if (list.Count == sourceCount)
			{
				config.initDeck[list[0]] = new Item(replacement);
				for (int num = list.Count - 1; num >= 1; num--)
				{
					config.initDeck.RemoveAt(list[num]);
				}
			}
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				value = new NodeDisplayInfo.Element<int?>(sourceCount),
				refItems = new NodeDisplayInfo.Element<List<Item>>(new List<Item>
				{
					new Item(replacement)
				}),
				refItemDefs = new NodeDisplayInfo.Element<List<ItemDefinition>>(new List<ItemDefinition> { replacement })
			};
		}
	}

	public class AddItem : MasteryNode
	{
		public int count;

		public NewItemGenerator newItemGenerator;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			for (int i = 0; i < count; i++)
			{
				List<Item> list = newItemGenerator.GenerateNewItems(eve);
				if (!list.IsNullOrEmpty() && list.Count == 1)
				{
					Item item = list[0];
					config.initDeck.Add(item);
				}
			}
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				value = new NodeDisplayInfo.Element<int?>(count)
			};
		}

		public override List<IEffectNode> GetChildren()
		{
			return new List<IEffectNode> { newItemGenerator };
		}
	}

	public class UpgradeItem : MasteryNode
	{
		public int count;

		public ItemDefinition target;

		public bool displayUpgradedItem;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			foreach (Item item in config.initDeck.FindAll((Item it) => it.GetDef() == target).Take(count))
			{
				item.Upgrade();
			}
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			Item item = new Item(target);
			if (displayUpgradedItem)
			{
				item.Upgrade();
			}
			return new NodeDisplayInfo
			{
				value = new NodeDisplayInfo.Element<int?>(count),
				refItems = new NodeDisplayInfo.Element<List<Item>>(new List<Item> { item })
			};
		}
	}

	public class UpgradeTaggedItems : MasteryNode
	{
		public ItemTag targetTag;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			if (targetTag == null || config?.initDeck == null)
			{
				return;
			}
			foreach (Item item in config.initDeck)
			{
				if (item != null && item.HasTag(targetTag) && item.Upgradeable)
				{
					item.Upgrade();
				}
			}
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				refTags = new NodeDisplayInfo.Element<List<ItemTag>>(new List<ItemTag> { targetTag })
			};
		}
	}

	public class ChangeItemCost : MasteryNode
	{
		public int count;

		public ItemDefinition target;

		public int delta;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			if (count <= 0 || target == null || config?.initDeck == null)
			{
				return;
			}
			foreach (Item item in config.initDeck.Where((Item item) => item?.GetDef() == target).Take(count))
			{
				item.deltaCost = (int)item.deltaCost + delta;
			}
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				value = new NodeDisplayInfo.Element<int?>(delta),
				baseValue = new NodeDisplayInfo.Element<int?>(count),
				refItems = new NodeDisplayInfo.Element<List<Item>>(new List<Item>
				{
					new Item(target)
				}),
				refItemDefs = new NodeDisplayInfo.Element<List<ItemDefinition>>(new List<ItemDefinition> { target })
			};
		}
	}

	public class ChangeItemCounter : MasteryNode
	{
		public int count;

		public int delta;

		public ItemDefinition target;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			foreach (Item item in config.initDeck.FindAll((Item it) => it.GetDef() == target).Take(count))
			{
				item.counter = (int)item.counter + delta;
			}
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				value = new NodeDisplayInfo.Element<int?>(delta),
				refItems = new NodeDisplayInfo.Element<List<Item>>(new List<Item>
				{
					new Item(target)
				}),
				refItemDefs = new NodeDisplayInfo.Element<List<ItemDefinition>>(new List<ItemDefinition> { target })
			};
		}
	}

	public class ChangeInitDeckTaggedMaxCounterNode : MasteryNode
	{
		public ItemTag targetTag;

		public int delta;

		public bool baseAdd;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			if (targetTag == null || config?.initDeck == null)
			{
				return;
			}
			foreach (Item item in config.initDeck)
			{
				if (item != null && item.HasTag(targetTag))
				{
					item.maxCounter = (int)item.maxCounter + delta;
					item.resetInitMaxCounter = (int)item.resetInitMaxCounter + delta;
					if (baseAdd)
					{
						item.counter = (int)item.counter + delta;
						item.resetInitCounter = (int)item.resetInitCounter + delta;
					}
				}
			}
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				value = new NodeDisplayInfo.Element<int?>(delta),
				outputBool = new NodeDisplayInfo.Element<bool?>(baseAdd),
				refTags = new NodeDisplayInfo.Element<List<ItemTag>>(new List<ItemTag> { targetTag })
			};
		}
	}

	public class SetItemCounter : MasteryNode
	{
		public int count;

		public ItemDefinition target;

		public int value;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			foreach (Item item in config.initDeck.FindAll((Item it) => it.GetDef() == target).Take(count))
			{
				item.counter = value;
			}
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				value = new NodeDisplayInfo.Element<int?>(value),
				baseValue = new NodeDisplayInfo.Element<int?>(count),
				refItems = new NodeDisplayInfo.Element<List<Item>>(new List<Item>
				{
					new Item(target)
				}),
				refItemDefs = new NodeDisplayInfo.Element<List<ItemDefinition>>(new List<ItemDefinition> { target })
			};
		}
	}

	public class AttachRuneNode : MasteryNode
	{
		public int count;

		public RuneDef rune;

		public int stack;

		public ItemDefinition target;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			foreach (Item item in config.initDeck.FindAll((Item it) => it.GetDef() == target).Take(count))
			{
				item.AddRune(new Rune(rune, stack));
			}
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				value = new NodeDisplayInfo.Element<int?>(stack),
				refItems = new NodeDisplayInfo.Element<List<Item>>(new List<Item>
				{
					new Item(target)
				}),
				refRunes = new NodeDisplayInfo.Element<List<Rune>>(new List<Rune>
				{
					new Rune(rune, stack)
				})
			};
		}
	}

	public class AttachEnchantNode : MasteryNode
	{
		public EnchantDef enchant;

		public int stack;

		public ItemDefinition target;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			foreach (Item item in config.initDeck.FindAll((Item it) => it.GetDef() == target))
			{
				item.AddEnchant(new Enchant(enchant, stack));
			}
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				refItems = new NodeDisplayInfo.Element<List<Item>>(new List<Item>
				{
					new Item(target)
				}),
				refEnchants = new NodeDisplayInfo.Element<List<Enchant>>(new List<Enchant>
				{
					new Enchant(enchant, stack)
				})
			};
		}
	}

	public class ChangeCharacterHealth : MasteryNode
	{
		public int delta;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			config.initHealth += delta;
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				value = new NodeDisplayInfo.Element<int?>(delta)
			};
		}
	}

	public class OverrideYinYang : MasteryNode
	{
		public int yang;

		public int yin;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
			config.overrideYin = yin;
			config.overrideYan = yang;
		}

		public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return new NodeDisplayInfo
			{
				value = new NodeDisplayInfo.Element<int?>(yang),
				baseValue = new NodeDisplayInfo.Element<int?>(yin)
			};
		}
	}

	public class DisplayOnlyItemNode : MasteryNode
	{
		public NewItemGenerator item;

		public override void Modify(GameEvent eve, CharacterGameInitConfig config, ApplicationStateInfo applicationState = null)
		{
		}

		public override List<IEffectNode> GetChildren()
		{
			return new List<IEffectNode> { item };
		}
	}

	[CTSOAssetName]
	[LocalizeText("专精名称")]
	public string masteryName;

	[CTSOAssetName]
	[LocalizeText("专精描述")]
	public string description;

	public List<FeatureInfoData> features;

	public Sprite icon;

	public PlayerCharacterDef attachCharacter;

	public MasteryNode node;

	public List<Ability> abilities;

	public override string CTAssetIDPrepend => "CHADMAS";

	public Sprite IconAsset => icon;

	public Vector2Int IconPreferSize_Editor => new Vector2Int(128, 128);

	public string GetDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return TextDataPreset.FormatEffectObjectDescription(this, description, applicationState, out generator);
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		list.AddRange(abilities.OrEmptyIfNull().Select((Func<Ability, IEffectNode>)((Ability x) => x)).ToList());
		list.Add(node);
		return list;
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		return abilities.OrEmptyIfNull().SelectMany((Ability x) => x.GetAllSubscribeGameEventType()).Distinct();
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		return abilities.OrEmptyIfNull().Any((Ability x) => x.CheckTriggerGameEventAble(eve));
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		foreach (Ability ability in abilities)
		{
			ability.TriggerGameEvent(eve);
		}
	}
}
