using System;
using System.Collections.Generic;
using System.Linq;

public class GameEvent
{
	public enum ContainerType
	{
		None = 0,
		TableContainer = 1,
		DrawPileContainer = 2,
		WastePileContainer = 3,
		DeckContainer = 4,
		ExilePileContainer = 5,
		RoomContainer = 10,
		CompanionContainer = 11
	}

	public enum DealType
	{
		Any,
		OwnerToOpponent,
		OpponentToOwner,
		OwnerToOwner,
		OpponentToOpponent,
		CasterToOpponent,
		CasterToOwner,
		ToOwner,
		ToOpponent,
		ToCaster,
		OpponentToCaster,
		OwnerToCaster,
		FromOwner,
		FromOpponent,
		FromCaster,
		FromPlayer,
		FromEnemy,
		PlayerToEnemy,
		EnemyToPlayer,
		ToPlayer,
		ToEnemy,
		CasterAttachToOpponent,
		CasterAttachToOwner,
		ToCasterAttach,
		OpponentToCasterAttach,
		OwnerToCasterAttach,
		FormCasterAttach
	}

	public enum EventBFTType
	{
		None,
		Owner,
		Opponent,
		Target,
		Source,
		AddItemOwner,
		OverloadOwner,
		Player,
		Enemy,
		Origin,
		Destination
	}

	public enum EventItemType
	{
		None,
		CasterItem,
		SourceItem,
		TargetItem,
		SpellItem,
		AddItem,
		OverloadItem,
		CasterAimItem,
		DamagedItem,
		CasterAttachingItem,
		NodeTempItem,
		DestinationItem
	}

	public enum EveObjType
	{
		Caster = 0,
		Source = 1,
		Target = 2,
		Destination = 3,
		Origin = 4,
		Get_CasterAttach = 10000,
		Get_SourceAttach = 10001,
		Get_TargetAttach = 10002,
		Get_DestinationAttach = 10003,
		Get_OriginAttach = 10004,
		Get_CasterBftOwner = 20000,
		Get_SourceBftOwner = 20001,
		Get_TargetBftOwner = 20002,
		Get_DestinationBftOwner = 20003,
		Get_OriginBftOwner = 20004
	}

	public enum OwnerItemsType
	{
		None,
		AllReal,
		TableItems,
		DrawPileItems,
		WastePileItems,
		Deck,
		AllItemsIncludeDeck,
		Companion,
		ExilePileItems
	}

	public enum TriggerType
	{
		Real,
		InBattleDisplay,
		OutBattleDisplay
	}

	public enum Type
	{
		None = 0,
		NodeCastSpell = 1,
		BeforeAddItem = 2,
		AddItem = 3,
		BeforeExileItem = 4,
		ExileItem = 5,
		Overflow = 7,
		BattleStart = 8,
		BeforeBattleEnd = 9,
		PlayerTurnStart = 10,
		PlayerTurnEnd = 11,
		BeforeConsume = 12,
		PlayerFirstTurnStart = 13,
		AfterSpellCast = 14,
		AfterTransform = 17,
		BeforeSpellCast = 15,
		BeforeSpellCastHandleCost = 16,
		BeforeDamage = 20,
		AfterDamage = 21,
		EnemyTurnStart = 22,
		EnemyTurnEnd = 23,
		AfterAddEnchant = 25,
		BeforeRemoveEnchant = 26,
		EnterCity = 27,
		ExitCity = 28,
		AfterHeal = 32,
		ChangeMaxCounter = 31,
		ChangeCoolDown = 33,
		ChangeCounter = 34,
		ChangeCoin = 35,
		ChangeCrystal = 36,
		ChangeGem = 37,
		BftHealthChange = 38,
		BftMaxHealthChange = 39,
		Display = 40,
		BeforeEnterLocation = 49,
		EnterLocation = 51,
		ExitLocation = 52,
		AfterEnterLocationScreenFade = 53,
		AfterExitLocationToMap = 54,
		BeforeFinishWorldEvent = 55,
		StartWorldEvent = 56,
		EnchantValueChange = 58,
		UpgradeItem = 60,
		DowngradeItem = 61,
		DrawFromPile = 63,
		SpellCastNodeCheck = 79,
		SetSpellTargetCheck = 80,
		BeforeRemoveItem = 82,
		AfterRemoveItem = 83,
		MoveSlotIndex = 85,
		SlotIndexChange = 86,
		AfterConsume = 87,
		BeforeMapGeneration = 88,
		BeforeGameStart = 89,
		AddInfluence = 90,
		RemoveInfluence = 91,
		TriggerTurnPhaseChange = 92,
		AddInherentItems = 95,
		BeforePlayerTurnEnd = 96,
		AfterTurnStartDrawItems = 97,
		BeforeCost = 98,
		AfterCost = 99,
		BeforeEnemyEscape = 101,
		CastSkill = 102,
		AddSkill = 103,
		RemoveSkill = 104,
		AfterDeathblow = 105,
		BeforeEnterNextStage = 106,
		DrawCardRecycle = 107,
		BeforeBreakItem = 108,
		AfterBreakItem = 109,
		EnemyEnterPhase2 = 110,
		EnemyEnterPhase2AfterConsumeItem = 111,
		AfterEnemyDefeated = 112,
		AfterSkillCast = 115,
		AfterPlayerTurnEndWaste = 116,
		CounterReachMaxCounter = 117,
		AfterTriggerChapter = 159,
		TriggerChapterEffect = 162,
		ConvertYinYangPower = 184,
		ChangeYinYangPower = 186,
		AddRune = 190,
		RemoveRune = 191,
		RuneValueChange = 192,
		EnterCrystalFreeze = 194,
		ExitCrystalFreeze = 195,
		TriggerBonus = 196,
		BeforeSummonCompanion = 197,
		TargetRequirement = 200,
		AfterEnterNextStage = 202,
		ContainerSlotCountChange = 203,
		StoreSellItem = 204,
		ActivateSkill = 205,
		BeforeConsumeSkill = 206,
		BeforeSwapSkill = 207,
		WorldEventChoose = 208,
		MoveItemToHead = 209,
		MoveItemToTail = 210,
		ActivatePotential = 211,
		PlayerDeath = 212,
		BeforeGameGeneration = 213,
		SkillDurabilityClean = 214,
		FinishBonus = 215,
		AfterSwapItem = 219,
		PrayEffect = 220,
		AfterAddPrayToRitual = 221,
		AfterRemovePrayFormRitual = 222,
		AfterRitualTrigger = 223,
		AfterBattleEnd = 225,
		SpawnItem = 226,
		JoinItemBattle = 227,
		SideItemChange = 228,
		BeforeShowForerunner = 229,
		EnterRoom = 230,
		AfterLoadGame = 231,
		ActivateStartGameBuff = 232,
		BeforeBattlEncounterEnd = 233,
		AddItemTag = 234,
		AfterTriggerCustomEvent = 235,
		AfterTriggerAbility = 236,
		ChangeWillPower = 237,
		BeforeChangeStance = 241,
		AfterChangeStance = 242,
		SetEnvironmentInfluence = 250,
		BeforeDeathBlow = 251,
		SkillDurabilityChange = 252,
		MoonCocoonBreak = 253,
		AfterShuffleContainer = 254,
		BeforeGameEnd = 10000,
		AfterGameEnd = 10001,
		AddItemCounterResolveInfo = 50001,
		ConsumeResolveInfo = 50002,
		AddCostResolveInfo = 50003,
		BftDeathCheckResolve = 50004,
		SpellCastResolve = 50005,
		TurnDrawResolve = 50006,
		FixedSlotCountResolve = 50007,
		SpellCastCostResolve = 50008,
		ExileResolveInfo = 50010,
		DrawItemResolve = 50012,
		ShuffleItemResolve = 50013,
		SpellCostConsumeResolve = 50014,
		DrawCardRecyleResolve = 50016,
		ChangeEnchantResolve = 50017,
		BreakItemResolve = 50018,
		AddChapterResolve = 50019,
		TurnStartCoolDownResolve = 50020,
		CounterChangeResolveInfo = 50021,
		YinYangConvertRequirementResolve = 50022,
		YinYangConsumeRequirementResolve = 50023,
		HealResolve = 50024,
		ItemDefendDamageAbleResolve = 50025,
		DamageHealthChangeResolve = 50026,
		SpellCastCoolDownResolve = 50027,
		SpellCastEffectResolve = 50028,
		GenerateOverflowDamageCommandResolveInfo = 50029,
		EffectMarkResolve = 50030,
		GenerateDamageTimesResolve = 50040,
		GenerateDamageBFTResolve = 50041,
		GenerateDamagePierceResolve = 50042,
		GenerateDamageDamageValueResolve = 50043,
		GenerateDamageSubCounterResolve = 50044,
		AddItemToContainerResolve = 50060,
		CanCastWithoutFulfillCostResolve = 50061,
		DrawItemReplaceResolve = 50070,
		ConsumeResetCounterResolve = 50071,
		PendingDeathReviveResolve = 50072,
		PendingDeathRevivePhase2Resolve = 50073,
		BftMaxHealthChangeResolve = 50074,
		CoinChangeResolve = 50075,
		CrystalChangeResolve = 50076,
		ManualSelectCostResolve = 50077,
		ItemCastAbleInCoolDownResolve = 50078,
		SkillPropertyValueResolve = 50079,
		SkillCastCoolDownResolve = 50080,
		SpawnItemResolve = 50081,
		ChangeStanceValueResolve = 50082,
		ChangeTempHealthResolve = 50083,
		RepeatStanceAbilityEffectResolve = 50084,
		ChangeStanceValueWithChangeTypeResolve = 50085,
		DeathBlowEnterCrystalFreezeResolve = 50086,
		GenerateCrystalUpgradeCostResolveInfo = 51000
	}

	public class ResolveInfo
	{
		public int deltaPercent;

		public int deltaTimes;

		public int deltaValue;

		public List<EffectMarkAbility.EffectMarkInfo> effectMarkInfos = new List<EffectMarkAbility.EffectMarkInfo>();

		public int fixedTargetValue;

		public bool isFixedTargetValue;

		public List<(Item, int)> itemValuePairs = new List<(Item, int)>();

		public int lowerBoundary = int.MinValue;

		public bool promote;

		public HashSet<IGameEventSubscriber> promoteSources = new HashSet<IGameEventSubscriber>();

		public bool reject;

		public bool noOverflow;

		public bool suppressBreak;

		public bool realHealthOnly;

		public BattleFieldTransform overrideBftTarget;

		public bool pierce;

		public Item rejectSource;

		public Item replaceItem;

		public int sourceDelta;

		public float sourceDeltaPercent;

		public int targetDelta;

		public int targetDeltaPercent;

		public int upperBoundary = int.MaxValue;
	}

	public const string NodeTempItemKey = "NodeTempItemKey";

	public const string NodeTempItemsKey = "NodeTempItemsKey";

	public const string NodeTempEnchantKey = "NodeTempEnchantKey";

	public const string NodeTempEnchantsKey = "NodeTempEnchantsKey";

	public const string NodeTempRuneKey = "NodeTempRuneKey";

	public const string NodeTempRunesKey = "NodeTempRunesKey";

	public const string NodeTempInfluenceKey = "NodeTempInfluenceKey";

	public const string NodeTempInfluencesKey = "NodeTempInfluencesKey";

	public const string LoopIndexKey = "LoopIndexKey";

	public const string TriggerChapterIndexKey = "TriggerChapterIndexKey";

	public AddItemEventInfo addItemInfo;

	public Item SwapSourceItem;

	public int amount;

	public int amount2;

	public BattleBonus battleBonus;

	public FinishBattleEncounterAction.FinishType battleFinishType;

	public bool boolValue;

	public string stringValue;

	public ConsumeItemAction.ConsumeType consumeType;

	public ExileItemAction.ExileType exileType;

	public Damage damage;

	public DamageCommand damagecommand;

	public Damage.ResolveType damageResolveType;

	public int? breakItemOriginalDefendCounter;

	public Dictionary<EveObjType, object> eveObjDic = new Dictionary<EveObjType, object>();

	public GameStateInfo gameState;

	public Dictionary<string, object> nodeTempValues = new Dictionary<string, object>();

	public int choiceNumber;

	public SkillDef.SkillPropertyType skillPropertyType;

	public ResolveInfo resolveInfo;

	public List<Item> SelectedItems = new List<Item>();

	public List<GameSkill> SelectedSkills = new List<GameSkill>();

	public List<Influence> SelectedInfluences = new List<Influence>();

	public List<Enchant> SelectedEnchants = new List<Enchant>();

	public List<Rune> SelectedRunes = new List<Rune>();

	public SpellCastInfo spellInfo;

	public EnchantDefGenerator negativeEnchantReplacementGenerator;

	public bool succesGame;

	public bool isAfterIntroFirstGame;

	public TriggerType triggerType;

	public Type type;

	public NodeBasedWorldEvent.WorldEventChoiceSetup worldEventChoiceSetup;

	public int deltaValue;

	public BattleFieldTransform dealTypeSourceOwnerOverride;

	public PathLocation TargetLocation
	{
		get
		{
			return GetEveObj<PathLocation>(EveObjType.Target);
		}
		set
		{
			SetEveObj(EveObjType.Target, value);
		}
	}

	public BattleFieldTransform TargetBft
	{
		get
		{
			return GetEveObj<BattleFieldTransform>(EveObjType.Target);
		}
		set
		{
			SetEveObj(EveObjType.Target, value);
		}
	}

	public Enchant TargetEnchant
	{
		get
		{
			return GetEveObj<Enchant>(EveObjType.Target);
		}
		set
		{
			SetEveObj(EveObjType.Target, value);
		}
	}

	public Rune TargetRune
	{
		get
		{
			return GetEveObj<Rune>(EveObjType.Target);
		}
		set
		{
			SetEveObj(EveObjType.Target, value);
		}
	}

	public Influence TargetInfluence
	{
		get
		{
			return GetEveObj<Influence>(EveObjType.Target);
		}
		set
		{
			SetEveObj(EveObjType.Target, value);
		}
	}

	public Item TargetItem
	{
		get
		{
			return GetEveObj<Item>(EveObjType.Target);
		}
		set
		{
			SetEveObj(EveObjType.Target, value);
		}
	}

	public GameSkill TargetSkill
	{
		get
		{
			return GetEveObj<GameSkill>(EveObjType.Target);
		}
		set
		{
			SetEveObj(EveObjType.Target, value);
		}
	}

	public GameSkill SourceSkill
	{
		get
		{
			return GetEveObj<GameSkill>(EveObjType.Source);
		}
		set
		{
			SetEveObj(EveObjType.Source, value);
		}
	}

	public Item SourceItem
	{
		get
		{
			return GetEveObj<Item>(EveObjType.Source);
		}
		set
		{
			SetEveObj(EveObjType.Source, value);
		}
	}

	public object Caster
	{
		get
		{
			return GetEveObj<object>(EveObjType.Caster);
		}
		set
		{
			SetEveObj(EveObjType.Caster, value);
		}
	}

	public Item CasterItem
	{
		get
		{
			return GetEveObj<Item>(EveObjType.Caster);
		}
		set
		{
			SetEveObj(EveObjType.Caster, value);
		}
	}

	public Enchant CasterEnchant
	{
		get
		{
			return GetEveObj<Enchant>(EveObjType.Caster);
		}
		set
		{
			SetEveObj(EveObjType.Caster, value);
		}
	}

	public Rune CasterRune
	{
		get
		{
			return GetEveObj<Rune>(EveObjType.Caster);
		}
		set
		{
			SetEveObj(EveObjType.Caster, value);
		}
	}

	public Influence CasterInfluence
	{
		get
		{
			return GetEveObj<Influence>(EveObjType.Caster);
		}
		set
		{
			SetEveObj(EveObjType.Caster, value);
		}
	}

	public ItemTagInstance CasterItemTag
	{
		get
		{
			return GetEveObj<ItemTagInstance>(EveObjType.Caster);
		}
		set
		{
			SetEveObj(EveObjType.Caster, value);
		}
	}

	public Stance CasterStance
	{
		get
		{
			return GetEveObj<Stance>(EveObjType.Caster);
		}
		set
		{
			SetEveObj(EveObjType.Caster, value);
		}
	}

	public StanceDef CasterStanceDef
	{
		get
		{
			return GetEveObj<StanceDef>(EveObjType.Caster);
		}
		set
		{
			SetEveObj(EveObjType.Caster, value);
		}
	}

	public GameSkill CasterSkill
	{
		get
		{
			return GetEveObj<GameSkill>(EveObjType.Caster);
		}
		set
		{
			SetEveObj(EveObjType.Caster, value);
		}
	}

	public SkillWord CasterSkillWord
	{
		get
		{
			return GetEveObj<SkillWord>(EveObjType.Caster);
		}
		set
		{
			SetEveObj(EveObjType.Caster, value);
		}
	}

	public WorldEvent TargetWorldEvent
	{
		get
		{
			return GetEveObj<WorldEvent>(EveObjType.Target);
		}
		set
		{
			SetEveObj(EveObjType.Target, value);
		}
	}

	public WorldEventChoice TargetWorldEventChoice
	{
		get
		{
			return GetEveObj<WorldEventChoice>(EveObjType.Destination);
		}
		set
		{
			SetEveObj(EveObjType.Destination, value);
		}
	}

	public ItemContainer DestinationContainer
	{
		get
		{
			return GetEveObj<ItemContainer>(EveObjType.Destination);
		}
		set
		{
			SetEveObj(EveObjType.Destination, value);
		}
	}

	public ItemContainer OriginContainer
	{
		get
		{
			return GetEveObj<ItemContainer>(EveObjType.Origin);
		}
		set
		{
			SetEveObj(EveObjType.Origin, value);
		}
	}

	public PivotManager.IWithPivot CasterPivot
	{
		get
		{
			if (CasterItem != null)
			{
				return CasterItem;
			}
			if (CasterEnchant != null)
			{
				return CasterEnchant.attachItem;
			}
			if (CasterRune != null)
			{
				return CasterRune.attachItem;
			}
			if (CasterInfluence != null)
			{
				return CasterInfluence;
			}
			return null;
		}
	}

	public PivotManager.IWithPivot TargetPivot
	{
		get
		{
			if (TargetItem != null)
			{
				return TargetItem;
			}
			if (TargetEnchant != null)
			{
				return TargetEnchant.attachItem;
			}
			if (TargetRune != null)
			{
				return TargetRune.attachItem;
			}
			if (TargetInfluence != null)
			{
				return TargetInfluence;
			}
			return null;
		}
	}

	public object CasterAttachObject => GetAttachObject<object>(EveObjType.Caster);

	public BattleFieldTransform SourceOwner => dealTypeSourceOwnerOverride ?? GetEveObj<BattleFieldTransform>(EveObjType.Get_SourceBftOwner);

	public BattleFieldTransform TargetOwner => GetEveObj<BattleFieldTransform>(EveObjType.Get_TargetBftOwner);

	public BattleFieldTransform OpponentBFT => gameState?.battleState?.GetOpponent(CasterOwner);

	public BattleFieldTransform CasterOwner => GetEveObj<BattleFieldTransform>(EveObjType.Get_CasterBftOwner);

	public Item NodeTempItem
	{
		get
		{
			return ((List<Item>)nodeTempValues.TryGetValue("NodeTempItemKey"))?.FirstOrDefault();
		}
		set
		{
			nodeTempValues["NodeTempItemKey"] = new List<Item> { value };
		}
	}

	public int CastNodeLoopIndex
	{
		get
		{
			return (int)nodeTempValues.TryGetValue("LoopIndexKey");
		}
		set
		{
			nodeTempValues["LoopIndexKey"] = value;
		}
	}

	public GameEvent(Type type, TriggerType triggerType)
	{
		this.type = type;
		this.triggerType = triggerType;
	}

	public List<T> GetSelectedValues<T>() where T : class
	{
		if (typeof(T) == typeof(Item))
		{
			return SelectedItems.Cast<T>().ToList();
		}
		if (typeof(T) == typeof(Enchant))
		{
			return SelectedEnchants.Cast<T>().ToList();
		}
		if (typeof(T) == typeof(Rune))
		{
			return SelectedRunes.Cast<T>().ToList();
		}
		if (typeof(T) == typeof(Influence))
		{
			return SelectedInfluences.Cast<T>().ToList();
		}
		if (typeof(T) == typeof(GameSkill))
		{
			return SelectedSkills.Cast<T>().ToList();
		}
		return null;
	}

	public void SetSelectedValues<T>(List<T> value) where T : class
	{
		if (typeof(T) == typeof(Item))
		{
			SelectedItems = value.Cast<Item>().ToList();
		}
		else if (typeof(T) == typeof(Enchant))
		{
			SelectedEnchants = value.Cast<Enchant>().ToList();
		}
		else if (typeof(T) == typeof(Rune))
		{
			SelectedRunes = value.Cast<Rune>().ToList();
		}
		else if (typeof(T) == typeof(Influence))
		{
			SelectedInfluences = value.Cast<Influence>().ToList();
		}
		else if (typeof(T) == typeof(GameSkill))
		{
			SelectedSkills = value.Cast<GameSkill>().ToList();
		}
	}

	public static bool IsResolveGameEventType(Type type)
	{
		return type >= Type.AddItemCounterResolveInfo;
	}

	public void SetCaster(object obj)
	{
		if (GetEveObj(EveObjType.Caster) != null)
		{
			SetEveObj(EveObjType.Caster, null);
		}
		SetEveObj(EveObjType.Caster, obj);
	}

	public void SetEveObj(EveObjType type, object obj)
	{
		if (type >= EveObjType.Get_CasterAttach)
		{
			throw new Exception("不能设置Get_类型的EveObj");
		}
		if (eveObjDic.ContainsKey(type) && eveObjDic[type] != null && obj != null)
		{
			throw new Exception("重复的事件对象类型");
		}
		eveObjDic[type] = obj;
	}

	public object GetEveObj(EveObjType type)
	{
		return GetEveObj<object>(type);
	}

	public T GetEveObj<T>(EveObjType type) where T : class
	{
		switch (type)
		{
		case EveObjType.Caster:
		case EveObjType.Source:
		case EveObjType.Target:
		case EveObjType.Destination:
		case EveObjType.Origin:
			if (eveObjDic.ContainsKey(type) && eveObjDic[type] is T result)
			{
				return result;
			}
			return null;
		case EveObjType.Get_CasterAttach:
			return GetEveObj<IAttachment>(EveObjType.Caster)?.AttachTarget as T;
		case EveObjType.Get_SourceAttach:
			return GetEveObj<IAttachment>(EveObjType.Source)?.AttachTarget as T;
		case EveObjType.Get_TargetAttach:
			return GetEveObj<IAttachment>(EveObjType.Target)?.AttachTarget as T;
		case EveObjType.Get_DestinationAttach:
			return GetEveObj<IAttachment>(EveObjType.Destination)?.AttachTarget as T;
		case EveObjType.Get_OriginAttach:
			return GetEveObj<IAttachment>(EveObjType.Origin)?.AttachTarget as T;
		case EveObjType.Get_CasterBftOwner:
			return GetEveObj<IBftOwnable>(EveObjType.Caster)?.OwnerBft as T;
		case EveObjType.Get_SourceBftOwner:
			return GetEveObj<IBftOwnable>(EveObjType.Source)?.OwnerBft as T;
		case EveObjType.Get_TargetBftOwner:
			return GetEveObj<IBftOwnable>(EveObjType.Target)?.OwnerBft as T;
		case EveObjType.Get_DestinationBftOwner:
			return GetEveObj<IBftOwnable>(EveObjType.Destination)?.OwnerBft as T;
		case EveObjType.Get_OriginBftOwner:
			return GetEveObj<IBftOwnable>(EveObjType.Origin)?.OwnerBft as T;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}

	public T GetAttachObject<T>(EveObjType type) where T : class
	{
		if (eveObjDic.ContainsKey(type) && eveObjDic[type] is IAttachment { AttachTarget: not null, AttachTarget: T attachTarget })
		{
			return attachTarget;
		}
		return null;
	}

	public bool CheckDealType(DealType dealType)
	{
		object eveObj = GetEveObj(EveObjType.Caster);
		object eveObj2 = GetEveObj(EveObjType.Target);
		object eveObj3 = GetEveObj(EveObjType.Source);
		object casterAttachObject = CasterAttachObject;
		BattleFieldTransform casterOwner = CasterOwner;
		BattleFieldTransform opponentBFT = OpponentBFT;
		BattleFieldTransform targetOwner = TargetOwner;
		BattleFieldTransform sourceOwner = SourceOwner;
		switch (dealType)
		{
		case DealType.Any:
			return true;
		case DealType.OwnerToOpponent:
			if (casterOwner != null && targetOwner != null && sourceOwner == casterOwner)
			{
				return targetOwner == opponentBFT;
			}
			return false;
		case DealType.OpponentToOwner:
			if (sourceOwner != null && targetOwner != null && sourceOwner == opponentBFT)
			{
				return targetOwner == casterOwner;
			}
			return false;
		case DealType.OwnerToOwner:
			if (sourceOwner != null && targetOwner != null && sourceOwner == casterOwner)
			{
				return targetOwner == casterOwner;
			}
			return false;
		case DealType.OpponentToOpponent:
			if (sourceOwner != null && targetOwner != null && sourceOwner == opponentBFT)
			{
				return targetOwner == opponentBFT;
			}
			return false;
		case DealType.CasterToOwner:
			if (eveObj != null && targetOwner != null && eveObj3 == eveObj)
			{
				return targetOwner == casterOwner;
			}
			return false;
		case DealType.CasterToOpponent:
			if (eveObj != null && targetOwner != null && eveObj3 == eveObj)
			{
				return targetOwner == opponentBFT;
			}
			return false;
		case DealType.ToOwner:
			if (targetOwner != null)
			{
				return targetOwner == casterOwner;
			}
			return false;
		case DealType.ToOpponent:
			if (targetOwner != null)
			{
				return targetOwner == opponentBFT;
			}
			return false;
		case DealType.ToCaster:
			if (eveObj2 != null)
			{
				return eveObj2 == eveObj;
			}
			return false;
		case DealType.OpponentToCaster:
			if (sourceOwner != null && opponentBFT != null && sourceOwner == opponentBFT)
			{
				return eveObj2 == eveObj;
			}
			return false;
		case DealType.OwnerToCaster:
			if (sourceOwner != null && casterOwner != null && sourceOwner == casterOwner)
			{
				return eveObj2 == eveObj;
			}
			return false;
		case DealType.FromOwner:
			if (sourceOwner != null && casterOwner != null)
			{
				return sourceOwner == casterOwner;
			}
			return false;
		case DealType.FromOpponent:
			if (sourceOwner != null && opponentBFT != null)
			{
				return sourceOwner == opponentBFT;
			}
			return false;
		case DealType.FromCaster:
			if (eveObj3 != null && eveObj != null)
			{
				return eveObj3 == eveObj;
			}
			return false;
		case DealType.FromPlayer:
			return sourceOwner?.isPlayer ?? false;
		case DealType.FromEnemy:
			return sourceOwner?.isEnemy ?? false;
		case DealType.PlayerToEnemy:
			if (sourceOwner != null && targetOwner != null && sourceOwner.isPlayer)
			{
				return targetOwner.isEnemy;
			}
			return false;
		case DealType.EnemyToPlayer:
			if (sourceOwner != null && targetOwner != null && sourceOwner.isEnemy)
			{
				return targetOwner.isPlayer;
			}
			return false;
		case DealType.ToPlayer:
			return targetOwner?.isPlayer ?? false;
		case DealType.ToEnemy:
			return targetOwner?.isEnemy ?? false;
		case DealType.CasterAttachToOpponent:
			if (sourceOwner != null && targetOwner != null && eveObj3 == casterAttachObject)
			{
				return targetOwner == opponentBFT;
			}
			return false;
		case DealType.CasterAttachToOwner:
			if (sourceOwner != null && targetOwner != null && eveObj3 == casterAttachObject)
			{
				return targetOwner == casterOwner;
			}
			return false;
		case DealType.ToCasterAttach:
			if (eveObj2 != null)
			{
				return eveObj2 == casterAttachObject;
			}
			return false;
		case DealType.OpponentToCasterAttach:
			if (sourceOwner != null && eveObj2 != null && sourceOwner == opponentBFT)
			{
				return eveObj2 == casterAttachObject;
			}
			return false;
		case DealType.OwnerToCasterAttach:
			if (sourceOwner != null && eveObj2 != null && sourceOwner == casterOwner)
			{
				return eveObj2 == casterAttachObject;
			}
			return false;
		case DealType.FormCasterAttach:
			if (casterAttachObject != null)
			{
				return eveObj3 == casterAttachObject;
			}
			return false;
		default:
			return false;
		}
	}

	public ItemContainer GetOwnerContainer(EventBFTType bftType, ContainerType type)
	{
		BattleFieldTransform eventBFT = GetEventBFT(bftType);
		switch (type)
		{
		case ContainerType.TableContainer:
			if (triggerType == TriggerType.OutBattleDisplay)
			{
				return null;
			}
			return eventBFT?.tableContainer;
		case ContainerType.DrawPileContainer:
			if (triggerType == TriggerType.OutBattleDisplay)
			{
				return null;
			}
			return eventBFT?.drawPileItemContainer;
		case ContainerType.WastePileContainer:
			if (triggerType == TriggerType.OutBattleDisplay)
			{
				return null;
			}
			return eventBFT?.wastePileItemContainer;
		case ContainerType.DeckContainer:
			return eventBFT?.deckContainer;
		case ContainerType.ExilePileContainer:
			if (triggerType == TriggerType.OutBattleDisplay)
			{
				return null;
			}
			return eventBFT?.exilePileItemContainer;
		case ContainerType.CompanionContainer:
			if (triggerType == TriggerType.OutBattleDisplay)
			{
				return null;
			}
			return eventBFT?.companionContainer;
		default:
			return null;
		}
	}

	public BattleFieldTransform GetEventBFT(EventBFTType type)
	{
		return type switch
		{
			EventBFTType.None => null, 
			EventBFTType.Owner => CasterOwner, 
			EventBFTType.Opponent => OpponentBFT, 
			EventBFTType.Target => TargetOwner, 
			EventBFTType.Source => SourceOwner, 
			EventBFTType.AddItemOwner => addItemInfo?.addItem.OwnerBft, 
			EventBFTType.OverloadOwner => TargetItem?.OwnerBft, 
			EventBFTType.Player => gameState?.playerState?.bft, 
			EventBFTType.Enemy => gameState?.battleState?.enemy?.bft, 
			EventBFTType.Origin => GetEveObj<BattleFieldTransform>(EveObjType.Origin), 
			EventBFTType.Destination => GetEveObj<BattleFieldTransform>(EveObjType.Destination), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public Item GetEventItem(EventItemType type)
	{
		switch (type)
		{
		case EventItemType.CasterItem:
			return CasterItem;
		case EventItemType.SourceItem:
			return SourceItem;
		case EventItemType.TargetItem:
			return TargetItem;
		case EventItemType.SpellItem:
			return spellInfo?.spellItem;
		case EventItemType.AddItem:
			return addItemInfo?.addItem;
		case EventItemType.OverloadItem:
			return addItemInfo?.addItem;
		case EventItemType.CasterAimItem:
			try
			{
				return CasterItem?.aimItem;
			}
			catch
			{
				return null;
			}
		case EventItemType.DamagedItem:
			return damage?.targetItem;
		case EventItemType.CasterAttachingItem:
			if (CasterAttachObject is Item)
			{
				return CasterAttachObject as Item;
			}
			break;
		case EventItemType.NodeTempItem:
			return NodeTempItem;
		case EventItemType.DestinationItem:
			return GetEveObj<Item>(EveObjType.Destination);
		}
		return null;
	}

	public List<Item> GetEventItems(EventBFTType bftType, OwnerItemsType itemType)
	{
		BattleFieldTransform eventBFT = GetEventBFT(bftType);
		if (eventBFT == null)
		{
			return null;
		}
		switch (itemType)
		{
		case OwnerItemsType.AllReal:
			return eventBFT.GetAllRealItems();
		case OwnerItemsType.TableItems:
			return eventBFT.tableContainer.GetAllItems();
		case OwnerItemsType.DrawPileItems:
			return eventBFT.drawPileItemContainer.GetAllItems();
		case OwnerItemsType.WastePileItems:
			return eventBFT.wastePileItemContainer.GetAllItems();
		case OwnerItemsType.Deck:
			return eventBFT.deckContainer.GetAllItems();
		case OwnerItemsType.ExilePileItems:
			if (triggerType == TriggerType.OutBattleDisplay)
			{
				return null;
			}
			return eventBFT.exilePileItemContainer?.GetAllItems();
		case OwnerItemsType.AllItemsIncludeDeck:
		{
			List<Item> list = new List<Item>();
			list.AddRange(eventBFT.GetAllRealItems());
			list.AddRange(eventBFT.deckContainer.GetAllItems());
			return list;
		}
		case OwnerItemsType.Companion:
			return eventBFT.companionContainer.GetAllItems();
		default:
			return null;
		}
	}

	public bool IsInstantTriggerEvent()
	{
		if (type == Type.BeforeGameStart || type == Type.BattleStart || type == Type.BeforeBattleEnd || type == Type.PlayerTurnStart || type == Type.PlayerTurnEnd || type == Type.EnemyTurnStart || type == Type.EnemyTurnEnd || type == Type.ExitLocation || type == Type.EnterLocation || type == Type.ExitCity || type == Type.EnterCity)
		{
			return false;
		}
		return true;
	}

	public bool IsResolveGameEvent()
	{
		return IsResolveGameEventType(type);
	}

	public bool IsDisplayEvent()
	{
		if (type == Type.Display)
		{
			return true;
		}
		return false;
	}

	public bool IsBattleDisplay()
	{
		if (!IsDisplayEvent())
		{
			return false;
		}
		if (CasterEnchant != null)
		{
			return false;
		}
		if (CasterRune != null)
		{
			return false;
		}
		if (CasterInfluence != null)
		{
			return false;
		}
		if (CasterSkill != null)
		{
			return false;
		}
		if (CasterItem != null)
		{
			ItemContainer.ContainerType? containerType = CasterItem.owner?.type;
			if (!containerType.HasValue)
			{
				return false;
			}
			if (containerType == ItemContainer.ContainerType.Table || containerType == ItemContainer.ContainerType.DrawPile || containerType == ItemContainer.ContainerType.WastePile || containerType == ItemContainer.ContainerType.Companion)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public GameEvent ShallowCopy()
	{
		GameEvent obj = MemberwiseClone() as GameEvent;
		obj.eveObjDic = new Dictionary<EveObjType, object>(obj.eveObjDic);
		obj.nodeTempValues = new Dictionary<string, object>(obj.nodeTempValues);
		obj.SelectedItems = new List<Item>(obj.SelectedItems);
		return obj;
	}
}
