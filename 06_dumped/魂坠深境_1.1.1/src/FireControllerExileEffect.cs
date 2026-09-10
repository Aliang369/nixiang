using System;
using System.Collections.Generic;
using System.Linq;

public abstract class FireControllerExileEffect : Effect
{
	protected const string FireTagGuid = "77c86291a66effd46854b42f82dd32a3";

	protected const string BurstEnchantGuid = "a031fadc9104de2459ca595278678b17";

	protected const string FireSeedEnchantGuid = "97fb47d81f913754aa2ac35e19f09a57";

	private const string FireballGuid = "25ab7c26789d51a43b494f2d70dd04d3";

	private const string BlazingFireballGuid = "559f98003b630184eabad16e3d1d4fc7";

	private const string BacklashFireballGuid = "ca8f1b8a1560ae342a715287fda1a230";

	protected const string FireBoltGuid = "a47f2520cd3271e4d84166493bb5170c";

	protected const string SwiftEnchantGuid = "996afab9e52a8c54e8efbd04e8aab0cb";

	protected const string ScorchGuid = "bd9720a5357cecc49af317cfdc8d9032";

	protected static BattleFieldTransform GetCasterBft(GameEvent eve)
	{
		object obj = eve.CasterOwner;
		if (obj == null)
		{
			obj = eve.OriginContainer?.OwnerBft;
			if (obj == null)
			{
				obj = eve.TargetOwner;
				if (obj == null)
				{
					SpellCastInfo spellInfo = eve.spellInfo;
					if (spellInfo == null)
					{
						return null;
					}
					Item spellItem = spellInfo.spellItem;
					if (spellItem == null)
					{
						return null;
					}
					obj = spellItem.OwnerBft;
				}
			}
		}
		return (BattleFieldTransform)obj;
	}

	protected static List<Item> GetTableItems(GameEvent eve)
	{
		return GetCasterBft(eve)?.tableContainer?.GetAllItems() ?? new List<Item>();
	}

	protected static List<Item> GetExilePileItems(GameEvent eve)
	{
		return GetCasterBft(eve)?.exilePileItemContainer?.GetAllItems() ?? new List<Item>();
	}

	protected static List<Item> GetTableAndExilePileItems(GameEvent eve)
	{
		List<Item> list = new List<Item>();
		list.AddRange(GetTableItems(eve));
		list.AddRange(GetExilePileItems(eve));
		return list;
	}

	protected static bool IsFireItem(Item item)
	{
		if (item == null)
		{
			return false;
		}
		return item.def?.tags?.Any((ItemTag tag) => tag?.id == "77c86291a66effd46854b42f82dd32a3") == true;
	}

	protected static bool IsFireAttackItem(Item item)
	{
		if (IsFireItem(item))
		{
			ItemDefinition def = item.def;
			if ((object)def == null)
			{
				return false;
			}
			return def.tags?.Contains(ScriptableObjectSingleton<GameDataManager>.instance.attackTag) == true;
		}
		return false;
	}

	protected static bool IsFireballFamily(Item item)
	{
		string text = item?.def?.id;
		if (!(text == "25ab7c26789d51a43b494f2d70dd04d3") && !(text == "559f98003b630184eabad16e3d1d4fc7"))
		{
			return text == "ca8f1b8a1560ae342a715287fda1a230";
		}
		return true;
	}

	protected static bool IsSkinBranch(GameEvent eve)
	{
		return eve.CasterItem?.CurrentItemSkinConfig != null;
	}

	protected static int GetTierValue(GameEvent eve, int normalValue, int upgradeValue, int? skinValue = null)
	{
		if (IsSkinBranch(eve) && skinValue.HasValue)
		{
			return skinValue.Value;
		}
		Item casterItem = eve.CasterItem;
		if (casterItem == null || !casterItem.upgraded)
		{
			return normalValue;
		}
		return upgradeValue;
	}

	protected static int UseConfiguredOrDefault(int configuredValue, int defaultValue)
	{
		if (configuredValue == 0)
		{
			return defaultValue;
		}
		return configuredValue;
	}

	protected static ItemDefinition GetItemDef(GameEvent eve, string guid)
	{
		return eve.gameState?.applicationState?.assetCatalog?.itemDefCatalog?.GetAsset(guid);
	}

	protected static EnchantDef GetEnchantDef(GameEvent eve, string guid)
	{
		return eve.gameState?.applicationState?.assetCatalog?.enchantDefCatalog?.GetAsset(guid);
	}

	protected static Enchant GetBurstEnchant(Item item)
	{
		return item?.Enchants?.FirstOrDefault((Enchant enchant) => enchant?.def?.id == "a031fadc9104de2459ca595278678b17" && (int)enchant.value > 0);
	}

	protected static void TriggerItemSpell(GameEvent eve, Item item)
	{
		Effect.EnqueueAction(eve, new TriggerCastItemAction(item, eve.CasterItem, TriggerTargetSpell.TargetingType.Default, autoTargetForPlayer: true, useSourceAsTriggerAnchor: true, suppressRemoveAfterCast: true));
	}

	protected static void DealDamageToOpponent(GameEvent eve, int damage, object source = null)
	{
		DealDamageToOpponent(eve, damage, 1, source);
	}

	protected static void DealDamageToOpponent(GameEvent eve, int damage, int baseTimes, object source = null)
	{
		DealDamageToOpponent(eve, new DamageCommand
		{
			baseDamage = damage,
			source = (source ?? eve.CasterItem),
			targetBFT = eve.OpponentBFT,
			overflow = true,
			spellCastInfo = eve.spellInfo
		}, baseTimes);
	}

	protected static void DealDamageToOpponent(GameEvent eve, DamageCommand damageCommand, int baseTimes = 1)
	{
		if (damageCommand != null && damageCommand.baseDamage > 0 && damageCommand.targetBFT != null)
		{
			int finalDamageTimes = GetFinalDamageTimes(eve, damageCommand, baseTimes);
			for (int i = 0; i < finalDamageTimes; i++)
			{
				Effect.EnqueueAction(eve, new DamageAction(CopyDamageCommand(damageCommand)));
			}
		}
	}

	private static int GetFinalDamageTimes(GameEvent eve, DamageCommand damageCommand, int baseTimes)
	{
		baseTimes = Math.Max(0, baseTimes);
		if (eve?.gameState?.gameEventManager == null)
		{
			return DamageAction.ClampBatchCount(baseTimes);
		}
		eve.damagecommand = damageCommand;
		GameEvent.ResolveInfo resolveInfo = eve.gameState.gameEventManager.TriggerGenerateDamageResolveInfo(damageCommand, eve.triggerType, Damage.ResolveType.DamageTimes, eve.TargetItem);
		return DamageAction.ClampBatchCount((long)baseTimes + (long)resolveInfo.deltaTimes);
	}

	private static DamageCommand CopyDamageCommand(DamageCommand source)
	{
		return new DamageCommand
		{
			baseDamage = source.baseDamage,
			lastDamageInfo = source.lastDamageInfo,
			overflow = source.overflow,
			pierce = source.pierce,
			source = source.source,
			sourceOwnerOverride = source.sourceOwnerOverride,
			spellCastInfo = source.spellCastInfo,
			targetBFT = source.targetBFT,
			transformSetup = source.transformSetup,
			transform3DSetup = source.transform3DSetup,
			hitSetup = source.hitSetup,
			hit3DSetup = source.hit3DSetup,
			persistentHit3DSetup = source.persistentHit3DSetup
		};
	}

	protected static void AddEnchantToItem(GameEvent eve, Item target, string enchantGuid, int delta)
	{
		EnchantDef enchantDef = GetEnchantDef(eve, enchantGuid);
		if (target != null && !(enchantDef == null) && delta != 0)
		{
			Effect.EnqueueAction(eve, new ChangeEnchantAction(target, eve.CasterItem, enchantDef, delta));
		}
	}

	protected static void SpawnGeneratedItem(GameEvent eve, Item item, ItemContainer targetContainer, Func<int> insertIndexer)
	{
		if (item != null && targetContainer != null)
		{
			if (targetContainer.type != ItemContainer.ContainerType.Deck)
			{
				AddSpawnItemEnchant(item);
			}
			Effect.EnqueueAction(eve, new SpawnItemAction(item, targetContainer, withAnim: true, eve.CasterItem, insertIndexer));
		}
	}

	private static void AddSpawnItemEnchant(Item item)
	{
		EnchantDef enchantDef = ScriptableObjectSingleton<GameDataManager>.instance.spawnItemEnchantDef;
		if (item != null && !(enchantDef == null) && !item.Enchants.Any((Enchant enchant) => enchant?.def == enchantDef))
		{
			item.AddEnchant(new Enchant(enchantDef));
		}
	}

	protected static int GetPreviewDamage(GameEvent eve, int baseDamage)
	{
		BattleFieldTransform eventBFT = eve.GetEventBFT(GameEvent.EventBFTType.Opponent);
		if (eventBFT == null || eve.gameState == null)
		{
			return baseDamage;
		}
		return Damage.GenerateDamage(new DamageCommand
		{
			baseDamage = baseDamage,
			source = eve.CasterItem,
			targetBFT = eventBFT,
			overflow = true,
			spellCastInfo = eve.spellInfo
		}, eve.gameState, eve.triggerType)?.castDamage ?? baseDamage;
	}

	protected static NodeDisplayInfo DamageDisplay(GameEvent eve, int baseDamage, int times = 1)
	{
		int previewDamage = GetPreviewDamage(eve, baseDamage);
		DamageCommand damageCommand = new DamageCommand
		{
			baseDamage = baseDamage,
			source = eve.CasterItem,
			targetBFT = eve.OpponentBFT,
			overflow = true,
			spellCastInfo = eve.spellInfo
		};
		int finalDamageTimes = GetFinalDamageTimes(eve, damageCommand, times);
		return new NodeDisplayInfo
		{
			baseValue = new NodeDisplayInfo.Element<int?>(baseDamage),
			deltaValue = new NodeDisplayInfo.Element<int?>(previewDamage - baseDamage),
			value = new NodeDisplayInfo.Element<int?>(previewDamage),
			times = new NodeDisplayInfo.Element<int?>(finalDamageTimes),
			deltaTimes = new NodeDisplayInfo.Element<int?>(finalDamageTimes - times),
			damage = new NodeDisplayInfo.Element<bool>(value: true)
		};
	}

	protected static List<Enchant> EnchantDisplay(GameEvent eve, string enchantGuid, int value)
	{
		EnchantDef enchantDef = GetEnchantDef(eve, enchantGuid);
		if (!(enchantDef == null))
		{
			return new List<Enchant>
			{
				new Enchant(enchantDef, value)
			};
		}
		return null;
	}

	protected static AddSpawnItemsInfo AddItemDisplay(GameEvent eve, Item item, int repeatTimes = 1)
	{
		if (item == null)
		{
			return null;
		}
		return new AddSpawnItemsInfo
		{
			sourceItem = eve.CasterItem,
			sourceSpellInfo = eve.spellInfo,
			repeatTimes = repeatTimes,
			itemInfoEachTime = new List<AddSpawnItemsInfo.AddSingleItemInfo>
			{
				new AddSpawnItemsInfo.AddSingleItemInfo(item)
			}
		};
	}

	protected static Item CreateDisplayItem(GameEvent eve, string itemGuid, int counter = 0, int upgradeLevel = 0)
	{
		ItemDefinition itemDef = GetItemDef(eve, itemGuid);
		if (itemDef == null)
		{
			return null;
		}
		Item item = new Item(itemDef)
		{
			upgradeLevel = upgradeLevel
		};
		if (item.withCounter)
		{
			item.counter = counter;
		}
		return item;
	}
}
