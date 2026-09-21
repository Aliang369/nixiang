using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class PlayerGameStateInfo : InGameManager
{
	public const int InGameCurrencyUpperBound = 999;

	private const string WarChanterCharacterGuid = "54dc4a3f4afca9c45bfefb5517bf4c7f";

	private const string WarChanterSwordTagGuid = "04731c1b57be21b4084615a246151c9b";

	private const string WarChanterMusicTagGuid = "d6b2a3d93e966504babd27ae6e1bd869";

	private const string WingedMercenaryCharacterGuid = "bfd05c3dbfdfbe3409c96f4959e6ec4a";

	private const string FearlessWarriorCharacterGuid = "79cf9dd341e6629449f337136426e032";

	private const string FireControllerCharacterGuid = "2810595260b066f419ca9777065d8509";

	private const string ElectricScholarCharacterGuid = "7a2194cdc8cc1a1488b9681c2696552c";

	private const string CourtMusicianCharacterGuid = "33ca50369890ba6488995c156e024edd";

	private const string FrostWatcherCharacterGuid = "4caefba73f8be2040a70cdadd955164a";

	private const string FrostWatcherAcceleratedLockEnchantGuid = "d176ccd46cbb21e4b962c4d89f292558";

	private const string HarmonyMusicianCharacterGuid = "c9f41602ca13679489c65d2f305bde82";

	private const string HarmonyMusicianFugueTalentGuid = "5d2824c3493d063d48f70f3a1db02a43";

	private const string HarmonyMusicianFantasiaTalentGuid = "dc84552fec37c5d6fb5a830da39c666d";

	private const string RangerCharacterGuid = "a0951dabc0c553e4bba1ddc7797c3bce";

	private const string WanfaCharacterGuid = "30c29e6605d71ca4d922eb476859b2e2";

	private const string GarrisonCharacterGuid = "53b7d9c8778a57141a93c1aaf7b971db";

	private const string SpiritCallerCharacterGuid = "f3125525ebd385c46a52add92f99a074";

	private const string SmallKnightCharacterGuid = "dfc28fc6a2956471fa695c08982e99bf";

	private const string DancerCharacterGuid = "b11c723c4a9b4bc42bd8b2e17e05a29f";

	private const string ShadowDancerCharacterGuid = "eb2dc678e34a8a541924029d60bde298";

	private const string FrenziedDancerCharacterGuid = "39ccf3c15c79ad44f8d15f6929e380ba";

	private const string ReturnerCharacterGuid = "614a19aa42c16f544b03a62015de4e03";

	private const string DanceStepTagGuid = "f3715ebbdd3d5474a92986278633149b";

	private const string TemporaryEmpowerEnchantGuid = "f9e88d7c3a394964cb007a19d87b93d4";

	private const int FireControllerTempHealthPerExile = 5;

	private const string FleshAndIronExchangeInfluenceGuid = "5aa6bb074baa8d6e890e34a2569efcc1";

	private const float FearlessWarriorDamageBonusPercentPerHit = 20f;

	private const float FearlessWarriorDamageBonusPercentPerHitWithFleshAndIronExchange = 10f;

	public const string OutcastEnvoyCharacterGuid = "5c00e2c5c7da49440929aa48f0c166b2";

	public static readonly IReadOnlyList<string> ElementItemTagGuids = new string[3] { "77c86291a66effd46854b42f82dd32a3", "f0a277c28722d954c83ddac1d1a769fc", "93afc9e6f0614d0438412ac4966da745" };

	public static readonly IReadOnlyList<string> MissingWillPowerInfluenceIds = new string[7] { "1a7f7b5d6c15028558f21e3a469fe638", "9a35b35f4e35da156a56138de9244d97", "61a4510e14126e908893c0de366bf070", "47a55165bf2580b9a919c30fac51d4eb", "830e92f19191a6e2c8a6151e4325a2e0", "6c145f3c296b2cbd4985ae47e8a8d0ea", "828f51a677997e6a6b3dbe5920342f4d" };

	[SaveField(null, null)]
	public List<TalentDef> activatedPotentialTalents;

	[SaveField(null, null)]
	public BattleFieldTransform bft;

	[SaveField(null, null)]
	public ProtectedInt coinCount;

	[SaveField(null, null)]
	public ProtectedInt crystalCount;

	public RealSpellCastInfo playerSpellInfo;

	[SaveField(null, null)]
	public GameSkillPreset prepareSkillsPreset;

	[SaveField(null, null)]
	public List<GameSkillSlot> activeSkills_inSlot;

	[SaveField(null, null)]
	public List<GameSkill> activatedSkills_outSlot;

	[SaveField(null, null)]
	public List<GameSkill> consumedSkills;

	[SaveField(null, null)]
	public bool deathPendingReviveUsed;

	[SaveField(null, null)]
	public int highestGrantedMissingWillPowerLevel;

	[SaveField(null, null)]
	public ProtectedInt additionalWillPowerCost;

	[SaveField(null, null)]
	public int fearlessWarriorDamageBonusThisTurn;

	[SaveField(null, null)]
	public int rangerSlotBonusThisTurn;

	[SaveField(null, null)]
	public int wanfaDamageBonusThisTurn;

	[SaveField(null, null)]
	public bool spiritCallerFreeCompanionUsedThisTurn;

	[SaveField(null, null)]
	public int frenziedDancerShuffleCountThisTurn;

	[SaveField(null, null)]
	public int pendingMusicianTempHealth;

	[SaveField(null, null)]
	public int riftNormalVariantBStage;

	[SaveField(null, null)]
	public List<string> usedRiftGuidanceIds;

	[SaveField(null, null)]
	public List<RiftBossEffectState> pendingRiftBossEffects;

	[SaveField(null, null)]
	public bool pendingRiftBossExtraPlayerTurn;

	[SaveField(null, null)]
	public bool riftGuidanceDrawAfterMoveUsedThisTurn;

	[NonSerialized]
	private int activeRiftBossMaxHealthDelta;

	private bool resolvingFrostWatcherExtraTurnEndEffects;

	private List<Item> pendingHarmonyMusicianFugueResetChapters;

	[NonSerialized]
	private HashSet<Item> shadowDancerEnchantProtectedItems;

	[SaveField(null, null)]
	public int skillLimit;

	[SaveField(null, null)]
	public ProfileSkillPreset skillPreset;

	[SaveField(null, null)]
	public CharacterGameInitConfig characterGameConfig;

	[SaveField(null, null)]
	public PlayerCharacterState character;

	public static int MaxMissingWillPowerLevel => MissingWillPowerInfluenceIds.Count;

	public PlayerCharacterDef CharacterDef => character.def;

	public CharacterMastery MasteryInThisGame => base.gameState.newGameConfigData.GetMastery(base.gameState.applicationState);

	public IReadOnlyList<GameSkill> PreparableSkills => (from s in prepareSkillsPreset.gameSkillSlots
		where s.gameSkill != null
		select s.gameSkill).ToList();

	public IEnumerable<GameSkill> ActivatedSkills_inSlot => from s in activeSkills_inSlot
		where s.gameSkill != null
		select s.gameSkill;

	public IEnumerable<GameSkill> ActivatedSkills => activatedSkills_outSlot.Concat(ActivatedSkills_inSlot);

	public IEnumerable<GameSkill> AllSkills => ActivatedSkills.Concat(PreparableSkills).Concat(consumedSkills);

	public IEnumerable<GameSkillSlot> AllSkillsSlot => activeSkills_inSlot.Concat(activatedSkills_outSlot.Select((GameSkill s) => new GameSkillSlot
	{
		gameSkill = s
	}));

	public List<TalentDef> CurrentActivateAblePotentialTalent
	{
		get
		{
			List<TalentDef> list = new List<TalentDef>();
			list.AddRange(from t in character.GetActivateAblePotentialTalents()
				where !activatedPotentialTalents.Contains(t)
				select t);
			IEnumerable<TalentDef> collection = from c in base.gameState.applicationState.characterManager.characterStates
				where !c.locked && c.def.sharePotential != null && c.EnableSharePotential && !activatedPotentialTalents.Contains(c.def.sharePotential) && c.def != base.gameState.playerState.character.def
				select c.def.sharePotential;
			list.AddRange(collection);
			return list;
		}
	}

	public CharacterClassState CharacterClassState => base.gameState.applicationState.characterManager.GetCharacterClassState(CharacterDef.attachClass);

	public CharacterClassType CharacterClass => CharacterDef.attachClass.mClass;

	public ItemContainer TableContainer => bft.tableContainer;

	public ItemContainer DrawPileContainer => bft.drawPileItemContainer;

	public ItemContainer WastePileContainer => bft.wastePileItemContainer;

	public ItemContainer DeckContainer => bft.deckContainer;

	private bool IsWarChanter => character?.def?.id == "54dc4a3f4afca9c45bfefb5517bf4c7f";

	private bool IsWingedMercenary => character?.def?.id == "bfd05c3dbfdfbe3409c96f4959e6ec4a";

	private bool IsFearlessWarrior => character?.def?.id == "79cf9dd341e6629449f337136426e032";

	private bool IsFireController => character?.def?.id == "2810595260b066f419ca9777065d8509";

	private bool IsElectricScholar => character?.def?.id == "7a2194cdc8cc1a1488b9681c2696552c";

	private bool IsCourtMusician => character?.def?.id == "33ca50369890ba6488995c156e024edd";

	private bool IsFrostWatcher => character?.def?.id == "4caefba73f8be2040a70cdadd955164a";

	private bool IsHarmonyMusician => character?.def?.id == "c9f41602ca13679489c65d2f305bde82";

	private bool IsRanger => character?.def?.id == "a0951dabc0c553e4bba1ddc7797c3bce";

	private bool IsWanfa => character?.def?.id == "30c29e6605d71ca4d922eb476859b2e2";

	private bool IsGarrison => character?.def?.id == "53b7d9c8778a57141a93c1aaf7b971db";

	private bool IsSpiritCaller => character?.def?.id == "f3125525ebd385c46a52add92f99a074";

	private bool IsSmallKnight => character?.def?.id == "dfc28fc6a2956471fa695c08982e99bf";

	private bool IsDancer => character?.def?.id == "b11c723c4a9b4bc42bd8b2e17e05a29f";

	private bool IsShadowDancer => character?.def?.id == "eb2dc678e34a8a541924029d60bde298";

	private bool IsFrenziedDancer => character?.def?.id == "39ccf3c15c79ad44f8d15f6929e380ba";

	private bool IsReturner => character?.def?.id == "614a19aa42c16f544b03a62015de4e03";

	private bool IsHarmonyMusicianFugueActivated => activatedPotentialTalents?.Any((TalentDef t) => t?.id == "5d2824c3493d063d48f70f3a1db02a43") ?? false;

	private bool IsHarmonyMusicianFantasiaActivated => activatedPotentialTalents?.Any((TalentDef t) => t?.id == "dc84552fec37c5d6fb5a830da39c666d") ?? false;

	public int CurrentUsedWillPower => base.gameState.playerState.bft.influences.Sum((Influence inf) => inf.willPowerCost) + Mathf.Max(additionalWillPowerCost, 0);

	public int RemainWillPower => characterGameConfig.initWillPower - CurrentUsedWillPower;

	public int MaxWillPower => characterGameConfig.initWillPower;

	public override void Init(GameStateInfo gameState)
	{
		if (bft == null)
		{
			bft = new BattleFieldTransform(base.gameState, isPlayer: true);
		}
		bft.EnsureRuntimeContainers(gameState);
		if (prepareSkillsPreset == null)
		{
			prepareSkillsPreset = new GameSkillPreset();
		}
		if (activatedSkills_outSlot == null)
		{
			activatedSkills_outSlot = new List<GameSkill>();
		}
		if (activeSkills_inSlot == null)
		{
			activeSkills_inSlot = new List<GameSkillSlot>();
			for (int i = 0; i < skillLimit; i++)
			{
				activeSkills_inSlot.Add(new GameSkillSlot());
			}
		}
		if (activatedPotentialTalents == null)
		{
			activatedPotentialTalents = new List<TalentDef>();
		}
		if (consumedSkills == null)
		{
			consumedSkills = new List<GameSkill>();
		}
		if (usedRiftGuidanceIds == null)
		{
			usedRiftGuidanceIds = new List<string>();
		}
		if (pendingRiftBossEffects == null)
		{
			pendingRiftBossEffects = new List<RiftBossEffectState>();
		}
		pendingHarmonyMusicianFugueResetChapters = null;
		if (shadowDancerEnchantProtectedItems == null)
		{
			shadowDancerEnchantProtectedItems = new HashSet<Item>();
		}
		RemoveInvalidSavedSkills();
		gameState.playerBftDisplay.Init(bft);
		playerSpellInfo = new RealSpellCastInfo(gameState);
		base.gameState.actionManager.actionQueueCleanEvent += CheckDeath;
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, ResetFearlessWarriorDamageBonus);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, ResetXiaKeCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, ResetFrostWatcherExtraTurnEndEffects);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, ApplyRiftBossEffectsOnBattleStart);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, ClearHarmonyMusicianFugueResetPending);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BftDeathCheckResolve, ResolveRiftBossFailureRevive);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.PlayerTurnStart, ResetXiaKeCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.EnemyTurnStart, ResetXiaKeCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, ResetFearlessWarriorDamageBonus);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, ResetXiaKeCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, ResetFrostWatcherExtraTurnEndEffects);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, ClearRiftBossEffectsOnBattleEnd);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, ClearRiftBossExtraPlayerTurn);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, ClearHarmonyMusicianFugueResetPending);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BftHealthChange, OnFearlessWarriorHealthChange);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterDamage, OnFearlessWarriorArmorDamaged);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BftHealthChange, OnGarrisonHealthChange);
		base.gameState.applicationState.eventManager.gameEvent.AddGlobalListener(TriggerFrostWatcherExtraTurnEndEffects);
		base.gameState.applicationState.eventManager.gameEvent.AddGlobalListener(TriggerWarChanterSwordEcho);
		base.gameState.applicationState.eventManager.gameEvent.AddGlobalListener(ResolveSpiritCallerFreeCompanionCost);
		base.gameState.applicationState.eventManager.gameEvent.AddGlobalListener(QueueHarmonyMusicianFugueResetAfterFinalChapter);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.GenerateDamageDamageValueResolve, ResolveFearlessWarriorDamageBonus);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.GenerateDamageDamageValueResolve, ResolveWanfaDamageBonus);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.GenerateDamageDamageValueResolve, ResolveElectricScholarSameElementDamageBonus);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.FixedSlotCountResolve, ResolveRangerFixedSlotCountBonus);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.FixedSlotCountResolve, ResolveWingedMercenaryFixedSlotCountBonus);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.FixedSlotCountResolve, ResolveCourtMusicianFixedSlotCountBonus);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.FixedSlotCountResolve, ResolveRiftBossFixedSlotCountBonus);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.TurnDrawResolve, ResolveRiftBossTurnDrawBonus);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.ExileItem, OnPlayerExileItem);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.PlayerTurnStart, ResetHarmonyMusicianFinishedMusicChapters);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.PlayerTurnStart, ApplyPendingMusicianTempHealth);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.EnemyTurnStart, ResetHarmonyMusicianFugueFinishedMusicChaptersBeforeEnemyTurn);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AddChapterResolve, ResolveHarmonyMusicianMusicChapterOnTurnEnd);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.ConvertYinYangPower, OnRangerConvertYinYangPower);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.ChangeYinYangPower, OnWanfaChangeYinYangPower);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterSpellCast, OnSpiritCallerAfterSpellCast);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterSwapItem, OnSmallKnightAfterSwapItem);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.MoveSlotIndex, OnDancerAscendingMove);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.MoveSlotIndex, OnRiftGuidanceCardMove);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterSwapItem, OnDancerAfterSwapItem);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.ChangeEnchantResolve, ResolveShadowDancerFirstEnchantConsumption);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterTriggerCustomEvent, OnFrenziedDancerAfterFrenzy);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.GenerateDamageDamageValueResolve, ResolveFrenziedDancerFrenzyDamage);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterSwapItem, OnReturnerAfterSwapItem);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.PlayerTurnStart, ResetDancerCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, ResetDancerCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, ResetDancerCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, ClearPendingMusicianTempHealth);
	}

	public void ResetPlayerSpellRuntimeState()
	{
		playerSpellInfo = new RealSpellCastInfo(base.gameState);
	}

	private void RemoveInvalidSavedSkills()
	{
		foreach (GameSkillSlot item in activeSkills_inSlot)
		{
			if (item.gameSkill != null && item.gameSkill.skillDef == null)
			{
				item.SetSkill(null);
				TraceLog.Error("[PlayerGameStateInfo] activeSkills_inSlot - invalid skill");
			}
		}
		foreach (GameSkill item2 in activatedSkills_outSlot.ToList())
		{
			if (item2?.skillDef == null)
			{
				activatedSkills_outSlot.Remove(item2);
				TraceLog.Error("[PlayerGameStateInfo] activatedSkills_outSlot - invalid skill");
			}
		}
		foreach (GameSkill item3 in consumedSkills.ToList())
		{
			if (item3?.skillDef == null)
			{
				activatedSkills_outSlot.Remove(item3);
				TraceLog.Error("[PlayerGameStateInfo] consumedSkills - invalid skill");
			}
		}
	}

	public override void Dispose()
	{
		pendingHarmonyMusicianFugueResetChapters = null;
		base.gameState.actionManager.actionQueueCleanEvent -= CheckDeath;
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, ResetFearlessWarriorDamageBonus);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, ResetXiaKeCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, ResetFrostWatcherExtraTurnEndEffects);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, ApplyRiftBossEffectsOnBattleStart);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, ClearHarmonyMusicianFugueResetPending);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BftDeathCheckResolve, ResolveRiftBossFailureRevive);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.PlayerTurnStart, ResetXiaKeCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.EnemyTurnStart, ResetXiaKeCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, ResetFearlessWarriorDamageBonus);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, ResetXiaKeCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, ResetFrostWatcherExtraTurnEndEffects);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, ClearRiftBossEffectsOnBattleEnd);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, ClearRiftBossExtraPlayerTurn);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, ClearHarmonyMusicianFugueResetPending);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BftHealthChange, OnFearlessWarriorHealthChange);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterDamage, OnFearlessWarriorArmorDamaged);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BftHealthChange, OnGarrisonHealthChange);
		base.gameState.applicationState.eventManager.gameEvent.RemoveGlobalListener(TriggerFrostWatcherExtraTurnEndEffects);
		base.gameState.applicationState.eventManager.gameEvent.RemoveGlobalListener(TriggerWarChanterSwordEcho);
		base.gameState.applicationState.eventManager.gameEvent.RemoveGlobalListener(ResolveSpiritCallerFreeCompanionCost);
		base.gameState.applicationState.eventManager.gameEvent.RemoveGlobalListener(QueueHarmonyMusicianFugueResetAfterFinalChapter);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.GenerateDamageDamageValueResolve, ResolveFearlessWarriorDamageBonus);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.GenerateDamageDamageValueResolve, ResolveWanfaDamageBonus);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.GenerateDamageDamageValueResolve, ResolveElectricScholarSameElementDamageBonus);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.FixedSlotCountResolve, ResolveRangerFixedSlotCountBonus);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.FixedSlotCountResolve, ResolveWingedMercenaryFixedSlotCountBonus);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.FixedSlotCountResolve, ResolveCourtMusicianFixedSlotCountBonus);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.FixedSlotCountResolve, ResolveRiftBossFixedSlotCountBonus);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.TurnDrawResolve, ResolveRiftBossTurnDrawBonus);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.ExileItem, OnPlayerExileItem);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.PlayerTurnStart, ResetHarmonyMusicianFinishedMusicChapters);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.PlayerTurnStart, ApplyPendingMusicianTempHealth);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.EnemyTurnStart, ResetHarmonyMusicianFugueFinishedMusicChaptersBeforeEnemyTurn);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AddChapterResolve, ResolveHarmonyMusicianMusicChapterOnTurnEnd);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.ConvertYinYangPower, OnRangerConvertYinYangPower);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.ChangeYinYangPower, OnWanfaChangeYinYangPower);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterSpellCast, OnSpiritCallerAfterSpellCast);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterSwapItem, OnSmallKnightAfterSwapItem);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.MoveSlotIndex, OnDancerAscendingMove);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.MoveSlotIndex, OnRiftGuidanceCardMove);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterSwapItem, OnDancerAfterSwapItem);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.ChangeEnchantResolve, ResolveShadowDancerFirstEnchantConsumption);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterTriggerCustomEvent, OnFrenziedDancerAfterFrenzy);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.GenerateDamageDamageValueResolve, ResolveFrenziedDancerFrenzyDamage);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterSwapItem, OnReturnerAfterSwapItem);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.PlayerTurnStart, ResetDancerCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BattleStart, ResetDancerCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, ResetDancerCharacterFeatureState);
		base.gameState.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, ClearPendingMusicianTempHealth);
	}

	public void Setup(CharacterGameInitConfig characterGameInitConfig, PlayerCharacterState character)
	{
		characterGameConfig = characterGameInitConfig;
		this.character = character;
		int num = characterGameConfig?.characterDef?.initHealth ?? (character?.def?.initHealth).GetValueOrDefault();
		int num2 = Mathf.Max(characterGameInitConfig?.initHealth ?? num, 1);
		if (characterGameInitConfig == null || characterGameInitConfig.initHealth <= 0)
		{
			TraceLog.Warn($"[PlayerGameStateInfo] Invalid init health detected, fallback to {num2}.");
		}
		bft.health = num2;
		bft.maxHealth = num2;
		bft.turnDrawItemCount = characterGameInitConfig?.turnDrawCount ?? 0;
		additionalWillPowerCost = 0;
		fearlessWarriorDamageBonusThisTurn = 0;
		rangerSlotBonusThisTurn = 0;
		wanfaDamageBonusThisTurn = 0;
		spiritCallerFreeCompanionUsedThisTurn = false;
		frenziedDancerShuffleCountThisTurn = 0;
		shadowDancerEnchantProtectedItems?.Clear();
		riftGuidanceDrawAfterMoveUsedThisTurn = false;
		resolvingFrostWatcherExtraTurnEndEffects = false;
	}

	private void ResetXiaKeCharacterFeatureState(GameEvent.Type _, GameEvent eve)
	{
		rangerSlotBonusThisTurn = 0;
		wanfaDamageBonusThisTurn = 0;
		spiritCallerFreeCompanionUsedThisTurn = false;
	}

	private void ResetDancerCharacterFeatureState(GameEvent.Type _, GameEvent eve)
	{
		frenziedDancerShuffleCountThisTurn = 0;
		shadowDancerEnchantProtectedItems?.Clear();
		riftGuidanceDrawAfterMoveUsedThisTurn = false;
	}

	private void ClearRiftBossExtraPlayerTurn(GameEvent.Type _, GameEvent eve)
	{
		pendingRiftBossExtraPlayerTurn = false;
	}

	private bool HasRiftGuidance(string name)
	{
		BattleFieldTransform battleFieldTransform = bft;
		if (battleFieldTransform == null)
		{
			return false;
		}
		return battleFieldTransform.influences?.Any((Influence influence) => influence?.def?.InfluenceName == name) == true;
	}

	private void OnRiftGuidanceCardMove(GameEvent.Type _, GameEvent eve)
	{
		if (HasRiftGuidance("影舞者的指引") && !riftGuidanceDrawAfterMoveUsedThisTurn)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && base.gameState.battleState.turnPhase == TurnPhase.PlayerControlPhase && eve.SourceOwner == bft && eve.TargetItem?.OwnerBft == bft && eve.deltaValue > 0)
			{
				riftGuidanceDrawAfterMoveUsedThisTurn = true;
				base.gameState.actionManager.EnqueueAction(new DrawFromPileAction(bft, null));
			}
		}
	}

	public void AddPendingMusicianTempHealth(int amount)
	{
		if (amount > 0)
		{
			pendingMusicianTempHealth = CombatValueLimit.Clamp((long)pendingMusicianTempHealth + (long)amount);
		}
	}

	private void ApplyPendingMusicianTempHealth(GameEvent.Type _, GameEvent eve)
	{
		if (pendingMusicianTempHealth > 0)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true)
			{
				int delta = pendingMusicianTempHealth;
				pendingMusicianTempHealth = 0;
				base.gameState.actionManager.EnqueueAction(new ChangeTempHealthAction(delta, bft));
			}
		}
	}

	private void ClearPendingMusicianTempHealth(GameEvent.Type _, GameEvent eve)
	{
		pendingMusicianTempHealth = 0;
	}

	private void OnDancerAscendingMove(GameEvent.Type _, GameEvent eve)
	{
		if (IsDancer)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && base.gameState.battleState.turnPhase == TurnPhase.PlayerControlPhase && eve.SourceOwner == bft && eve.TargetItem?.OwnerBft == bft && eve.deltaValue > 0)
			{
				base.gameState.actionManager.EnqueueAction(new ChangeTempHealthAction(eve.deltaValue, bft));
			}
		}
	}

	private void OnDancerAfterSwapItem(GameEvent.Type _, GameEvent eve)
	{
		if (!IsDancer)
		{
			return;
		}
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && base.gameState.battleState.turnPhase == TurnPhase.PlayerControlPhase && eve.OriginContainer == TableContainer && eve.SwapSourceItem?.OwnerBft == bft && eve.SourceItem?.OwnerBft == bft && eve.TargetItem?.OwnerBft == bft)
		{
			int num = eve.amount2 - eve.amount;
			if (num > 0)
			{
				base.gameState.actionManager.EnqueueAction(new ChangeTempHealthAction(num, bft));
			}
		}
	}

	private void ResolveShadowDancerFirstEnchantConsumption(GameEvent.Type _, GameEvent eve)
	{
		if (!IsShadowDancer)
		{
			return;
		}
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && eve.triggerType == GameEvent.TriggerType.Real && eve.deltaValue < 0 && eve.resolveInfo != null && !eve.resolveInfo.reject)
		{
			Item eveObj = eve.GetEveObj<Item>(GameEvent.EveObjType.Destination);
			if (eveObj?.OwnerBft == bft && eve.TargetEnchant?.attachItem == eveObj && !shadowDancerEnchantProtectedItems.Contains(eveObj))
			{
				shadowDancerEnchantProtectedItems.Add(eveObj);
				eve.resolveInfo.reject = true;
				eve.resolveInfo.rejectSource = eveObj;
			}
		}
	}

	private void OnFrenziedDancerAfterFrenzy(GameEvent.Type _, GameEvent eve)
	{
		if (IsFrenziedDancer)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && !(eve.stringValue != "狂乱") && eve.SourceOwner == bft)
			{
				frenziedDancerShuffleCountThisTurn++;
			}
		}
	}

	private void ResolveFrenziedDancerFrenzyDamage(GameEvent.Type _, GameEvent eve)
	{
		if (!IsFrenziedDancer)
		{
			return;
		}
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo == null || gameStateInfo.battleState?.isInBattle != true || frenziedDancerShuffleCountThisTurn <= 0 || eve.resolveInfo == null || eve.damageResolveType != Damage.ResolveType.DamageValue || !IsPlayerOwnedDamageAgainstCurrentEnemy(eve.damagecommand, bft, base.gameState.battleState.enemy?.bft))
		{
			return;
		}
		Item item = eve.spellInfo?.spellItem ?? eve.SourceItem;
		if (item?.OwnerBft == bft)
		{
			ItemDefinition def = item.def;
			if ((object)def != null && def.tags?.Any((ItemTag tag) => (object)tag != null && tag.type == ItemTag.Type.Attack) == true)
			{
				eve.resolveInfo.sourceDelta += frenziedDancerShuffleCountThisTurn;
			}
		}
	}

	private void OnReturnerAfterSwapItem(GameEvent.Type _, GameEvent eve)
	{
		if (!IsReturner)
		{
			return;
		}
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && eve.OriginContainer == TableContainer && eve.SwapSourceItem?.OwnerBft == bft && ItemHasTagId(eve.SwapSourceItem, "f3715ebbdd3d5474a92986278633149b"))
		{
			EnqueueReturnerCooldownReduction(eve.SourceItem, eve.SwapSourceItem);
			if (eve.TargetItem != eve.SourceItem)
			{
				EnqueueReturnerCooldownReduction(eve.TargetItem, eve.SwapSourceItem);
			}
		}
	}

	private void EnqueueReturnerCooldownReduction(Item item, Item source)
	{
		if (item?.OwnerBft == bft && item.HasSpell && !ItemHasTagId(item, "f3715ebbdd3d5474a92986278633149b"))
		{
			base.gameState.actionManager.EnqueueAction(new ChangeCoolDownAction(item, source, -1));
		}
	}

	private void ResetFearlessWarriorDamageBonus(GameEvent.Type _, GameEvent eve)
	{
		fearlessWarriorDamageBonusThisTurn = 0;
	}

	private void ResetFrostWatcherExtraTurnEndEffects(GameEvent.Type _, GameEvent eve)
	{
		resolvingFrostWatcherExtraTurnEndEffects = false;
	}

	private void OnFearlessWarriorHealthChange(GameEvent.Type _, GameEvent eve)
	{
		if (IsFearlessWarrior)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && eve.TargetBft == bft && eve.deltaValue < 0 && !eve.boolValue && eve.damage != null)
			{
				fearlessWarriorDamageBonusThisTurn++;
			}
		}
	}

	private void OnFearlessWarriorArmorDamaged(GameEvent.Type _, GameEvent eve)
	{
		if (IsFearlessWarrior)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && HasInfluence("5aa6bb074baa8d6e890e34a2569efcc1") && eve.damage?.targetItem?.OwnerBft == bft && eve.damage.targetItem.Defence && (int)eve.damage.subCounter > 0)
			{
				fearlessWarriorDamageBonusThisTurn++;
			}
		}
	}

	private void ResolveFearlessWarriorDamageBonus(GameEvent.Type _, GameEvent eve)
	{
		if (IsFearlessWarrior)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && fearlessWarriorDamageBonusThisTurn > 0 && eve.resolveInfo != null && eve.damageResolveType == Damage.ResolveType.DamageValue && (eve.damagecommand?.source as IBftOwnable)?.OwnerBft == bft && eve.damagecommand.targetBFT == base.gameState.battleState.enemy?.bft)
			{
				float num = (HasInfluence("5aa6bb074baa8d6e890e34a2569efcc1") ? 10f : 20f);
				eve.resolveInfo.sourceDeltaPercent += (float)fearlessWarriorDamageBonusThisTurn * num;
			}
		}
	}

	private bool HasInfluence(string influenceGuid)
	{
		BattleFieldTransform battleFieldTransform = bft;
		if (battleFieldTransform == null)
		{
			return false;
		}
		return battleFieldTransform.influences?.Any((Influence influence) => influence?.def?.id == influenceGuid) == true;
	}

	private void OnRangerConvertYinYangPower(GameEvent.Type _, GameEvent eve)
	{
		if (IsRanger)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && eve.amount > 0)
			{
				rangerSlotBonusThisTurn++;
			}
		}
	}

	private void ResolveRangerFixedSlotCountBonus(GameEvent.Type _, GameEvent eve)
	{
		if (IsRanger)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && eve.resolveInfo != null && rangerSlotBonusThisTurn > 0)
			{
				eve.resolveInfo.deltaValue += rangerSlotBonusThisTurn;
			}
		}
	}

	private void OnWanfaChangeYinYangPower(GameEvent.Type _, GameEvent eve)
	{
		if (IsWanfa)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && IsPlayerTurnPhase())
			{
				int num = Mathf.Max(0, -eve.amount) + Mathf.Max(0, -eve.amount2);
				wanfaDamageBonusThisTurn += num;
			}
		}
	}

	private void ResolveWanfaDamageBonus(GameEvent.Type _, GameEvent eve)
	{
		if (IsWanfa)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && IsPlayerTurnPhase() && wanfaDamageBonusThisTurn > 0 && eve.resolveInfo != null && eve.damageResolveType == Damage.ResolveType.DamageValue && IsPlayerOwnedDamageAgainstCurrentEnemy(eve.damagecommand, bft, base.gameState.battleState.enemy?.bft))
			{
				eve.resolveInfo.sourceDelta += wanfaDamageBonusThisTurn;
			}
		}
	}

	private static bool IsPlayerOwnedDamageAgainstCurrentEnemy(DamageCommand damageCommand, BattleFieldTransform playerBft, BattleFieldTransform enemyBft)
	{
		if (playerBft != null && enemyBft != null && (damageCommand?.source as IBftOwnable)?.OwnerBft == playerBft)
		{
			return damageCommand.targetBFT == enemyBft;
		}
		return false;
	}

	private bool IsPlayerTurnPhase()
	{
		TurnPhase turnPhase = base.gameState.battleState.turnPhase;
		if (turnPhase != TurnPhase.PlayerTurnStartPhase && turnPhase != TurnPhase.PlayerControlPhase)
		{
			return turnPhase == TurnPhase.PlayerTurnEndPhase;
		}
		return true;
	}

	private void OnGarrisonHealthChange(GameEvent.Type _, GameEvent eve)
	{
		if (IsGarrison)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && eve.TargetBft == bft && eve.deltaValue < 0 && !eve.boolValue && eve.damage?.SourceOwnerBft == bft)
			{
				base.gameState.actionManager.EnqueueAction(new TrigramsManager.ChangYinYangPowerAction(1, 1, TrigramsManager.DurationType.Temp, eve.damage.source));
			}
		}
	}

	private void ResolveSpiritCallerFreeCompanionCost(GameEvent.Type type, GameEvent eve)
	{
		if (type == GameEvent.Type.SpellCastCostResolve && IsSpiritCaller && !spiritCallerFreeCompanionUsedThisTurn)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && eve.resolveInfo != null && IsPlayerCompanionSpell(eve.spellInfo))
			{
				int valueOrDefault = (eve.spellInfo.focusSpell?.requirement?.GetBaseCostValue(eve.spellInfo)).GetValueOrDefault();
				int num = Mathf.Max(0, valueOrDefault + eve.resolveInfo.deltaValue);
				eve.resolveInfo.deltaValue -= num;
			}
		}
	}

	private void OnSpiritCallerAfterSpellCast(GameEvent.Type _, GameEvent eve)
	{
		if (!IsSpiritCaller || spiritCallerFreeCompanionUsedThisTurn)
		{
			return;
		}
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo == null || gameStateInfo.battleState?.isInBattle != true)
		{
			return;
		}
		SpellCastInfo spellInfo = eve.spellInfo;
		if (spellInfo != null && spellInfo.casterType == SpellCastInfo.CasterType.Player && !eve.spellInfo.isTriggerCast)
		{
			Item spellItem = eve.spellInfo.spellItem;
			if (spellItem != null && spellItem.Companion)
			{
				spiritCallerFreeCompanionUsedThisTurn = true;
			}
		}
	}

	private bool IsPlayerCompanionSpell(SpellCastInfo spellInfo)
	{
		if (spellInfo != null && spellInfo.spellItem?.Companion == true)
		{
			return spellInfo.spellItem.OwnerBft == bft;
		}
		return false;
	}

	private void OnSmallKnightAfterSwapItem(GameEvent.Type _, GameEvent eve)
	{
		if (IsSmallKnight)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && eve.OriginContainer == TableContainer && eve.SourceItem != null && eve.TargetItem != null && eve.SourceItem != eve.TargetItem)
			{
				base.gameState.actionManager.EnqueueAction(ApplySmallKnightSwapAttackCostReduction);
			}
		}
	}

	private void ApplySmallKnightSwapAttackCostReduction()
	{
		ItemTag attackTag = ScriptableObjectSingleton<GameDataManager>.instance?.attackTag;
		EnchantDef asset = base.gameState.applicationState.assetCatalog.enchantDefCatalog.GetAsset("f9e88d7c3a394964cb007a19d87b93d4");
		if (!(attackTag == null) && !(asset == null))
		{
			List<Item> list = (from item in TableContainer.GetAllItems()
				where item?.CurrentSpell?.requirement != null && item.HasTag(attackTag) && GetCurrentSmallKnightAttackCost(item) >= 1
				select item).ToList();
			if (list.Count != 0)
			{
				Item targetItem = list.SelectRandom(base.gameState.randomManager.NextDouble);
				base.gameState.actionManager.EnqueueAction(new ChangeEnchantAction(targetItem, null, asset, 1));
			}
		}
	}

	private int GetCurrentSmallKnightAttackCost(Item item)
	{
		DisplaySpellCastInfo displaySpellCastInfo = new DisplaySpellCastInfo(base.gameState, inBattleDisplay: true)
		{
			spellItem = item,
			casterType = SpellCastInfo.CasterType.Player
		};
		return item.CurrentSpell.requirement.GetFinalCostValue(displaySpellCastInfo, displaySpellCastInfo.triggerType);
	}

	private void OnPlayerExileItem(GameEvent.Type _, GameEvent eve)
	{
		if (!IsFireController)
		{
			return;
		}
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true)
		{
			ItemContainer originContainer = eve.OriginContainer;
			if (originContainer != null && originContainer.IsBattleContainer() && eve.OriginContainer.OwnerBft == bft)
			{
				base.gameState.actionManager.EnqueueAction(new ChangeTempHealthAction(5, bft));
			}
		}
	}

	private void ResolveWingedMercenaryFixedSlotCountBonus(GameEvent.Type _, GameEvent eve)
	{
		if (IsWingedMercenary)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && eve.resolveInfo != null)
			{
				int num = TableContainer.GetAllItems().Count(IsWingedMercenaryZeroCostAttackSlotFreeItem);
				eve.resolveInfo.deltaValue += num;
			}
		}
	}

	private void ResolveCourtMusicianFixedSlotCountBonus(GameEvent.Type _, GameEvent eve)
	{
		if (IsCourtMusician)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true && eve.resolveInfo != null)
			{
				int num = TableContainer.GetAllItems().Count(IsCourtMusicianMusicSlotFreeItem);
				eve.resolveInfo.deltaValue += num;
			}
		}
	}

	public void AddPendingRiftBossEffect(RiftBossEffectType type, int value)
	{
		if (pendingRiftBossEffects == null)
		{
			pendingRiftBossEffects = new List<RiftBossEffectState>();
		}
		pendingRiftBossEffects.Add(new RiftBossEffectState
		{
			type = type,
			value = value
		});
	}

	private void ResolveRiftBossFailureRevive(GameEvent.Type _, GameEvent eve)
	{
		if (IsInBossBattle() && eve != null && eve.triggerType == GameEvent.TriggerType.Real && eve.TargetBft == bft && eve.resolveInfo != null && !eve.resolveInfo.reject && !pendingRiftBossEffects.IsEmptyOrNull())
		{
			RiftBossEffectState riftBossEffectState = pendingRiftBossEffects.FirstOrDefault((RiftBossEffectState e) => e != null && e.type == RiftBossEffectType.FailureRevive);
			if (riftBossEffectState != null)
			{
				pendingRiftBossEffects.Remove(riftBossEffectState);
				eve.resolveInfo.reject = true;
				int targetValue = Mathf.Max((int)((float)(int)bft.maxHealth * 0.5f), 1);
				base.gameState.actionManager.EnqueueAction(new SetBftHealthAction(targetValue, null, bft));
			}
		}
	}

	private void ApplyRiftBossEffectsOnBattleStart(GameEvent.Type _, GameEvent eve)
	{
		activeRiftBossMaxHealthDelta = 0;
		if (!IsInBossBattle() || pendingRiftBossEffects.IsEmptyOrNull())
		{
			return;
		}
		foreach (RiftBossEffectState pendingRiftBossEffect in pendingRiftBossEffects)
		{
			if (pendingRiftBossEffect != null && pendingRiftBossEffect.type == RiftBossEffectType.MaxHealthPercent)
			{
				int num = Mathf.CeilToInt((float)((int)bft.maxHealth * Mathf.Max(pendingRiftBossEffect.value, 1)) / 100f);
				if (num > 0)
				{
					activeRiftBossMaxHealthDelta += num;
					base.gameState.actionManager.EnqueueAction(new ChangeBftMaxHealthAction(bft, num, baseAdd: true));
				}
			}
		}
	}

	private void ClearRiftBossEffectsOnBattleEnd(GameEvent.Type _, GameEvent eve)
	{
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo != null && gameStateInfo.battleState?.enemy?.enemyEncounterType == EnemyEncounterType.Boss)
		{
			if (activeRiftBossMaxHealthDelta > 0)
			{
				base.gameState.actionManager.EnqueueAction(new ChangeBftMaxHealthAction(bft, -activeRiftBossMaxHealthDelta, baseAdd: true, hideNfx: true));
				activeRiftBossMaxHealthDelta = 0;
			}
			pendingRiftBossEffects?.Clear();
		}
	}

	private void ResolveRiftBossFixedSlotCountBonus(GameEvent.Type _, GameEvent eve)
	{
		if (IsInBossBattle() && eve.resolveInfo != null && !pendingRiftBossEffects.IsEmptyOrNull())
		{
			eve.resolveInfo.deltaValue += pendingRiftBossEffects.Where((RiftBossEffectState effect) => effect != null && effect.type == RiftBossEffectType.FixedSlotDelta).Sum((RiftBossEffectState effect) => Mathf.Max(effect.value, 1));
		}
	}

	private void ResolveRiftBossTurnDrawBonus(GameEvent.Type _, GameEvent eve)
	{
		if (IsInBossBattle() && eve.resolveInfo != null && eve.TargetBft == bft && !pendingRiftBossEffects.IsEmptyOrNull())
		{
			eve.resolveInfo.deltaValue += pendingRiftBossEffects.Where((RiftBossEffectState effect) => effect != null && effect.type == RiftBossEffectType.TurnDrawDelta).Sum((RiftBossEffectState effect) => Mathf.Max(effect.value, 1));
		}
	}

	private bool IsInBossBattle()
	{
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true)
		{
			CharacterAI enemy = base.gameState.battleState.enemy;
			if (enemy == null)
			{
				return false;
			}
			return enemy.enemyEncounterType == EnemyEncounterType.Boss;
		}
		return false;
	}

	private void TriggerWarChanterSwordEcho(GameEvent.Type type, GameEvent eve)
	{
		if (type != GameEvent.Type.ConsumeResolveInfo || !IsWarChanter)
		{
			return;
		}
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo == null || gameStateInfo.battleState?.isInBattle != true || eve.resolveInfo == null || eve.consumeType != ConsumeItemAction.ConsumeType.SpellCost || eve.triggerType != GameEvent.TriggerType.Real)
		{
			return;
		}
		Item sourceItem = eve.SourceItem;
		Item targetItem = eve.TargetItem;
		if (IsWarChanterSwordEchoSource(sourceItem) && IsWarChanterMusicCost(targetItem) && (!eve.resolveInfo.reject || eve.resolveInfo.rejectSource == null || eve.resolveInfo.rejectSource == sourceItem))
		{
			if (!eve.resolveInfo.reject)
			{
				eve.resolveInfo.reject = true;
				eve.resolveInfo.rejectSource = sourceItem;
			}
			AddWarChanterEchoChapter(targetItem, sourceItem);
		}
	}

	private bool IsWarChanterSwordEchoSource(Item item)
	{
		if (item != null && item.OwnerBft == bft && item.owner == TableContainer)
		{
			return ItemHasTagId(item, "04731c1b57be21b4084615a246151c9b");
		}
		return false;
	}

	private bool IsWarChanterMusicCost(Item item)
	{
		if (item != null && item.OwnerBft == bft && item.owner == TableContainer)
		{
			return ItemHasTagId(item, "d6b2a3d93e966504babd27ae6e1bd869");
		}
		return false;
	}

	private static bool ItemHasTagId(Item item, string tagId)
	{
		if (item == null || item.def?.tags?.Any((ItemTag tag) => tag?.id == tagId) != true)
		{
			if (item == null)
			{
				return false;
			}
			return item.tagInstances?.Any((ItemTagInstance tagInstance) => tagInstance?.tag?.id == tagId) == true;
		}
		return true;
	}

	private void AddWarChanterEchoChapter(Item costItem, Item sourceItem)
	{
		GameEvent.ResolveInfo resolveInfo = base.gameState.gameEventManager.TriggerAddChapterResolve(costItem, sourceItem, GameEvent.TriggerType.Real);
		if (!resolveInfo.reject)
		{
			base.gameState.actionManager.EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_slot_addChapter, costItem));
			base.gameState.actionManager.EnqueueAction(new ChangeCounterAction(resolveInfo.targetDelta + 1, costItem, sourceItem, withPopupText: true));
		}
	}

	private void TriggerFrostWatcherExtraTurnEndEffects(GameEvent.Type type, GameEvent eve)
	{
		if (type != GameEvent.Type.PlayerTurnEnd || !IsFrostWatcher || resolvingFrostWatcherExtraTurnEndEffects)
		{
			return;
		}
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo == null || gameStateInfo.battleState?.isInBattle != true)
		{
			return;
		}
		List<Item> list = TableContainer.GetAllItems().Where(IsFrostWatcherExtraTurnEndEffectItem).ToList();
		if (list.Count == 0)
		{
			return;
		}
		resolvingFrostWatcherExtraTurnEndEffects = true;
		foreach (Item item in list)
		{
			base.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				TriggerFrostWatcherExtraTurnEndEffect(item);
			}));
		}
		base.gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			resolvingFrostWatcherExtraTurnEndEffects = false;
		}));
	}

	private bool IsFrostWatcherExtraTurnEndEffectItem(Item item)
	{
		if (!IsFrostWatcherTableItem(item))
		{
			return false;
		}
		if (!CanTriggerFrostWatcherEvent(item, item, GameEvent.Type.PlayerTurnEnd))
		{
			return item.Enchants.OrEmptyIfNull().Any((Enchant enchant) => IsFrostWatcherAttachedEnchant(enchant, item) && (CanTriggerFrostWatcherEvent(enchant, enchant, GameEvent.Type.PlayerTurnEnd) || CanTriggerFrostWatcherDurationLoss(enchant)));
		}
		return true;
	}

	private bool IsFrostWatcherTableItem(Item item)
	{
		if (item != null && item.owner == TableContainer && item.OwnerBft == bft)
		{
			return !item.IsAddAsSpellCost(base.gameState);
		}
		return false;
	}

	private bool IsFrostWatcherAttachedEnchant(Enchant enchant, Item item)
	{
		if (enchant != null && enchant.attachItem == item && IsFrostWatcherTableItem(item))
		{
			return item.Enchants.OrEmptyIfNull().Contains(enchant);
		}
		return false;
	}

	private void TriggerFrostWatcherExtraTurnEndEffect(Item item)
	{
		if (!IsFrostWatcherTableItem(item))
		{
			return;
		}
		List<Enchant> enchants = item.Enchants.OrEmptyIfNull().ToList();
		TriggerFrostWatcherExtraItemTurnEndEffect(item);
		if (IsFrostWatcherTableItem(item))
		{
			TriggerFrostWatcherExtraEnchantTurnEndEffects(item, enchants);
			if (IsFrostWatcherTableItem(item))
			{
				TriggerFrostWatcherEnchantDurationLoss(item, enchants);
			}
		}
	}

	private void TriggerFrostWatcherExtraItemTurnEndEffect(Item item)
	{
		TriggerFrostWatcherEvent(item, item, GameEvent.Type.PlayerTurnEnd, "Extra Item TurnEnd");
	}

	private void TriggerFrostWatcherExtraEnchantTurnEndEffects(Item item, List<Enchant> enchants)
	{
		foreach (Enchant item2 in enchants.OrEmptyIfNull())
		{
			if (IsFrostWatcherAttachedEnchant(item2, item))
			{
				TriggerFrostWatcherEvent(item2, item2, GameEvent.Type.PlayerTurnEnd, "Extra Enchant TurnEnd");
			}
		}
	}

	private bool CanTriggerFrostWatcherEvent(IGameEventSubscriber subscriber, object caster, GameEvent.Type eventType)
	{
		GameEvent gameEvent = new GameEvent(eventType, GameEvent.TriggerType.Real)
		{
			gameState = base.gameState
		};
		try
		{
			gameEvent.SetCaster(caster);
			return subscriber?.CheckTriggerGameEventAble(gameEvent) ?? false;
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, $"FrostWatcher Extra Event Check Exception：{subscriber}-{gameEvent.type}");
			return false;
		}
		finally
		{
			gameEvent.SetCaster(null);
		}
	}

	private void TriggerFrostWatcherEvent(IGameEventSubscriber subscriber, object caster, GameEvent.Type eventType, string label)
	{
		if (!CanTriggerFrostWatcherEvent(subscriber, caster, eventType))
		{
			return;
		}
		GameEvent gameEvent = new GameEvent(eventType, GameEvent.TriggerType.Real)
		{
			gameState = base.gameState
		};
		try
		{
			gameEvent.SetCaster(caster);
			subscriber.TriggerGameEvent(gameEvent);
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, $"FrostWatcher {label} Exception：{subscriber}-{gameEvent.type}");
		}
		finally
		{
			gameEvent.SetCaster(null);
		}
	}

	private bool CanTriggerFrostWatcherDurationLoss(Enchant enchant)
	{
		if (!IsFrostWatcherAttachedEnchant(enchant, enchant?.attachItem))
		{
			return false;
		}
		return (enchant.def?.attachAbility).OrEmptyIfNull().Any((Ability ability) => TryGetFrostWatcherDurationLossEffect(ability, enchant, out var _));
	}

	private bool TryGetFrostWatcherDurationLossEffect(Ability ability, Enchant enchant, out ChangeEnchant durationLossEffect)
	{
		durationLossEffect = null;
		if (!(ability is TurnTriggerAbility { requireAfterTurnEnd: not false, turnType: TurnTriggerAbility.TurnType.turnStart, triggerCheckNode: null } turnTriggerAbility) || !enchant.afterTurnEnd)
		{
			return false;
		}
		if (turnTriggerAbility.ownerType != TurnTriggerAbility.OwnerTriggerType.Self && turnTriggerAbility.ownerType != TurnTriggerAbility.OwnerTriggerType.Player)
		{
			return false;
		}
		if (!turnTriggerAbility.occasion.HasFlag(Ability.TriggerOccasion.InTableItem) && !turnTriggerAbility.occasion.HasFlag(Ability.TriggerOccasion.Always))
		{
			return false;
		}
		if (!(turnTriggerAbility.effect is ChangeEnchant { enchantGenerator: PredefinedEnchantGenerator enchantGenerator } changeEnchant) || enchantGenerator.enchantDef != enchant.def || !(changeEnchant.deltaValueGenerator is PredefinedValueGenerator { value: <0 }) || !(changeEnchant.targetSelector is SingleTargetSelector { target: GameEvent.EventItemType.CasterAttachingItem }))
		{
			return false;
		}
		durationLossEffect = changeEnchant;
		return true;
	}

	private void TriggerFrostWatcherEnchantDurationLoss(Item item, List<Enchant> enchants)
	{
		foreach (Enchant item2 in enchants.OrEmptyIfNull())
		{
			if (!IsFrostWatcherAttachedEnchant(item2, item))
			{
				continue;
			}
			foreach (Ability item3 in (item2.def?.attachAbility).OrEmptyIfNull().ToList())
			{
				if (!IsFrostWatcherAttachedEnchant(item2, item))
				{
					break;
				}
				if (TryGetFrostWatcherDurationLossEffect(item3, item2, out var durationLossEffect))
				{
					TriggerFrostWatcherDurationLossEffect(durationLossEffect, item2);
				}
			}
		}
	}

	private void TriggerFrostWatcherDurationLossEffect(ChangeEnchant durationLossEffect, Enchant enchant)
	{
		GameEvent gameEvent = new GameEvent(GameEvent.Type.PlayerTurnEnd, GameEvent.TriggerType.Real)
		{
			gameState = base.gameState
		};
		try
		{
			gameEvent.SetCaster(enchant);
			durationLossEffect.Cast(gameEvent);
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, $"FrostWatcher Duration Loss Exception：{enchant}-{gameEvent.type}");
		}
		finally
		{
			gameEvent.SetCaster(null);
		}
	}

	private void ResetHarmonyMusicianFinishedMusicChapters(GameEvent.Type _, GameEvent eve)
	{
		if (!IsHarmonyMusician)
		{
			return;
		}
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true)
		{
			if (IsHarmonyMusicianFugueActivated)
			{
				ResetHarmonyMusicianFugueFinishedMusicChapters();
			}
			else
			{
				EnqueueHarmonyMusicianChapterResets(TableContainer.GetAllItems().Where(IsFinishedPlayerTableMusicChapter).ToList());
			}
		}
	}

	private void ResetHarmonyMusicianFugueFinishedMusicChaptersBeforeEnemyTurn(GameEvent.Type _, GameEvent eve)
	{
		if (IsHarmonyMusician && IsHarmonyMusicianFugueActivated)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true)
			{
				ResetHarmonyMusicianFugueFinishedMusicChapters();
			}
		}
	}

	private void QueueHarmonyMusicianFugueResetAfterFinalChapter(GameEvent.Type type, GameEvent eve)
	{
		if (type != GameEvent.Type.CounterReachMaxCounter || pendingHarmonyMusicianFugueResetChapters != null || !IsHarmonyMusician || !IsHarmonyMusicianFugueActivated)
		{
			return;
		}
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo == null || gameStateInfo.battleState?.isInBattle != true || !IsFinishedPlayerTableMusicChapter(eve?.TargetItem))
		{
			return;
		}
		List<Item> musicChapters = GetCompletedHarmonyMusicianFugueCycleChapters();
		if (musicChapters != null)
		{
			pendingHarmonyMusicianFugueResetChapters = musicChapters;
			base.gameState.actionManager.EnqueueAction(delegate
			{
				ResolvePendingHarmonyMusicianFugueReset(musicChapters);
			});
		}
	}

	private void ResolvePendingHarmonyMusicianFugueReset(List<Item> musicChapters)
	{
		if (pendingHarmonyMusicianFugueResetChapters != musicChapters)
		{
			return;
		}
		pendingHarmonyMusicianFugueResetChapters = null;
		if (IsHarmonyMusician && IsHarmonyMusicianFugueActivated)
		{
			GameStateInfo gameStateInfo = base.gameState;
			if (gameStateInfo != null && gameStateInfo.battleState?.isInBattle == true)
			{
				EnqueueHarmonyMusicianChapterResets(musicChapters.Where(IsFinishedPlayerTableMusicChapter).ToList());
			}
		}
	}

	private void ClearHarmonyMusicianFugueResetPending(GameEvent.Type _, GameEvent eve)
	{
		pendingHarmonyMusicianFugueResetChapters = null;
	}

	private void ResolveHarmonyMusicianMusicChapterOnTurnEnd(GameEvent.Type _, GameEvent eve)
	{
		if (!IsHarmonyMusician)
		{
			return;
		}
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo == null || gameStateInfo.battleState?.isInBattle != true || base.gameState.battleState.turnPhase != TurnPhase.PlayerTurnEndPhase || eve.resolveInfo == null)
		{
			return;
		}
		Item targetItem = eve.TargetItem;
		if (IsPlayerTableMusicChapter(targetItem) && IsMusicTagChapterSource(targetItem, eve.GetEveObj(GameEvent.EveObjType.Source)))
		{
			int? harmonyMusicianMusicChapterTurnEndDelta = GetHarmonyMusicianMusicChapterTurnEndDelta(targetItem);
			if (harmonyMusicianMusicChapterTurnEndDelta.HasValue)
			{
				eve.resolveInfo.targetDelta += harmonyMusicianMusicChapterTurnEndDelta.Value;
			}
		}
	}

	private void ResolveElectricScholarSameElementDamageBonus(GameEvent.Type _, GameEvent eve)
	{
		if (!IsElectricScholar)
		{
			return;
		}
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo == null || gameStateInfo.battleState?.isInBattle != true || eve.resolveInfo == null || eve.damageResolveType != Damage.ResolveType.DamageValue)
		{
			return;
		}
		Item damageSourceItem = GetDamageSourceItem(eve.damagecommand?.source);
		if (damageSourceItem?.OwnerBft == bft && damageSourceItem.owner == TableContainer)
		{
			int bestSameElementSequenceCount = GetBestSameElementSequenceCount(damageSourceItem);
			if (bestSameElementSequenceCount >= 2)
			{
				eve.resolveInfo.sourceDelta += bestSameElementSequenceCount - 1;
			}
		}
	}

	private int GetBestSameElementSequenceCount(Item sourceItem)
	{
		List<string> itemElementTagIds = GetItemElementTagIds(sourceItem);
		if (itemElementTagIds.Count == 0)
		{
			return 0;
		}
		List<Item> list = (from item in TableContainer.GetAllItems()
			where item != null && !item.IsAddAsSpellCost(base.gameState)
			select item).ToList();
		int num = list.IndexOf(sourceItem);
		if (num < 0)
		{
			return 0;
		}
		int num2 = 0;
		foreach (string item in itemElementTagIds)
		{
			int num3 = 1;
			int num4 = num - 1;
			while (num4 >= 0 && HasElementTag(list[num4], item))
			{
				num3++;
				num4--;
			}
			for (int num5 = num + 1; num5 < list.Count && HasElementTag(list[num5], item); num5++)
			{
				num3++;
			}
			num2 = Mathf.Max(num2, num3);
		}
		return num2;
	}

	private static List<string> GetItemElementTagIds(Item item)
	{
		return item?.def?.tags?.Where((ItemTag tag) => tag != null && ElementItemTagGuids.Contains(tag.id)).Select((ItemTag tag) => tag.id).Distinct()
			.ToList() ?? new List<string>();
	}

	private static bool HasElementTag(Item item, string tagId)
	{
		if (item == null)
		{
			return false;
		}
		return item.def?.tags?.Any((ItemTag tag) => tag?.id == tagId) == true;
	}

	public static bool IsElementItemTag(ItemTag tag)
	{
		if (tag != null)
		{
			return ElementItemTagGuids.Contains(tag.id);
		}
		return false;
	}

	public static bool ShouldTreatAsAllElementTags(Item item)
	{
		if (item != null && item.OwnerBft?.isPlayer == true && item.owner?.gameState?.playerState?.character?.def?.id == "5c00e2c5c7da49440929aa48f0c166b2")
		{
			ItemDefinition def = item.def;
			if ((object)def == null)
			{
				return false;
			}
			return def.tags?.Any(IsElementItemTag) == true;
		}
		return false;
	}

	private static Item GetDamageSourceItem(object source)
	{
		if (source is Item result)
		{
			return result;
		}
		if (source is IAttachment { AttachTarget: Item attachTarget })
		{
			return attachTarget;
		}
		return null;
	}

	private bool IsWingedMercenaryZeroCostAttackSlotFreeItem(Item item)
	{
		if (item != null && !item.IsAddAsSpellCost(base.gameState) && item.HasSpell)
		{
			ItemDefinition def = item.def;
			if ((object)def != null && def.tags?.Any((ItemTag tag) => (object)tag != null && tag.type == ItemTag.Type.Attack) == true)
			{
				return GetItemFinalCost(item) == 0;
			}
		}
		return false;
	}

	private bool IsCourtMusicianMusicSlotFreeItem(Item item)
	{
		if (item != null && !item.IsAddAsSpellCost(base.gameState) && (int)item.counter < (int)item.maxCounter)
		{
			ItemDefinition def = item.def;
			if ((object)def == null)
			{
				return false;
			}
			return def.tags?.Any((ItemTag tag) => (object)tag != null && tag.type == ItemTag.Type.Music) == true;
		}
		return false;
	}

	private bool IsFinishedPlayerTableMusicChapter(Item item)
	{
		if (IsPlayerTableFinalizableMusicChapter(item))
		{
			return (int)item.counter == (int)item.maxCounter;
		}
		return false;
	}

	private int? GetHarmonyMusicianMusicChapterTurnEndDelta(Item target)
	{
		if (!IsHarmonyMusicianFantasiaActivated)
		{
			return TableContainer.GetItemIndex(target);
		}
		int? harmonyMusicianContiguousMusicChapterCount = GetHarmonyMusicianContiguousMusicChapterCount(target);
		if (!harmonyMusicianContiguousMusicChapterCount.HasValue)
		{
			return null;
		}
		return harmonyMusicianContiguousMusicChapterCount.Value - 1;
	}

	private int? GetHarmonyMusicianContiguousMusicChapterCount(Item target)
	{
		List<Item> allItems = TableContainer.GetAllItems();
		int num = allItems.IndexOf(target);
		if (num < 0)
		{
			return null;
		}
		int num2 = 1;
		int num3 = num - 1;
		while (num3 >= 0 && IsPlayerTableMusicChapter(allItems[num3]))
		{
			num2++;
			num3--;
		}
		for (int i = num + 1; i < allItems.Count && IsPlayerTableMusicChapter(allItems[i]); i++)
		{
			num2++;
		}
		return num2;
	}

	private bool IsPlayerTableMusicChapter(Item item)
	{
		if (item != null && item.OwnerBft == bft && item.owner == TableContainer && !item.IsAddAsSpellCost(base.gameState))
		{
			ItemDefinition def = item.def;
			if ((object)def == null)
			{
				return false;
			}
			return def.tags?.Any((ItemTag tag) => (object)tag != null && tag.type == ItemTag.Type.Music) == true;
		}
		return false;
	}

	private bool IsPlayerTableFinalizableMusicChapter(Item item)
	{
		if (IsPlayerTableMusicChapter(item))
		{
			return (int)item.maxCounter > 0;
		}
		return false;
	}

	private static bool IsMusicTagChapterSource(Item targetItem, object source)
	{
		if (source is ItemTagInstance itemTagInstance && itemTagInstance.AttachTarget == targetItem)
		{
			ItemTag tag = itemTagInstance.tag;
			if ((object)tag == null)
			{
				return false;
			}
			return tag.type == ItemTag.Type.Music;
		}
		return false;
	}

	private List<Item> GetCompletedHarmonyMusicianFugueCycleChapters()
	{
		List<Item> list = TableContainer.GetAllItems().Where(IsPlayerTableFinalizableMusicChapter).ToList();
		if (list.Count <= 0 || !list.All(IsFinishedPlayerTableMusicChapter))
		{
			return null;
		}
		return list;
	}

	private void ResetHarmonyMusicianFugueFinishedMusicChapters()
	{
		pendingHarmonyMusicianFugueResetChapters = null;
		List<Item> completedHarmonyMusicianFugueCycleChapters = GetCompletedHarmonyMusicianFugueCycleChapters();
		if (completedHarmonyMusicianFugueCycleChapters != null)
		{
			EnqueueHarmonyMusicianChapterResets(completedHarmonyMusicianFugueCycleChapters);
		}
	}

	private void EnqueueHarmonyMusicianChapterResets(List<Item> items)
	{
		foreach (Item item in items)
		{
			base.gameState.actionManager.EnqueueAction(new SetCounterAction(0, item, null));
		}
		foreach (Item item2 in items)
		{
			base.gameState.actionManager.EnqueueAction(new ResolveChapterResetEffectAction(item2));
		}
	}

	private int GetItemFinalCost(Item item)
	{
		ItemRequirement itemRequirement = item?.CurrentSpell?.requirement;
		if (itemRequirement == null)
		{
			return 0;
		}
		DisplaySpellCastInfo castInfo = new DisplaySpellCastInfo(base.gameState, inBattleDisplay: true)
		{
			spellItem = item
		};
		return itemRequirement.GetFinalCostValue(castInfo, GameEvent.TriggerType.InBattleDisplay);
	}

	public bool HasEnoughCoin(int price)
	{
		int variableValue = base.gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.CoinLowerBound);
		return (int)base.gameState.playerState.coinCount - price >= variableValue;
	}

	public static int CalculateMissingWillPowerLevel(int currentUsedWillPower, int maxWillPower)
	{
		return Mathf.Clamp(currentUsedWillPower - maxWillPower, 0, MaxMissingWillPowerLevel);
	}

	public static IEnumerable<int> GetMissingWillPowerLevelsToGrant(int currentUsedWillPower, int maxWillPower, int highestGrantedLevel)
	{
		int targetLevel = CalculateMissingWillPowerLevel(currentUsedWillPower, maxWillPower);
		for (int level = Mathf.Max(highestGrantedLevel + 1, 1); level <= targetLevel; level++)
		{
			yield return level;
		}
	}

	public int PreviewWillPowerOverLimit(int addWillPowerCost)
	{
		return Mathf.Max(CurrentUsedWillPower + addWillPowerCost - MaxWillPower, 0);
	}

	public List<Influence> PreviewMissingWillPowerPenaltyInfluences(int addWillPowerCost)
	{
		return (from def in GetMissingWillPowerLevelsToGrant(CurrentUsedWillPower + addWillPowerCost, MaxWillPower, highestGrantedMissingWillPowerLevel).Select(GetMissingWillPowerInfluenceDef)
			where def != null
			select def).Select(CreateMissingWillPowerInfluence).ToList();
	}

	public bool ShouldShowWillPowerOverLimitConfirm(int addWillPowerCost, out int overLimitWillPower, out List<Influence> penaltyInfluences)
	{
		overLimitWillPower = PreviewWillPowerOverLimit(addWillPowerCost);
		penaltyInfluences = PreviewMissingWillPowerPenaltyInfluences(addWillPowerCost);
		if (overLimitWillPower > 0)
		{
			return !penaltyInfluences.IsNullOrEmpty();
		}
		return false;
	}

	public static InfluenceDef FindMissingWillPowerInfluenceDef(AssetCatalog assetCatalog, int level)
	{
		string text = MissingWillPowerInfluenceIds.ElementAtOrDefault(level - 1);
		if (text.IsNullOrEmpty())
		{
			return null;
		}
		InfluenceDef influenceDef = assetCatalog?.influenceCatalog?.GetAsset(text);
		if ((object)influenceDef == null || influenceDef.type != InfluenceDef.InfluenceType.Negative)
		{
			return null;
		}
		return influenceDef;
	}

	public static Influence CreateMissingWillPowerInfluence(InfluenceDef influenceDef)
	{
		return new Influence(influenceDef)
		{
			willPowerCost = 0
		};
	}

	public static bool ShouldSyncMissingWillPowerPenalty(Influence influence)
	{
		if (influence != null && influence.willPowerCost > 0)
		{
			InfluenceDef def = influence.def;
			if ((object)def == null)
			{
				return true;
			}
			return def.type != InfluenceDef.InfluenceType.Negative;
		}
		return false;
	}

	public void SyncMissingWillPowerPenaltyInfluences()
	{
		foreach (int item in GetMissingWillPowerLevelsToGrant(CurrentUsedWillPower, MaxWillPower, highestGrantedMissingWillPowerLevel))
		{
			InfluenceDef missingWillPowerInfluenceDef = GetMissingWillPowerInfluenceDef(item);
			if (missingWillPowerInfluenceDef == null)
			{
				break;
			}
			base.gameState.actionManager.EnqueueAction(new AddInfluenceAction(CreateMissingWillPowerInfluence(missingWillPowerInfluenceDef), bft));
			highestGrantedMissingWillPowerLevel = item;
		}
	}

	private InfluenceDef GetMissingWillPowerInfluenceDef(int level)
	{
		string arg = MissingWillPowerInfluenceIds.ElementAtOrDefault(level - 1);
		InfluenceDef influenceDef = FindMissingWillPowerInfluenceDef(base.gameState.applicationState.assetCatalog, level);
		if (influenceDef == null)
		{
			TraceLog.Warn($"[PlayerGameStateInfo] Missing will power penalty influence not found: level={level}, id={arg}.");
		}
		return influenceDef;
	}

	public void CheckDeath()
	{
		if ((int)bft.health > 0 || !base.gameState.loaded || base.gameState.gameEnded || base.gameState.UIManager.deathPendingPanel.pending)
		{
			return;
		}
		if (!base.gameState.gameEventManager.TriggerDeathCheckResolve(null, bft, GameEvent.TriggerType.Real).reject && !base.applicationState.guideManager.ShouldRejectDeathForGuide)
		{
			if (base.gameState.battleState != null && base.gameState.battleState.turnPhase == TurnPhase.EnemyMovePhase)
			{
				base.gameState.actionManager.EnqueueAction(new EndTurnAction(EndTurnAction.Type.Enemy));
			}
			base.gameState.UIManager.deathPendingPanel.TriggerDeathPending(OnDeath);
		}
		else
		{
			bft.health = 1;
		}
		void OnDeath()
		{
			if (base.gameState.battleState != null && base.gameState.battleState.isInBattle)
			{
				base.gameState.actionManager.EnqueueAction(new FinishBattleEncounterAction(FinishBattleEncounterAction.FinishType.Lose));
			}
			else
			{
				base.gameState.actionManager.EnqueueAction(new EndGameAction(EndGameAction.EndGameType.Death));
			}
		}
	}

	public void SetupSkillPreset(ProfileSkillPreset skillPreset)
	{
		this.skillPreset = skillPreset;
		prepareSkillsPreset = new GameSkillPreset(skillPreset);
	}
}
