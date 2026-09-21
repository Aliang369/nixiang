using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Item : IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber, IBftOwnable, PivotManager.IWithPivot
{
	public class ReplacementArmorSource
	{
		public Enchant enchant;

		public int armor;

		public ReplacementArmorAbility provider;
	}

	[SaveField(null, null)]
	public bool afterTurnEnd;

	[SaveField(null, null)]
	public bool battleJoined;

	[SaveField(null, null)]
	public ItemBlackBoard blackboard;

	[SaveField(null, null)]
	public ProtectedInt cooldown;

	[SaveField(null, null)]
	public ProtectedInt counter;

	[SaveField(null, null)]
	public ProtectedInt maxCounter;

	[SaveField(null, null)]
	public ProtectedInt resetInitCounter;

	[SaveField(null, null)]
	public ProtectedInt resetInitMaxCounter;

	[SaveField(null, null)]
	public int pendingChapterResetMaxCounterDelta;

	[SaveField(null, null)]
	public int currentSelectSpellIndex;

	[SaveField(null, null)]
	public ItemDefinition def;

	[SaveField(null, null)]
	public ProtectedInt deltaCD;

	[SaveField(null, null)]
	public ProtectedInt deltaCost;

	[SaveField(null, null)]
	private List<Enchant> enchants = new List<Enchant>();

	[SaveField(null, null)]
	public string indexOverrideOwnerName;

	[SaveField(null, null)]
	public bool transformActive;

	[SaveField(null, null)]
	public ItemSpell runtimeTransformSpellTemplate;

	[SaveField(null, null)]
	public string runtimeTransformSpellDescription;

	[SaveField(null, null)]
	public Item transformSpellSourceItem;

	[SaveField(null, null)]
	public bool transformHasRuntimeTemplate;

	[SaveField(null, null)]
	public bool removeAfterCast;

	[SaveField(null, null)]
	public ItemContainer owner;

	[SaveField(null, null)]
	private List<Rune> runes = new List<Rune>();

	[SaveField(null, null)]
	public int runeSlot;

	[SaveField(null, null)]
	public Item sourceDeckItem;

	[SaveField(null, null)]
	public Dictionary<string, object> tempValues = new Dictionary<string, object>();

	[SaveField(null, null)]
	public int treasureValue;

	[SaveField(null, null)]
	public int upgradeLevel;

	[SaveField(null, null)]
	public ItemSkinDef skin;

	[SaveField(null, null)]
	public ItemSkinBaseDef skinBase;

	[SaveField(null, null)]
	public List<ItemTagInstance> tagInstances;

	public static readonly Color defaultColor = Color.white;

	public bool HasPendingChapterResetEffect => pendingChapterResetMaxCounterDelta != 0;

	public ItemSkinBaseDef DisplaySkinBase => skinBase ?? ScriptableObjectSingleton<GameDataManager>.instance.defaultItemSkin;

	public IReadOnlyList<Enchant> Enchants => enchants;

	public IReadOnlyList<Rune> Runes => runes;

	public int CurrentSpellIndex
	{
		get
		{
			if (itemSpells != null && itemSpells.Count != 0)
			{
				return currentSelectSpellIndex % itemSpells.Count;
			}
			return 0;
		}
		set
		{
			currentSelectSpellIndex = Mathf.Abs(value);
		}
	}

	public ItemDefinition.ItemSkinConfig CurrentItemSkinConfig => def.GetItemSkinConfig(skin);

	public string CurrentSpellFormatDescription
	{
		get
		{
			ItemDefinition.ItemSkinConfig currentItemSkinConfig = CurrentItemSkinConfig;
			object obj;
			if (currentItemSkinConfig == null || !currentItemSkinConfig.removeSpell)
			{
				obj = CurrentItemSkinConfig?.GetSpellFormatDescription(upgraded);
				if (obj == null)
				{
					return def.spellFormtDescription;
				}
			}
			else
			{
				obj = null;
			}
			return (string)obj;
		}
	}

	public IOutGameEffectNode CurrentUpgradeEffectNode => CurrentItemSkinConfig?.upgradeEffect ?? def.upgradeEffect;

	public IOutGameEffectNode CurrentDowngradeEffectNode => CurrentItemSkinConfig?.downgradeEffect ?? def.downgradeEffect;

	public bool DamageAble
	{
		get
		{
			if (!HasBaseDamageableCounter)
			{
				return GetReplacementArmorValue() > 0;
			}
			return true;
		}
	}

	public bool Upgradeable
	{
		get
		{
			if (GetDef().upgradeable)
			{
				if (upgradeLevel != 0)
				{
					return GetDef().infiniteUpgrade;
				}
				return true;
			}
			return false;
		}
	}

	public QualityLevel qualityLevel => GetDef().qualityLevel;

	public bool upgraded => upgradeLevel > 0;

	public bool FromDeck => sourceDeckItem != null;

	public bool InvalidCounter => (int)counter < 0;

	public ItemTag majorTag => def.GetMajorTag(skin);

	public int? ownerSlotIndex
	{
		get
		{
			if (owner == null)
			{
				return null;
			}
			return owner.GetItemIndex(this);
		}
	}

	public bool withCounter => def.GetWithCounter(skin);

	public bool Health => def.Health;

	public bool Defence => def.Defence;

	public bool Companion => def.Companion;

	public bool DefenceCompanion => def.DefenceCompanion;

	public bool HasBaseDamageableCounter
	{
		get
		{
			if (withCounter)
			{
				if (!Defence)
				{
					if (Companion && DefenceCompanion)
					{
						ItemContainer itemContainer = owner;
						if (itemContainer == null)
						{
							return false;
						}
						return itemContainer.type == ItemContainer.ContainerType.Companion;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool HasReplacementDefence => GetReplacementArmorSources().Count > 0;

	public bool DefenceLike
	{
		get
		{
			if (!Defence)
			{
				if (Companion && DefenceCompanion)
				{
					ItemContainer itemContainer = owner;
					if (itemContainer != null && itemContainer.type == ItemContainer.ContainerType.Companion)
					{
						goto IL_0036;
					}
				}
				return HasReplacementDefence;
			}
			goto IL_0036;
			IL_0036:
			return true;
		}
	}

	public bool UsesDefenceCounterLimit
	{
		get
		{
			if (!Defence && !DefenceCompanion)
			{
				return HasReplacementDefence;
			}
			return true;
		}
	}

	public Sprite Icon => def.Icon;

	public string ItemName => def.itemName;

	public string ItemNameWithUpgrade => GetItemName();

	public bool IsCoolDowning => (int)cooldown > 0;

	public bool CoolDowned => !IsCoolDowning;

	public bool IsOverflow
	{
		get
		{
			if (owner != null)
			{
				return owner.ShouldOverflow(this);
			}
			return false;
		}
	}

	public bool HasSpell => CurrentSpell != null;

	public bool HasTransformTemplate
	{
		get
		{
			if (runtimeTransformSpellTemplate == null)
			{
				return def?.configuredTransformSpellTemplate != null;
			}
			return true;
		}
	}

	public ItemSpell BaseSpell
	{
		get
		{
			if (transformActive)
			{
				ItemSpell transformSpellTemplate = GetTransformSpellTemplate();
				if (transformSpellTemplate != null)
				{
					return transformSpellTemplate;
				}
			}
			ItemDefinition.ItemSkinConfig currentItemSkinConfig = CurrentItemSkinConfig;
			if (currentItemSkinConfig != null && currentItemSkinConfig.removeSpell)
			{
				return null;
			}
			if (CurrentItemSkinConfig != null && CurrentItemSkinConfig.overrideSpell != null)
			{
				return CurrentItemSkinConfig.overrideSpell;
			}
			return GetOriginalSpell();
		}
	}

	public ItemSpell CurrentSpell
	{
		get
		{
			Enchant currentOverrideSpellEnchant = GetCurrentOverrideSpellEnchant();
			if (currentOverrideSpellEnchant != null)
			{
				return currentOverrideSpellEnchant.def.overrideSpell;
			}
			return BaseSpell;
		}
	}

	public List<ItemSpell> itemSpells => def.itemSpells;

	public bool spellNeedsTarget
	{
		get
		{
			ItemSpell currentSpell = CurrentSpell;
			if (currentSpell == null)
			{
				return false;
			}
			return currentSpell.requirement?.NeedTarget == true;
		}
	}

	public Item aimItem => ((List<Item>)tempValues["AimItem"]).FirstOrDefault();

	public BattleFieldTransform OwnerBft
	{
		get
		{
			if (owner?.OwnerBft != null)
			{
				return owner.OwnerBft;
			}
			return null;
		}
	}

	public bool HasBurnCostSpell
	{
		get
		{
			ItemDefinition itemDefinition = def;
			if ((object)itemDefinition == null || !itemDefinition.burnCost)
			{
				ItemSpell currentSpell = CurrentSpell;
				if (currentSpell == null || !currentSpell.burnCost)
				{
					ItemSpell baseSpell = BaseSpell;
					if (baseSpell == null || !baseSpell.burnCost)
					{
						ItemSpell originalSpell = GetOriginalSpell();
						if (originalSpell == null || !originalSpell.burnCost)
						{
							ItemDefinition.ItemSkinConfig currentItemSkinConfig = CurrentItemSkinConfig;
							if (currentItemSkinConfig == null)
							{
								return false;
							}
							return currentItemSkinConfig.overrideSpell?.burnCost == true;
						}
					}
				}
			}
			return true;
		}
	}

	public bool ConsumesAllAvailableCost => CurrentSpell?.ConsumesAllAvailableCost ?? false;

	public bool DisplayCostAsInfinite => ConsumesAllAvailableCost;

	private Item()
	{
	}

	public Item(ItemDefinition def)
	{
		this.def = def;
		cooldown = 0;
		runeSlot = 1;
		counter = def.GetInitCounter();
		resetInitCounter = counter;
		maxCounter = def.GetInitMaxCounter();
		resetInitMaxCounter = maxCounter;
		blackboard = def.defaultBlackBoard;
		afterTurnEnd = false;
		treasureValue = def.treasureValue;
		tagInstances = new List<ItemTagInstance>();
		foreach (ItemTag tag in def.tags)
		{
			AddItemTag(tag);
		}
		foreach (EnchantDef item in def.inherentEnchants.OrEmptyIfNull())
		{
			if (item != null)
			{
				AddEnchant(new Enchant(item));
			}
		}
	}

	public Item(ItemDefinition def, ItemBlackBoard itemBlackBoard)
		: this(def)
	{
		blackboard = itemBlackBoard;
		counter = blackboard.GetValue(ItemBlackBoard.ValueType.InitCounter).GetValueOrDefault();
		resetInitCounter = counter;
		maxCounter = blackboard.GetValue(ItemBlackBoard.ValueType.InitMaxCounter).GetValueOrDefault();
		resetInitMaxCounter = maxCounter;
		if (blackboard.CustomInitCooldown.HasValue)
		{
			cooldown = blackboard.CustomInitCooldown.Value;
		}
	}

	public Item WithCurrentActiveSkin(ApplicationStateInfo applicationState)
	{
		if (applicationState?.itemSkinManager != null)
		{
			skin = applicationState.itemSkinManager.GetItemActiveSkin(def);
			skinBase = applicationState.itemSkinManager.GetItemActiveSkinBase(def);
		}
		return this;
	}

	public Item WithSkin(ItemSkinDef skin)
	{
		this.skin = skin;
		skinBase = skin?.defaultItemSkinBase;
		ApplySkinCounterConfig();
		return this;
	}

	public Item SetSkin(ItemSkinDef skin, bool toUpgrade = true)
	{
		if (skin == null)
		{
			return this;
		}
		this.skin = skin;
		skinBase = skin.defaultItemSkinBase;
		ApplySkinCounterConfig();
		if (toUpgrade && !upgraded)
		{
			Upgrade();
		}
		return this;
	}

	public Enchant GetCurrentOverrideSpellEnchant()
	{
		return enchants?.LastOrDefault((Enchant e) => e != null && (int)e.value > 0 && e.def?.overrideSpell != null);
	}

	public ItemSpell GetOriginalSpell()
	{
		if (itemSpells == null || itemSpells.Count <= 0)
		{
			return null;
		}
		return itemSpells[CurrentSpellIndex];
	}

	public ItemSpell GetTransformSpellTemplate()
	{
		ItemSpell configuredTransformSpellTemplate = runtimeTransformSpellTemplate;
		if (configuredTransformSpellTemplate == null)
		{
			ItemDefinition itemDefinition = def;
			if ((object)itemDefinition == null)
			{
				return null;
			}
			configuredTransformSpellTemplate = itemDefinition.configuredTransformSpellTemplate;
		}
		return configuredTransformSpellTemplate;
	}

	public string GetTransformSpellDescription()
	{
		string configuredTransformSpellDescription = runtimeTransformSpellDescription;
		if (configuredTransformSpellDescription == null)
		{
			ItemDefinition itemDefinition = def;
			if ((object)itemDefinition == null)
			{
				return null;
			}
			configuredTransformSpellDescription = itemDefinition.configuredTransformSpellDescription;
		}
		return configuredTransformSpellDescription;
	}

	public Item GetRuntimeTransformBlackboardSourceItem()
	{
		if (transformActive && runtimeTransformSpellTemplate != null && transformSpellSourceItem != null)
		{
			return transformSpellSourceItem;
		}
		return this;
	}

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		return ((IChildren<IEffectNode>)GetDef()).GetChildren();
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		foreach (Ability item in def.abilities.OrEmptyIfNull())
		{
			if (item == null)
			{
				continue;
			}
			foreach (GameEvent.Type item2 in item.GetAllSubscribeGameEventType())
			{
				yield return item2;
			}
		}
		foreach (ItemTag item3 in def.tags.OrEmptyIfNull())
		{
			if (item3 == null)
			{
				continue;
			}
			foreach (GameEvent.Type item4 in item3.GetAllSubscribeGameEventType())
			{
				yield return item4;
			}
		}
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		foreach (Ability item in def.abilities.OrEmptyIfNull())
		{
			if (item != null && item.CheckTriggerGameEventAble(eve))
			{
				item.TriggerGameEvent(eve);
			}
		}
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		bool result = false;
		foreach (Ability item in def.abilities.OrEmptyIfNull())
		{
			if (item != null && item.CheckTriggerGameEventAble(eve))
			{
				result = true;
			}
		}
		return result;
	}

	public bool IsHidden(GameStateInfo gameState)
	{
		BattleFieldTransform ownerBft = OwnerBft;
		if (ownerBft != null && ownerBft.isEnemy)
		{
			return gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.HideAllEnemyIntent);
		}
		return false;
	}

	public List<ReplacementArmorSource> GetReplacementArmorSources()
	{
		List<ReplacementArmorSource> list = new List<ReplacementArmorSource>();
		foreach (Enchant enchant in enchants)
		{
			foreach (Ability item in enchant.def.attachAbility.OrEmptyIfNull())
			{
				if (item is ReplacementArmorAbility replacementArmorAbility)
				{
					int replacementArmor = replacementArmorAbility.GetReplacementArmor(this, enchant);
					if (replacementArmor > 0)
					{
						list.Add(new ReplacementArmorSource
						{
							enchant = enchant,
							armor = replacementArmor,
							provider = replacementArmorAbility
						});
					}
				}
			}
		}
		return list;
	}

	public int GetReplacementArmorValue()
	{
		long num = 0L;
		foreach (ReplacementArmorSource replacementArmorSource in GetReplacementArmorSources())
		{
			num += replacementArmorSource.armor;
		}
		return CombatValueLimit.Clamp(num);
	}

	public bool ShouldBreakWhenDefendDepleted()
	{
		if (HasBaseDamageableCounter)
		{
			return true;
		}
		return GetReplacementArmorSources().Exists((ReplacementArmorSource source) => source.provider?.breakItemWhenArmorDepleted ?? false);
	}

	public int GetCurrentDefendCounter()
	{
		return CombatValueLimit.Clamp((long)Mathf.Max(0, counter) + (long)GetReplacementArmorValue());
	}

	public ItemDefinition GetDef()
	{
		return def;
	}

	public List<ItemTag> GetTags()
	{
		List<ItemTag> list = new List<ItemTag>(def.GetTags(skin));
		if (!PlayerGameStateInfo.ShouldTreatAsAllElementTags(this))
		{
			return list;
		}
		AssetCatalog assetCatalog = owner?.gameState?.applicationState?.assetCatalog;
		foreach (string tagId in PlayerGameStateInfo.ElementItemTagGuids)
		{
			if (!list.Any((ItemTag tag) => tag?.id == tagId))
			{
				ItemTag itemTag = assetCatalog?.itemTagDefCatalog?.GetAsset(tagId);
				if (itemTag != null)
				{
					list.Add(itemTag);
				}
			}
		}
		return list;
	}

	public bool HasTag(ItemTag tag)
	{
		if (!def.HasTag(tag, skin))
		{
			if (PlayerGameStateInfo.ShouldTreatAsAllElementTags(this))
			{
				return PlayerGameStateInfo.IsElementItemTag(tag);
			}
			return false;
		}
		return true;
	}

	public string GetStoryDescription()
	{
		return def.itemStory;
	}

	public Sprite GetSkinIcon(ApplicationStateInfo applicationState)
	{
		if (skin == null)
		{
			return def.Icon;
		}
		if (!(skin.icon != null))
		{
			return def.Icon;
		}
		return skin.icon;
	}

	public bool IsManualSelectCost(GameStateInfo gameState, GameEvent.TriggerType triggerType)
	{
		ItemSpell currentSpell = CurrentSpell;
		if (currentSpell != null && currentSpell.MovesHeadToCasterRightAsCost)
		{
			return false;
		}
		return gameState.gameEventManager.TriggerManualSelectCostResolve(this, triggerType).promote;
	}

	public bool IsAddAsSpellCost(GameStateInfo gameState)
	{
		if (gameState == null)
		{
			return false;
		}
		if (!gameState.playerState.playerSpellInfo.CheckItemAdded(this))
		{
			return gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo.CheckItemAdded(this);
		}
		return true;
	}

	public bool IsCooldownCastAble(GameStateInfo gameState, GameEvent.TriggerType triggerType = GameEvent.TriggerType.InBattleDisplay)
	{
		bool valueOrDefault = gameState?.gameEventManager?.TriggerItemCastAbleInCoolDownResolve(this, triggerType).promote == true;
		if (HasSpell && OwnerBft != null && OwnerBft.isPlayer)
		{
			return CoolDowned | valueOrDefault;
		}
		return false;
	}

	public void AddEnchant(Enchant enchant)
	{
		if (!enchants.Contains(enchant))
		{
			enchants.Add(enchant);
			enchant.attachItem = this;
		}
	}

	public void RemoveEnchant(Enchant enchant)
	{
		if (enchants.Contains(enchant))
		{
			enchants.Remove(enchant);
			enchant.attachItem = null;
		}
	}

	public void RemoveAllEnchants()
	{
		foreach (Enchant item in enchants.Where((Enchant enchant) => enchant == null || enchant.def?.attachAbility?.Any((Ability ability) => ability is EternalAbility) != true).ToList())
		{
			item.attachItem = null;
			enchants.Remove(item);
		}
	}

	public void AddItemTag(ItemTag tag)
	{
		if (tagInstances.Find((ItemTagInstance t) => t.tag == tag) == null)
		{
			ItemTagInstance itemTagInstance = new ItemTagInstance(this, tag);
			tagInstances.Add(itemTagInstance);
			GameEvent gameEvent = new GameEvent(GameEvent.Type.AddItemTag, GameEvent.TriggerType.Real);
			gameEvent.SetEveObj(GameEvent.EveObjType.Caster, itemTagInstance);
			gameEvent.SetEveObj(GameEvent.EveObjType.Target, itemTagInstance);
			gameEvent.SetEveObj(GameEvent.EveObjType.Destination, this);
			tag.addEffectNode?.Cast(gameEvent);
		}
	}

	public void AddRune(Rune rune)
	{
		if (!Runes.Contains(rune))
		{
			runes.Add(rune);
			rune.attachItem = this;
			GameEvent gameEvent = new GameEvent(GameEvent.Type.AddRune, GameEvent.TriggerType.Real);
			gameEvent.SetEveObj(GameEvent.EveObjType.Caster, rune);
			gameEvent.SetEveObj(GameEvent.EveObjType.Target, rune);
			gameEvent.SetEveObj(GameEvent.EveObjType.Destination, this);
			rune.def.addEffect?.Cast(gameEvent);
			GameEvent gameEvent2 = new GameEvent(GameEvent.Type.RuneValueChange, GameEvent.TriggerType.Real);
			gameEvent2.SetEveObj(GameEvent.EveObjType.Caster, rune);
			gameEvent2.SetEveObj(GameEvent.EveObjType.Target, rune);
			gameEvent2.SetEveObj(GameEvent.EveObjType.Destination, this);
			gameEvent2.deltaValue = rune.value;
			rune.def.changeEffect?.Cast(gameEvent2);
		}
	}

	public void ChangeRuneValue(Rune rune, int newValue)
	{
		int value = rune.value;
		rune.value = newValue;
		GameEvent gameEvent = new GameEvent(GameEvent.Type.RuneValueChange, GameEvent.TriggerType.Real);
		gameEvent.SetEveObj(GameEvent.EveObjType.Caster, rune);
		gameEvent.SetEveObj(GameEvent.EveObjType.Target, rune);
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, this);
		gameEvent.deltaValue = newValue - value;
		rune.def.changeEffect?.Cast(gameEvent);
	}

	public void RemoveRune(Rune rune)
	{
		if (Runes.Contains(rune))
		{
			runes.Remove(rune);
			rune.attachItem = null;
			GameEvent gameEvent = new GameEvent(GameEvent.Type.RemoveRune, GameEvent.TriggerType.Real);
			gameEvent.SetEveObj(GameEvent.EveObjType.Caster, rune);
			gameEvent.SetEveObj(GameEvent.EveObjType.Target, rune);
			gameEvent.SetEveObj(GameEvent.EveObjType.Destination, this);
			rune.def.removeEffect?.Cast(gameEvent);
			GameEvent gameEvent2 = new GameEvent(GameEvent.Type.RuneValueChange, GameEvent.TriggerType.Real);
			gameEvent2.SetEveObj(GameEvent.EveObjType.Caster, rune);
			gameEvent2.SetEveObj(GameEvent.EveObjType.Target, rune);
			gameEvent2.SetEveObj(GameEvent.EveObjType.Destination, this);
			gameEvent2.deltaValue = -rune.value;
			rune.def.changeEffect?.Cast(gameEvent2);
		}
	}

	internal bool RemoveRuneSilently(Rune rune)
	{
		if (rune != null)
		{
			List<Rune> list = runes;
			if (list != null && list.Remove(rune))
			{
				rune.attachItem = null;
				return true;
			}
		}
		return false;
	}

	public Item Copy(bool spawnFromDeck = false)
	{
		Item item = MemberwiseClone() as Item;
		item.owner = null;
		if (Enchants != null)
		{
			item.enchants = new List<Enchant>();
			foreach (Enchant enchant in Enchants)
			{
				item.enchants.Add(enchant.CopyToItem(item));
			}
		}
		if (Runes != null)
		{
			item.runes = new List<Rune>();
			foreach (Rune rune in Runes)
			{
				item.runes.Add(rune.CopyToItem(item));
			}
		}
		if (blackboard != null)
		{
			item.blackboard = new ItemBlackBoard();
			foreach (ItemBlackBoard.ValueInfo value in blackboard.values)
			{
				item.blackboard.SetValue(value.type, value.value);
			}
		}
		if (tagInstances != null)
		{
			item.tagInstances = new List<ItemTagInstance>();
			foreach (ItemTagInstance tagInstance in tagInstances)
			{
				item.tagInstances.Add(tagInstance.CopyToItem(item));
			}
		}
		item.tempValues = new Dictionary<string, object>(tempValues);
		if (spawnFromDeck)
		{
			item.sourceDeckItem = this;
			item.resetInitMaxCounter = maxCounter;
			item.resetInitCounter = counter;
		}
		else
		{
			item.sourceDeckItem = null;
			item.resetInitMaxCounter = resetInitMaxCounter;
			item.resetInitCounter = resetInitCounter;
		}
		return item;
	}

	public void SetDef(ItemDefinition def)
	{
		this.def = def;
	}

	private void ApplySkinCounterConfig()
	{
		counter = def.GetInitCounter(skin);
		resetInitCounter = counter;
		maxCounter = def.GetInitMaxCounter(skin);
		resetInitMaxCounter = maxCounter;
	}

	public void SetRuntimeTransformTemplate(ItemSpell spellTemplate, string description, Item sourceItem)
	{
		runtimeTransformSpellTemplate = spellTemplate;
		runtimeTransformSpellDescription = description;
		transformSpellSourceItem = sourceItem;
		transformHasRuntimeTemplate = runtimeTransformSpellTemplate != null;
	}

	public void ClearRuntimeTransformTemplate()
	{
		runtimeTransformSpellTemplate = null;
		runtimeTransformSpellDescription = null;
		transformSpellSourceItem = null;
		transformHasRuntimeTemplate = false;
	}

	public void SetRemoveAfterCast(bool value = true)
	{
		removeAfterCast = value;
	}

	public bool ConsumeRemoveAfterCast()
	{
		if (!removeAfterCast)
		{
			return false;
		}
		removeAfterCast = false;
		return true;
	}

	public bool ShouldAutoTransformAfterSpellCast(SpellCastInfo spellInfo)
	{
		if (spellInfo?.spellItem != this)
		{
			return false;
		}
		if (spellInfo.casterType != SpellCastInfo.CasterType.Player || spellInfo.isTriggerCast)
		{
			return false;
		}
		if (!HasTransformTemplate || transformActive)
		{
			return false;
		}
		if (IsCastingOverrideSpell(spellInfo))
		{
			return false;
		}
		ItemSpell originalSpell = GetOriginalSpell();
		if (originalSpell != null)
		{
			return spellInfo.focusSpell == originalSpell;
		}
		return false;
	}

	public bool IsCastingOverrideSpell(SpellCastInfo spellInfo)
	{
		if (spellInfo?.spellItem != this)
		{
			return false;
		}
		if (spellInfo.GetCastingOverrideSpellEnchant() != null)
		{
			return true;
		}
		ItemSpell originalSpell = GetOriginalSpell();
		ItemSpell transformSpellTemplate = GetTransformSpellTemplate();
		ItemSpell focusSpell = spellInfo.focusSpell;
		if (focusSpell != null && focusSpell != originalSpell)
		{
			return focusSpell != transformSpellTemplate;
		}
		return false;
	}

	public ItemSkinBaseDef.ImageSetup GetImageSetup(ItemSkinBaseDef.ItemSkinBaseSlotType type)
	{
		return DisplaySkinBase.GetImageSetup(type, majorTag);
	}

	public ItemSkinBaseDef.SubItemSetup GetSubItemSetup(ItemSkinBaseDef.ItemSkinBaseSlotType type)
	{
		return DisplaySkinBase.GetSubItemSetupSetup(type, majorTag);
	}

	public override string ToString()
	{
		return base.ToString() + def?.name;
	}

	public ConvertablePivot GetPivot(GameStateInfo gameState)
	{
		if (owner == null || OwnerBft == null)
		{
			return null;
		}
		ItemContainer.ContainerType type = owner.type;
		bool isPlayer = OwnerBft.isPlayer;
		switch (type)
		{
		case ItemContainer.ContainerType.Deck:
			if (isPlayer)
			{
				return new ConvertablePivot(gameState.UIManager.battlePanel.deckButton.transform, gameState.UIManager.mainCanvas);
			}
			return gameState.UIManager.enemyBftUIDisplay.tableItemArea.itemSpawnPivot;
		case ItemContainer.ContainerType.DrawPile:
			if (isPlayer)
			{
				return gameState.UIManager.battlePanel.drawPileButtonPivot;
			}
			return gameState.UIManager.enemyBftUIDisplay.tableItemArea.itemSpawnPivot;
		case ItemContainer.ContainerType.Table:
		{
			if (isPlayer)
			{
				Transform transform = gameState.UIManager.playerItemArea.GetSlotDisplay(this)?.transform;
				if (transform == null)
				{
					return null;
				}
				return new ConvertablePivot(transform, gameState.UIManager.ui3DCameraCanvas);
			}
			Transform transform2 = gameState.UIManager.enemyBftUIDisplay.tableItemArea.GetItemDisplay(this)?.transform;
			if (transform2 == null)
			{
				return null;
			}
			return new ConvertablePivot(transform2, gameState.UIManager.ui3DWorldCanvas);
		}
		case ItemContainer.ContainerType.WastePile:
			if (isPlayer)
			{
				return new ConvertablePivot(gameState.UIManager.battlePanel.wastePileButton.transform, gameState.UIManager.mainCanvas);
			}
			return gameState.UIManager.enemyBftUIDisplay.tableItemArea.itemSpawnPivot;
		case ItemContainer.ContainerType.ExilePile:
			if (isPlayer)
			{
				return new ConvertablePivot(gameState.UIManager.battlePanel.wastePileButton.transform, gameState.UIManager.mainCanvas);
			}
			return gameState.UIManager.enemyBftUIDisplay.tableItemArea.itemSpawnPivot;
		case ItemContainer.ContainerType.Companion:
			if (isPlayer)
			{
				return new ConvertablePivot(gameState.UIManager.battlePanel.playerBftUIDisplay.companionItemArea[this].transform, gameState.UIManager.mainCanvas);
			}
			return new ConvertablePivot(gameState.UIManager.enemyBftUIDisplay.companionItemArea[this].transform, gameState.UIManager.mainCanvas);
		default:
			TraceLog.Exception(new ArgumentOutOfRangeException("type", type, null));
			return null;
		}
	}

	public bool HasIcon()
	{
		return def.HasIcon();
	}

	public string GetTagsPreviewText()
	{
		bool hasSubTags;
		return GetMajorTagText() + "-" + GetSubTagsPreviewText(out hasSubTags);
	}

	public string GetMajorTagText()
	{
		ItemTag itemTag = majorTag;
		if (itemTag == null)
		{
			return "";
		}
		return itemTag.tagName;
	}

	public string GetSubTagsPreviewText(out bool hasSubTags)
	{
		hasSubTags = false;
		List<ItemTag> tags = GetTags();
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (ItemTag item in tags)
		{
			if (!item.majorTag && !item.hideTag)
			{
				if (num != 0)
				{
					stringBuilder.Append("/");
				}
				stringBuilder.Append(" " + item.tagName.ColorText(item.textColor) + " ");
				num++;
				hasSubTags = true;
			}
		}
		return stringBuilder.ToString();
	}

	public string GetItemName()
	{
		if (!upgraded)
		{
			return ItemName;
		}
		if (!string.IsNullOrEmpty(def.upgradedItemName))
		{
			if (upgradeLevel <= 1)
			{
				return def.upgradedItemName;
			}
			return $"{def.upgradedItemName}+{upgradeLevel}";
		}
		if (upgradeLevel <= 1)
		{
			return ItemName + "+";
		}
		return $"{ItemName}+{upgradeLevel}";
	}

	public Color GetItemNameColor(bool qualityColor = false)
	{
		if (qualityColor)
		{
			return ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(qualityLevel);
		}
		return defaultColor;
	}

	public Sprite GetBackgroundImage_Small()
	{
		ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
		if (majorTag == null)
		{
			return itemBackgroundImageConfig.bg_Attack;
		}
		return majorTag.type switch
		{
			ItemTag.Type.Companion => itemBackgroundImageConfig.bg_Companion, 
			ItemTag.Type.Health => itemBackgroundImageConfig.bg_Health, 
			ItemTag.Type.Component => itemBackgroundImageConfig.bg_Component, 
			ItemTag.Type.Attack => itemBackgroundImageConfig.bg_Attack, 
			ItemTag.Type.Defence => itemBackgroundImageConfig.bg_Defence, 
			ItemTag.Type.Blessing => itemBackgroundImageConfig.bg_Blessing, 
			ItemTag.Type.Curse => itemBackgroundImageConfig.bg_Curse, 
			ItemTag.Type.Ability => itemBackgroundImageConfig.bg_Ability, 
			_ => itemBackgroundImageConfig.bg_Attack, 
		};
	}

	public Sprite GetBackgroundImage_Large()
	{
		ItemTag itemTag = majorTag;
		ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
		if (itemTag == null)
		{
			return itemBackgroundImageConfig.bgl_Attack;
		}
		return itemTag.type switch
		{
			ItemTag.Type.Companion => itemBackgroundImageConfig.bgl_Companion, 
			ItemTag.Type.Health => itemBackgroundImageConfig.bgl_Health, 
			ItemTag.Type.Component => itemBackgroundImageConfig.bgl_Component, 
			ItemTag.Type.Attack => itemBackgroundImageConfig.bgl_Attack, 
			ItemTag.Type.Defence => itemBackgroundImageConfig.bgl_Defence, 
			ItemTag.Type.Blessing => itemBackgroundImageConfig.bgl_Blessing, 
			ItemTag.Type.Curse => itemBackgroundImageConfig.bgl_Curse, 
			ItemTag.Type.Ability => itemBackgroundImageConfig.bgl_Ability, 
			_ => itemBackgroundImageConfig.bgl_Attack, 
		};
	}

	public Sprite GetCounterImage()
	{
		ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
		ItemTag itemTag = majorTag;
		if (itemTag == null)
		{
			return itemBackgroundImageConfig.icon_Counter_None;
		}
		List<ItemTag> tags = def.tags;
		if (tags.Find((ItemTag t) => t.type == ItemTag.Type.Music) != null)
		{
			return itemBackgroundImageConfig.icon_Counter_Music;
		}
		switch (itemTag.type)
		{
		case ItemTag.Type.Curse:
			return itemBackgroundImageConfig.icon_Counter_Curse;
		case ItemTag.Type.Health:
			return itemBackgroundImageConfig.icon_Counter_Health;
		case ItemTag.Type.Defence:
			return itemBackgroundImageConfig.icon_Counter_Defence;
		case ItemTag.Type.Companion:
			if ((bool)tags.Find((ItemTag t) => t.type == ItemTag.Type.CompanionDefence))
			{
				return itemBackgroundImageConfig.icon_Counter_Health;
			}
			return itemBackgroundImageConfig.icon_Counter_None;
		case ItemTag.Type.Ability:
			return itemBackgroundImageConfig.icon_Ability;
		case ItemTag.Type.Component:
			return itemBackgroundImageConfig.icon_Counter_Health;
		case ItemTag.Type.Attack:
			return itemBackgroundImageConfig.icon_Counter_None;
		case ItemTag.Type.Blessing:
			return itemBackgroundImageConfig.icon_Counter_Curse;
		default:
			return itemBackgroundImageConfig.icon_Counter_None;
		}
	}

	public Sprite GetCounterImageLarge()
	{
		ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
		ItemTag itemTag = majorTag;
		if (itemTag == null)
		{
			return itemBackgroundImageConfig.icon_Counter_Curse;
		}
		List<ItemTag> tags = def.tags;
		if (tags.Find((ItemTag t) => t.type == ItemTag.Type.Music) != null)
		{
			return itemBackgroundImageConfig.iconl_Counter_Music;
		}
		switch (itemTag.type)
		{
		case ItemTag.Type.Curse:
			return itemBackgroundImageConfig.iconl_Counter_Curse;
		case ItemTag.Type.Health:
			return itemBackgroundImageConfig.iconl_Counter_Health;
		case ItemTag.Type.Defence:
			return itemBackgroundImageConfig.iconl_Counter_Defence;
		case ItemTag.Type.Companion:
			if ((bool)tags.Find((ItemTag t) => t.type == ItemTag.Type.CompanionDefence))
			{
				return itemBackgroundImageConfig.icon_Counter_Health;
			}
			return itemBackgroundImageConfig.icon_Counter_None;
		case ItemTag.Type.Ability:
			return itemBackgroundImageConfig.icon_Ability;
		case ItemTag.Type.Component:
			return itemBackgroundImageConfig.iconl_Counter_Health;
		case ItemTag.Type.Attack:
			return itemBackgroundImageConfig.iconl_Counter_None;
		case ItemTag.Type.Blessing:
			return itemBackgroundImageConfig.iconl_Counter_Curse;
		default:
			return itemBackgroundImageConfig.iconl_Counter_Curse;
		}
	}

	public Sprite GetSpellTextBackgroundImage()
	{
		ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
		ItemTag itemTag = majorTag;
		if (itemTag == null)
		{
			return itemBackgroundImageConfig.bgl_TextArea_Spell;
		}
		return itemTag.type switch
		{
			ItemTag.Type.Companion => itemBackgroundImageConfig.bgl_TextArea_Companion, 
			_ => itemBackgroundImageConfig.bgl_TextArea_Spell, 
		};
	}

	public Sprite GetCostImage()
	{
		ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
		if (HasBurnCostSpell)
		{
			return itemBackgroundImageConfig.icon_Cost_Burn;
		}
		ItemTag itemTag = majorTag;
		if (itemTag == null)
		{
			return itemBackgroundImageConfig.icon_Cost_Default;
		}
		return itemTag.type switch
		{
			ItemTag.Type.Companion => itemBackgroundImageConfig.icon_Cost_Companion, 
			_ => itemBackgroundImageConfig.icon_Cost_Default, 
		};
	}

	public Sprite GetCostImageLarge()
	{
		ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
		if (HasBurnCostSpell)
		{
			return itemBackgroundImageConfig.iconl_Cost_Burn;
		}
		ItemTag itemTag = majorTag;
		if (itemTag == null)
		{
			return itemBackgroundImageConfig.iconl_Cost_Default;
		}
		return itemTag.type switch
		{
			ItemTag.Type.Companion => itemBackgroundImageConfig.iconl_Cost_Companion, 
			_ => itemBackgroundImageConfig.iconl_Cost_Default, 
		};
	}

	public Material GetCostMaterial()
	{
		ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
		if (HasBurnCostSpell)
		{
			return itemBackgroundImageConfig.material_Cost_Burn;
		}
		ItemTag itemTag = majorTag;
		if ((object)itemTag == null || itemTag.type != ItemTag.Type.Companion)
		{
			ItemTag itemTag2 = majorTag;
			if ((object)itemTag2 == null || itemTag2.type != ItemTag.Type.CompanionDefence)
			{
				return itemBackgroundImageConfig.material_Cost_Default;
			}
		}
		return itemBackgroundImageConfig.material_Cost_Companion;
	}

	public string GetSpriteCounterText()
	{
		if (!withCounter)
		{
			return "";
		}
		if (InvalidCounter)
		{
			return "？";
		}
		if (tempValues.TryGetValue("SpiderPhase2Activity", out var value) && value is int)
		{
			_ = (int)value;
			return TextDataPreset.ValueFormat.FormatWith(counter);
		}
		if ((int)maxCounter == 0)
		{
			return TextDisplayHelper.ToNumSpriteString(counter) ?? "";
		}
		return TextDisplayHelper.ToNumSpriteString(counter) + "/" + TextDisplayHelper.ToNumSpriteString(maxCounter);
	}

	public string GetCounterText()
	{
		if (!withCounter)
		{
			return "";
		}
		if (InvalidCounter)
		{
			return "？";
		}
		if (tempValues.TryGetValue("SpiderPhase2Activity", out var value) && value is int)
		{
			_ = (int)value;
			return TextDataPreset.ValueFormat.FormatWith(counter);
		}
		if ((int)maxCounter == 0)
		{
			return counter.ToString();
		}
		return $"{counter}/{maxCounter}";
	}

	public Color GetCounterTextColor()
	{
		Color color = ((!(majorTag != null)) ? new Color(0.95f, 0.95f, 0.95f, 1f) : majorTag.counterTextColor);
		if (HasTag(ScriptableObjectSingleton<GameDataManager>.instance.ChapterTag) && (int)counter == (int)maxCounter && (int)maxCounter != 0)
		{
			return color * Color.grey;
		}
		return color;
	}

	public Color GetCostTextColor()
	{
		ItemDisplayBase.ItemImageAtlas itemBackgroundImageConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.itemBackgroundImageConfig;
		ItemTag itemTag = majorTag;
		if (itemTag == null)
		{
			return itemBackgroundImageConfig.color_Cost_Default;
		}
		return itemTag.type switch
		{
			ItemTag.Type.Companion => itemBackgroundImageConfig.color_Cost_Companion, 
			_ => itemBackgroundImageConfig.color_Cost_Default, 
		};
	}

	public Color GetCostImageColor()
	{
		return Color.white;
	}

	public Color GetCostDisplayTextColor()
	{
		return GetCostTextColor();
	}

	public string GetDisplayCostText(int cost)
	{
		if (!DisplayCostAsInfinite)
		{
			return TextDisplayHelper.ToNumSpriteString(cost);
		}
		return "<sprite=10 tint=1>";
	}

	public int GetDisplayFinalCost(ApplicationStateInfo applicationState, GameEvent.TriggerType triggerType)
	{
		if (CurrentSpell?.requirement == null)
		{
			return 0;
		}
		DisplaySpellCastInfo castInfo = new DisplaySpellCastInfo(applicationState.runningGameState)
		{
			spellItem = this
		};
		return CurrentSpell.requirement.GetFinalCostValue(castInfo, triggerType);
	}

	public int GetDisplayBaseCost(ApplicationStateInfo applicationState)
	{
		if (CurrentSpell?.requirement == null)
		{
			return 0;
		}
		DisplaySpellCastInfo castInfo = new DisplaySpellCastInfo(applicationState.runningGameState)
		{
			spellItem = this
		};
		return CurrentSpell.requirement.GetBaseCostValue(castInfo);
	}

	public bool TryGetActiveOverrideSpellCostDisplay(ApplicationStateInfo applicationState, GameEvent.TriggerType triggerType, out Enchant sourceEnchant, out int originalCost, out int overrideCost)
	{
		sourceEnchant = GetCurrentOverrideSpellEnchant();
		originalCost = 0;
		overrideCost = 0;
		ItemSpell baseSpell = BaseSpell;
		ItemSpell itemSpell = sourceEnchant?.def?.overrideSpell;
		if (sourceEnchant == null || itemSpell == null || baseSpell == null || itemSpell == baseSpell)
		{
			return false;
		}
		GameStateInfo gameState = applicationState?.runningGameState;
		if (baseSpell.requirement != null)
		{
			DisplaySpellCastInfo castInfo = new DisplaySpellCastInfo(gameState)
			{
				spellItem = this,
				forcedFocusSpell = baseSpell
			};
			originalCost = baseSpell.requirement.GetFinalCostValue(castInfo, triggerType);
		}
		overrideCost = GetDisplayFinalCost(applicationState, triggerType);
		return originalCost != overrideCost;
	}

	public void Upgrade(GameStateInfo gs = null)
	{
		upgradeLevel++;
		if (CurrentUpgradeEffectNode != null)
		{
			GameEvent gameEvent = new GameEvent(GameEvent.Type.UpgradeItem, GameEvent.TriggerType.Real);
			gameEvent.gameState = gs;
			gameEvent.CasterItem = this;
			gameEvent.TargetItem = this;
			CurrentUpgradeEffectNode.Cast(gameEvent);
		}
	}

	public void Downgrade(GameStateInfo gs = null)
	{
		upgradeLevel--;
		if (CurrentDowngradeEffectNode != null)
		{
			GameEvent gameEvent = new GameEvent(GameEvent.Type.DowngradeItem, GameEvent.TriggerType.Real);
			gameEvent.gameState = gs;
			gameEvent.CasterItem = this;
			gameEvent.TargetItem = this;
			CurrentDowngradeEffectNode.Cast(gameEvent);
		}
	}
}
