using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;

public class GameEventManager : InGameManager
{
	private static readonly HashSet<string> UltimatePurificationItemNames = new HashSet<string> { "终极净化", "終極淨化" };

	public static HashSet<GameEvent.Type> AllToUpdateNotifyGameEvent = new HashSet<GameEvent.Type>
	{
		GameEvent.Type.ExileItem,
		GameEvent.Type.AddItem,
		GameEvent.Type.AfterRemoveItem,
		GameEvent.Type.AfterAddEnchant,
		GameEvent.Type.BeforeRemoveEnchant,
		GameEvent.Type.AddInfluence,
		GameEvent.Type.RemoveInfluence,
		GameEvent.Type.AddRune,
		GameEvent.Type.RemoveRune,
		GameEvent.Type.AddSkill,
		GameEvent.Type.RemoveSkill,
		GameEvent.Type.BattleStart,
		GameEvent.Type.BeforeBattleEnd,
		GameEvent.Type.AfterBattleEnd,
		GameEvent.Type.BeforeGameStart,
		GameEvent.Type.BeforeGameEnd,
		GameEvent.Type.BeforeGameGeneration,
		GameEvent.Type.AfterLoadGame,
		GameEvent.Type.AfterChangeStance,
		GameEvent.Type.SetEnvironmentInfluence
	};

	private EventManager eventManager;

	private new GameStateInfo gameState;

	private Dictionary<GameEvent.Type, HashSet<IGameEventSubscriber>> itemSubscribeGameEventDic_stable;

	private bool mockObjectForTextDisplay;

	private readonly Dictionary<GameEvent.Type, HashSet<IGameEventSubscriber>> subscribeGameEventObjectDic = new Dictionary<GameEvent.Type, HashSet<IGameEventSubscriber>>();

	public event Action<GameEvent> onTriggerGameEvent;

	public override void Init(GameStateInfo gameState)
	{
		this.gameState = gameState;
		eventManager = this.gameState.applicationState.eventManager;
		eventManager.gameEvent.AddGlobalListener(TriggerGameEventForNotifiables);
	}

	public static GameEventManager GenerateMockGameEventMangerForTextDisplay(GameStateInfo gs)
	{
		return new GameEventManager
		{
			gameState = gs,
			mockObjectForTextDisplay = true
		};
	}

	public override void Dispose()
	{
		eventManager.gameEvent.RemoveGlobalListener(TriggerGameEventForNotifiables);
	}

	public void TriggerAfterDamage(Damage damage)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterDamage);
		gameEvent.damage = damage;
		gameEvent.amount = damage.subCounter;
		gameEvent.TargetItem = damage.targetItem;
		if (gameEvent.TargetItem == null)
		{
			gameEvent.TargetBft = damage.targetBFT;
		}
		gameEvent.dealTypeSourceOwnerOverride = damage.sourceOwnerOverride;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, damage.source);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterHeal(object source, BattleFieldTransform bft, int healAmount)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterHeal);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.TargetBft = bft;
		gameEvent.amount = healAmount;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAddItem(AddItemEventInfo addItemInfo)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AddItem);
		gameEvent.addItemInfo = addItemInfo;
		gameEvent.TargetItem = addItemInfo.addItem;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, addItemInfo.source);
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, addItemInfo.targetContainer);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerExileItem(Item item, object source, ItemContainer originContainer = null)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ExileItem);
		gameEvent.TargetItem = item;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.SetEveObj(GameEvent.EveObjType.Origin, originContainer);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterRemoveItem(Item item, object source, ItemContainer originContainer)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterRemoveItem);
		gameEvent.TargetItem = item;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.SetEveObj(GameEvent.EveObjType.Origin, originContainer);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeSpellCastHandleCost(SpellCastInfo castInfo)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeSpellCastHandleCost);
		gameEvent.SourceItem = castInfo.spellItem;
		gameEvent.TargetItem = castInfo.targetItem;
		gameEvent.spellInfo = castInfo;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterSpellCast(SpellCastInfo castInfo)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterSpellCast);
		gameEvent.SourceItem = castInfo.spellItem;
		gameEvent.TargetItem = castInfo.targetItem;
		gameEvent.spellInfo = castInfo;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterTransform(Item source, Item target)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterTransform);
		gameEvent.SourceItem = source;
		gameEvent.TargetItem = target;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeBattleEnd(FinishBattleEncounterAction.FinishType type)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeBattleEnd);
		gameEvent.battleFinishType = type;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterBattleEnd(FinishBattleEncounterAction.FinishType type)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterBattleEnd);
		gameEvent.battleFinishType = type;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBattleStart()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.BattleStart);
		TriggerGameEvent(eve);
	}

	public void TriggerPlayerDeath()
	{
		TriggerGameEvent(GenerateGameEvent(GameEvent.Type.PlayerDeath));
	}

	public void TriggerBeforeDamage(Damage damage)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeDamage);
		gameEvent.damage = damage;
		gameEvent.amount = damage.baseDamage;
		gameEvent.TargetItem = damage.targetItem;
		if (gameEvent.TargetItem == null)
		{
			gameEvent.TargetBft = damage.targetBFT;
		}
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, damage.source);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeItemAdd(AddItemEventInfo addItemInfo)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeAddItem);
		gameEvent.addItemInfo = addItemInfo;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, addItemInfo.source);
		gameEvent.TargetItem = addItemInfo.addItem;
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, addItemInfo.targetContainer);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAddEnchant(object source, Item target, Enchant enchant)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterAddEnchant);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.TargetEnchant = enchant;
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, target);
		gameEvent.SetCaster(enchant);
		enchant.def.addEffect?.Cast(gameEvent);
		gameEvent.SetCaster(null);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerTriggerBonus(BattleBonus bonus)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.TriggerBonus);
		gameEvent.battleBonus = bonus;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerFinishBonus(BattleBonus bonus)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.FinishBonus);
		gameEvent.battleBonus = bonus;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerEnchantValueChange(object source, Item targetItem, Enchant enchant, int delta)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.EnchantValueChange);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, targetItem);
		gameEvent.TargetEnchant = enchant;
		gameEvent.deltaValue = delta;
		gameEvent.SetCaster(enchant);
		enchant.def.changeEffect?.Cast(gameEvent);
		gameEvent.SetCaster(null);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeRemoveEnchant(object source, Item target, Enchant enchant)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeRemoveEnchant);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, target);
		gameEvent.TargetEnchant = enchant;
		gameEvent.SetCaster(enchant);
		enchant.def.removeEffect?.Cast(gameEvent);
		gameEvent.SetCaster(null);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeExileItem(Item target, object source, ItemContainer originContainer = null)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeExileItem);
		gameEvent.TargetItem = target;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.SetEveObj(GameEvent.EveObjType.Origin, originContainer);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeRemoveItem(Item target, object source, ItemContainer originContainer)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeRemoveItem);
		gameEvent.TargetItem = target;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.SetEveObj(GameEvent.EveObjType.Origin, originContainer);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeSpellCast(SpellCastInfo castInfo)
	{
		ClearBlockyDelicacyFlyVfxOnUltimatePurification(castInfo);
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeSpellCast);
		gameEvent.SourceItem = castInfo.spellItem;
		gameEvent.TargetItem = castInfo.targetItem;
		gameEvent.spellInfo = castInfo;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerChangeCoolDown(Item target, Item source, int delta, int finalValue)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ChangeCoolDown);
		gameEvent.TargetItem = target;
		gameEvent.SourceItem = source;
		gameEvent.deltaValue = delta;
		gameEvent.amount = finalValue;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerCoinChange(int value, int delta)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ChangeCoin);
		gameEvent.amount = value;
		gameEvent.deltaValue = delta;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerWillPowerChange(int value, int delta)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ChangeWillPower);
		gameEvent.amount = value;
		gameEvent.deltaValue = delta;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerGemChange(int value, int delta)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ChangeGem);
		gameEvent.amount = value;
		gameEvent.deltaValue = delta;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeConsume(Item item, SpellCastInfo castInfo, ConsumeItemAction.ConsumeType type)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeConsume);
		gameEvent.TargetItem = item;
		gameEvent.consumeType = type;
		if (castInfo != null)
		{
			gameEvent.SourceItem = castInfo.spellItem;
			gameEvent.spellInfo = castInfo;
		}
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterConsume(Item item, SpellCastInfo castInfo, ConsumeItemAction.ConsumeType type, ItemContainer container)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterConsume);
		gameEvent.TargetItem = item;
		gameEvent.SetEveObj(GameEvent.EveObjType.Origin, container);
		gameEvent.consumeType = type;
		if (castInfo != null)
		{
			gameEvent.SourceItem = castInfo.spellItem;
			gameEvent.spellInfo = castInfo;
		}
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeCost(Item item, SpellCastInfo castInfo)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeCost);
		gameEvent.TargetItem = item;
		gameEvent.consumeType = ConsumeItemAction.ConsumeType.SpellCost;
		if (castInfo != null)
		{
			gameEvent.SourceItem = castInfo.spellItem;
			gameEvent.spellInfo = castInfo;
		}
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterCost(Item item, SpellCastInfo castInfo)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterCost);
		gameEvent.TargetItem = item;
		gameEvent.consumeType = ConsumeItemAction.ConsumeType.SpellCost;
		if (castInfo != null)
		{
			gameEvent.SourceItem = castInfo.spellItem;
			gameEvent.spellInfo = castInfo;
		}
		TriggerGameEvent(gameEvent);
	}

	public void TriggerCounterChange(Item target, object source, int delta, int finalCounter)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ChangeCounter);
		gameEvent.TargetItem = target;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.deltaValue = delta;
		gameEvent.amount = finalCounter;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerMaxCounterChange(Item target, object source, int delta, int finalMaxCounter)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ChangeMaxCounter);
		gameEvent.TargetItem = target;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.deltaValue = delta;
		gameEvent.amount = finalMaxCounter;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerCounterReachMaxCounter(Item target, object source)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.CounterReachMaxCounter);
		gameEvent.TargetItem = target;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerCrystalChange(int value, int delta)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ChangeCrystal);
		gameEvent.amount = value;
		gameEvent.deltaValue = delta;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerFinishWorldEvent(WorldEvent worldEvent, WorldEventChoice choice)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeFinishWorldEvent);
		if (choice != null)
		{
			gameEvent.TargetWorldEventChoice = choice;
		}
		gameEvent.TargetWorldEvent = worldEvent;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerEnemyTurnEnd()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.EnemyTurnEnd);
		gameState.battleState.SetEnemyAfterTurnEndFlag();
		TriggerGameEvent(eve);
	}

	public void TriggerEnemyTurnStart()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.EnemyTurnStart);
		TriggerGameEvent(eve);
	}

	public void TriggerBeforeEnterLocation(PathLocation location)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeEnterLocation);
		gameEvent.TargetLocation = location;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerEnterLocation(PathLocation location)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.EnterLocation);
		gameEvent.TargetLocation = location;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterEnterLocationScreenFade(PathLocation location)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterEnterLocationScreenFade);
		gameEvent.TargetLocation = location;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterExitLocationToMap(PathLocation location)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterExitLocationToMap);
		gameEvent.TargetLocation = location;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerEnterRoom()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.EnterRoom);
		TriggerGameEvent(eve);
	}

	public void TriggerExitLocation(PathLocation location)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ExitLocation);
		gameEvent.TargetLocation = location;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerItemDowngrade(Item target, Item source)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.DowngradeItem);
		gameEvent.TargetItem = target;
		gameEvent.SourceItem = source;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerItemUpgrade(Item target, Item source)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.UpgradeItem);
		gameEvent.TargetItem = target;
		gameEvent.SourceItem = source;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerOverflow(Item target)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.Overflow);
		gameEvent.TargetItem = target;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforePlayerTurnEnd()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.BeforePlayerTurnEnd);
		TriggerGameEvent(eve);
	}

	public void TriggerAfterPlayerTurnEndWaste()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.AfterPlayerTurnEndWaste);
		TriggerGameEvent(eve);
	}

	public void TriggerPlayerTurnEnd()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.PlayerTurnEnd);
		gameState.battleState.SetPlayerAfterTurnEndFlag();
		TriggerGameEvent(eve);
	}

	public void TriggerPlayerTurnStart()
	{
		foreach (Item allRealItem in gameState.battleState.player.GetAllRealItems())
		{
			new ClearTransformAction(allRealItem).Excute(gameState);
		}
		GameEvent eve = GenerateGameEvent(GameEvent.Type.PlayerTurnStart);
		TriggerGameEvent(eve);
	}

	public void TriggerPlayerFirstTurnStart()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.PlayerFirstTurnStart);
		TriggerGameEvent(eve);
	}

	public void TriggerStartWorldEvent(WorldEvent worldEvent)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.StartWorldEvent);
		gameEvent.TargetWorldEvent = worldEvent;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBftHealthChange(BattleFieldTransform bft, int delta, object source, Damage damage, bool tempHealthChange = false, int requestDelta = 0, bool battleEndCleanup = false)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BftHealthChange);
		gameEvent.TargetBft = bft;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.deltaValue = delta;
		gameEvent.amount = requestDelta;
		gameEvent.boolValue = tempHealthChange;
		gameEvent.amount2 = (battleEndCleanup ? 1 : 0);
		gameEvent.damage = damage;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBftMaxHealthChange(BattleFieldTransform bft, int delta, object source)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BftMaxHealthChange);
		gameEvent.TargetBft = bft;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.deltaValue = delta;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterBftDeathBlow(BattleFieldTransform bft, object source, Damage damage)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterDeathblow);
		gameEvent.TargetBft = bft;
		gameEvent.damage = damage;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterEnemyDefeated(BattleFieldTransform bft, object source, Damage damage)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterEnemyDefeated);
		gameEvent.TargetBft = bft;
		gameEvent.damage = damage;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeBftDeathBlow(BattleFieldTransform bft, object source, Damage damage)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeDeathBlow);
		gameEvent.TargetBft = bft;
		gameEvent.damage = damage;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerMoonCocoonBreak(BattleFieldTransform bft, Item cocoon)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.MoonCocoonBreak);
		gameEvent.TargetBft = bft;
		gameEvent.SourceItem = cocoon;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerEnemyEnterPhase2(BattleFieldTransform bft)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.EnemyEnterPhase2);
		gameEvent.TargetBft = bft;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerEnemyEnterPhase2AfterConsumeItem(BattleFieldTransform bft)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.EnemyEnterPhase2AfterConsumeItem);
		gameEvent.TargetBft = bft;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerTurnPhaseChange()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.TriggerTurnPhaseChange);
		TriggerGameEvent(eve);
	}

	public void TriggerDrawFromPile(Item targetItem, Item sourceItem)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.DrawFromPile);
		gameEvent.TargetItem = targetItem;
		gameEvent.SourceItem = sourceItem;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerSlotMove(object source, Item target, int targetIndex, int deltaIndex)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.MoveSlotIndex);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.TargetItem = target;
		gameEvent.amount = targetIndex;
		gameEvent.deltaValue = deltaIndex;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerSlotIndexChange(object source, Item target, int beforeIndex, int deltaIndex)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SlotIndexChange);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.TargetItem = target;
		gameEvent.amount = beforeIndex;
		gameEvent.deltaValue = deltaIndex;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeMapGeneration()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.BeforeMapGeneration);
		TriggerGameEvent(eve);
	}

	public void TriggerBeforeStartGame()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.BeforeGameStart);
		TriggerGameEvent(eve);
	}

	public void TriggerBeforeGameGeneration()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.BeforeGameGeneration);
		TriggerGameEvent(eve);
	}

	public void TriggerAfterLoadGame()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.AfterLoadGame);
		TriggerGameEvent(eve);
	}

	public void TriggerBeforeGameEnd(bool success, bool isAfterIntroFirstGame)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeGameEnd);
		gameEvent.succesGame = success;
		gameEvent.isAfterIntroFirstGame = isAfterIntroFirstGame;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterGameEnd(bool success)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterGameEnd);
		gameEvent.succesGame = success;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAddInfluence(Influence influence)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AddInfluence);
		gameEvent.TargetInfluence = influence;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerRemoveInfluence(Influence influence)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.RemoveInfluence);
		gameEvent.TargetInfluence = influence;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterTurnStartDrawItems()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.AfterTurnStartDrawItems);
		TriggerGameEvent(eve);
	}

	public void TriggerBeforeEnemyEscape()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.BeforeEnemyEscape);
		TriggerGameEvent(eve);
	}

	public void TriggerAddSkill(GameSkill skill)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AddSkill);
		gameEvent.TargetSkill = skill;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerRemoveSkill(GameSkill skill)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.RemoveSkill);
		gameEvent.TargetSkill = skill;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerActivateSkill(GameSkill skill)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ActivateSkill);
		gameEvent.TargetSkill = skill;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerConsumeSkill(GameSkill skill)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeConsumeSkill);
		gameEvent.TargetSkill = skill;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerSkillDurabilityClean(GameSkill skill)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SkillDurabilityClean);
		gameEvent.TargetSkill = skill;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerSkillDurabilityChange(GameSkill skill, int delta, object source)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SkillDurabilityChange);
		gameEvent.TargetSkill = skill;
		gameEvent.deltaValue = delta;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerSwapSkill(GameSkill originSkill, GameSkill targetSkill)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeSwapSkill);
		gameEvent.SourceSkill = originSkill;
		gameEvent.TargetSkill = targetSkill;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterSwapItem(Item sourceItem, Item targetItem, Item swapSourceItem, ItemContainer container, int sourceBeforeIndex, int targetBeforeIndex)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterSwapItem);
		gameEvent.SourceItem = sourceItem;
		gameEvent.TargetItem = targetItem;
		gameEvent.SwapSourceItem = swapSourceItem;
		gameEvent.OriginContainer = container;
		gameEvent.amount = sourceBeforeIndex;
		gameEvent.amount2 = targetBeforeIndex;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterShuffleContainer(ItemContainer container)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterShuffleContainer);
		gameEvent.OriginContainer = container;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeEnterNextStage()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.BeforeEnterNextStage);
		TriggerGameEvent(eve);
	}

	public void TriggerAfterEnterNextStage()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.AfterEnterNextStage);
		TriggerGameEvent(eve);
	}

	public void TriggerBeforeBreakItem(Item target, object source, Damage damage, int? originalDefendCounter = null)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeBreakItem);
		gameEvent.TargetItem = target;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.damage = damage;
		gameEvent.breakItemOriginalDefendCounter = originalDefendCounter;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterBreakItem(Item target, object source, Damage damage, int? originalDefendCounter = null)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterBreakItem);
		gameEvent.TargetItem = target;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.damage = damage;
		gameEvent.breakItemOriginalDefendCounter = originalDefendCounter;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterTriggerChapter(Item target, int chapter)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterTriggerChapter);
		gameEvent.TargetItem = target;
		gameEvent.amount = chapter;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerTriggerChapterEffect(Item target)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.TriggerChapterEffect);
		gameEvent.TargetItem = target;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerConvertYinYangPower(TrigramsManager.YinYangType yinYangType, int amount, object source)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ConvertYinYangPower);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.boolValue = yinYangType == TrigramsManager.YinYangType.Yang;
		gameEvent.amount = amount;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerChangeYinYangPower(object source, int yin, int yang, bool temp = false)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ChangeYinYangPower);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.amount = yin;
		gameEvent.amount2 = yang;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerDrawCardRecycle(BattleFieldTransform bft)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.DrawCardRecycle);
		gameEvent.TargetBft = bft;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterSkillCast(GameSkill skill, GameEvent castEvent = null)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterSkillCast);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, skill);
		if (castEvent != null)
		{
			gameEvent.SelectedItems = ((castEvent.SelectedItems != null) ? new List<Item>(castEvent.SelectedItems) : new List<Item>());
			gameEvent.TargetItem = castEvent.TargetItem ?? ((gameEvent.SelectedItems.Count == 1) ? gameEvent.SelectedItems[0] : null);
		}
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAddRune(Item target, Rune rune)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AddRune);
		gameEvent.TargetRune = rune;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, rune);
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, target);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerRemoveRune(Item target, Rune rune)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.RemoveRune);
		gameEvent.TargetRune = rune;
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, target);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerRuneValueChange(Item target, Rune rune, int delta)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.RuneValueChange);
		gameEvent.TargetRune = rune;
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, target);
		gameEvent.deltaValue = delta;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerEnterCrystalFreeze(BattleFieldTransform bft)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.EnterCrystalFreeze);
		gameEvent.TargetBft = bft;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerExitCrystalFreeze(BattleFieldTransform bft)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ExitCrystalFreeze);
		gameEvent.TargetBft = bft;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeSummonCompanion(Item companionItem, BattleFieldTransform bft)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeSummonCompanion);
		gameEvent.TargetItem = companionItem;
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, bft);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerContainerSlotCountChange(ItemContainer container, int delta)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ContainerSlotCountChange);
		gameEvent.deltaValue = delta;
		gameEvent.DestinationContainer = container;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerStoreSellItem(Item item)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.StoreSellItem);
		gameEvent.TargetItem = item;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerMoveItemToHead(Item newHead, Item oldHead)
	{
		if (!(newHead?.def?.id == "afa80466af3115f4e823f85b3fb091b2"))
		{
			gameState.actionManager.BreakShadowEscapeChain(newHead?.owner);
		}
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.MoveItemToHead);
		gameEvent.TargetItem = newHead;
		gameEvent.SourceItem = oldHead;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerMoveItemToTail(Item newTail, Item oldTail)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.MoveItemToTail);
		gameEvent.TargetItem = newTail;
		gameEvent.SourceItem = oldTail;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterAddPrayToRitual(Pray pray)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterAddPrayToRitual);
		gameEvent.SetEveObj(GameEvent.EveObjType.Target, pray);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterRemovePrayFormRitual(Pray pray)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterRemovePrayFormRitual);
		gameEvent.SetEveObj(GameEvent.EveObjType.Target, pray);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterRitualTrigger()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.AfterRitualTrigger);
		TriggerGameEvent(eve);
	}

	public void TriggerSpawnItem(Item target, object source, ItemContainer container)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SpawnItem);
		gameEvent.TargetItem = target;
		gameEvent.DestinationContainer = container;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerJoinItemBattle(Item target)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.JoinItemBattle);
		gameEvent.TargetItem = target;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerSetEnvironmentInfluence(EnvironmentInfluence influence)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SetEnvironmentInfluence);
		gameEvent.SetEveObj(GameEvent.EveObjType.Target, influence);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerSideItemChange(Item souce, Item target)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SideItemChange);
		gameEvent.SourceItem = souce;
		gameEvent.TargetItem = target;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerShowForerunner()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.BeforeShowForerunner);
		TriggerGameEvent(eve);
	}

	public void TriggerBeforeBattlEncountEnd()
	{
		GameEvent eve = GenerateGameEvent(GameEvent.Type.BeforeBattlEncounterEnd);
		TriggerGameEvent(eve);
	}

	public void TriggerAfterTriggerCustomEvent(string eventName, object source)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterTriggerCustomEvent);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.stringValue = eventName;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterTriggerAbility(Item item)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterTriggerAbility);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, item);
		TriggerGameEvent(gameEvent);
	}

	public void TriggerBeforeChangeStanceValue(Stance origin, Stance destination, Stance deltaStance, object source, string stanceChangeType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BeforeChangeStance);
		gameEvent.SourceItem = source as Item;
		gameEvent.SetEveObj(GameEvent.EveObjType.Target, deltaStance);
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, destination);
		gameEvent.SetEveObj(GameEvent.EveObjType.Origin, origin);
		gameEvent.stringValue = stanceChangeType;
		TriggerGameEvent(gameEvent);
	}

	public void TriggerAfterChangeStanceValue(Stance origin, Stance destination, Stance deltaStance, object source, string stanceChangeType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AfterChangeStance);
		gameEvent.SourceItem = source as Item;
		gameEvent.SetEveObj(GameEvent.EveObjType.Target, deltaStance);
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, destination);
		gameEvent.SetEveObj(GameEvent.EveObjType.Origin, origin);
		gameEvent.stringValue = stanceChangeType;
		TriggerGameEvent(gameEvent);
	}

	public GameEvent.ResolveInfo TriggerResolveConsumeInfo(Item targetItem, Item sourceItem, ConsumeItemAction.ConsumeType type, SpellCastInfo spellInfo, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ConsumeResolveInfo, triggerType);
		gameEvent.consumeType = type;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.TargetItem = targetItem;
		gameEvent.SourceItem = sourceItem;
		gameEvent.spellInfo = spellInfo;
		TriggerGameEvent(gameEvent);
		ApplyPlayerCharacterConsumeResolve(gameEvent);
		return gameEvent.resolveInfo;
	}

	private void ApplyPlayerCharacterConsumeResolve(GameEvent eve)
	{
		if (eve.resolveInfo == null || eve.resolveInfo.reject || eve.consumeType != ConsumeItemAction.ConsumeType.Overflow)
		{
			return;
		}
		Item targetItem = eve.TargetItem;
		if (targetItem != null && targetItem.OwnerBft?.isPlayer == true && eve.TargetItem.Defence)
		{
			GameStateInfo gameStateInfo = gameState;
			if (gameStateInfo != null && gameStateInfo.playerState?.CharacterDef?.retainDefenceItemsOnOverflowConsume == true)
			{
				eve.resolveInfo.reject = true;
				eve.resolveInfo.rejectSource = eve.TargetItem;
			}
		}
	}

	public GameEvent.ResolveInfo TriggerResolveSpellCostConsumeInfo(Item targetItem, Item sourceItem, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SpellCostConsumeResolve, triggerType);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.TargetItem = targetItem;
		gameEvent.SourceItem = sourceItem;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerAddItemCounterResolveInfo(Item sourceItem, List<Item> toAddItems, GameEvent.TriggerType triggerType, SpellCastInfo spellInfo = null)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AddItemCounterResolveInfo, triggerType);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		foreach (Item toAddItem in toAddItems)
		{
			gameEvent.resolveInfo.itemValuePairs.Add((toAddItem, 0));
		}
		gameEvent.SourceItem = sourceItem;
		gameEvent.spellInfo = spellInfo;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerDeathCheckResolve(Item source, BattleFieldTransform bft, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BftDeathCheckResolve, triggerType);
		gameEvent.TargetBft = bft;
		gameEvent.SourceItem = source;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerSkillPropertyValueResolve(GameSkill sk, SkillDef.SkillPropertyType propertyType, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SkillPropertyValueResolve, triggerType);
		gameEvent.TargetSkill = sk;
		gameEvent.skillPropertyType = propertyType;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerHealResolve(object source, BattleFieldTransform bft, int delta, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.HealResolve, triggerType);
		gameEvent.TargetBft = bft;
		gameEvent.deltaValue = delta;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerItemDefendDamageAbleResolve(Item target)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ItemDefendDamageAbleResolve);
		gameEvent.TargetItem = target;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerGenerateDamageResolveInfo(DamageCommand damagecommand, GameEvent.TriggerType triggerType, Damage.ResolveType resolveType, Item defendTarget = null, Damage damage = null)
	{
		GameEvent gameEvent = null;
		switch (resolveType)
		{
		case Damage.ResolveType.BFT:
			gameEvent = GenerateGameEvent(GameEvent.Type.GenerateDamageBFTResolve, triggerType);
			break;
		case Damage.ResolveType.Pierce:
			gameEvent = GenerateGameEvent(GameEvent.Type.GenerateDamagePierceResolve, triggerType);
			break;
		case Damage.ResolveType.DamageValue:
			gameEvent = GenerateGameEvent(GameEvent.Type.GenerateDamageDamageValueResolve, triggerType);
			break;
		case Damage.ResolveType.DamageTimes:
			gameEvent = GenerateGameEvent(GameEvent.Type.GenerateDamageTimesResolve, triggerType);
			break;
		case Damage.ResolveType.RejectFinalDamage:
			gameEvent = GenerateGameEvent(GameEvent.Type.GenerateDamageSubCounterResolve, triggerType);
			break;
		}
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.damagecommand = damagecommand;
		gameEvent.damageResolveType = resolveType;
		gameEvent.dealTypeSourceOwnerOverride = FramingSkillAbility.GetDealTypeSourceOwnerOverride(gameState, damagecommand);
		if (gameEvent.dealTypeSourceOwnerOverride != null && damagecommand.sourceOwnerOverride == null)
		{
			damagecommand.sourceOwnerOverride = gameEvent.dealTypeSourceOwnerOverride;
		}
		if (defendTarget != null)
		{
			gameEvent.TargetItem = defendTarget;
		}
		else
		{
			gameEvent.TargetBft = damagecommand.targetBFT;
		}
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, damagecommand.source);
		gameEvent.spellInfo = damagecommand.spellCastInfo;
		gameEvent.damage = damage;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerGenerateAddCostResolveInfo(Item toAddCost, SpellCastInfo spellInfo, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AddCostResolveInfo, triggerType);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.spellInfo = spellInfo;
		gameEvent.SourceItem = spellInfo.spellItem;
		gameEvent.TargetItem = toAddCost;
		List<SpellCastInfo.AddedCostInfo> addedCostInfos = spellInfo.addedCostInfos;
		for (int i = 0; i < addedCostInfos.Count; i++)
		{
			gameEvent.resolveInfo.itemValuePairs.Add((addedCostInfos[i].item, 0));
		}
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerGenerateSpellCastResolveInfo(SpellCastInfo spellInfo, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SpellCastResolve, triggerType);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.spellInfo = spellInfo;
		gameEvent.SourceItem = spellInfo.spellItem;
		gameEvent.TargetItem = spellInfo.targetItem;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerGenerateSpellCastCostResolveInfo(SpellCastInfo spellInfo, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SpellCastCostResolve, triggerType);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.spellInfo = spellInfo;
		gameEvent.SourceItem = spellInfo.spellItem;
		gameEvent.TargetItem = spellInfo.targetItem;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerTurnDrawResolveInfo(BattleFieldTransform bft, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.TurnDrawResolve, triggerType);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.TargetBft = bft;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerFixedSlotCountResolveInfo(GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.FixedSlotCountResolve, triggerType);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.TargetBft = gameState.playerState.bft;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerCounterChangeResolveInfo(Item target, object source, int delta, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.CounterChangeResolveInfo, triggerType);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.TargetItem = target;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.deltaValue = delta;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerExileResolveInfo(Item targetItem, object source, ExileItemAction.ExileType exileType, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ExileResolveInfo, triggerType);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.exileType = exileType;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.TargetItem = targetItem;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerCrystalUpdatePriceResolveInfo(GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.GenerateCrystalUpgradeCostResolveInfo, triggerType);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerAddInherentItemsResolveInfo(Item item)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AddInherentItems);
		gameEvent.TargetItem = item;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.resolveInfo.reject = true;
		gameEvent.triggerType = GameEvent.TriggerType.Real;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerDrawItemResolve(Item item, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.DrawItemResolve, triggerType);
		gameEvent.TargetItem = item;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerShuffleResolve(Item targetItem, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ShuffleItemResolve, triggerType);
		gameEvent.TargetItem = targetItem;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerDrawCardRecycleResolve(BattleFieldTransform bft, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.DrawCardRecyleResolve, triggerType);
		gameEvent.TargetBft = bft;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerChangeEnchantResolve(int deltaAmount, Item targetItem, Enchant targetEnchant, object source, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ChangeEnchantResolve, triggerType);
		gameEvent.deltaValue = deltaAmount;
		gameEvent.TargetEnchant = targetEnchant;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.SetEveObj(GameEvent.EveObjType.Destination, targetItem);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerBreakItemResolve(Item target, object source, Damage damage, GameEvent.TriggerType triggerType, int? originalDefendCounter = null)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BreakItemResolve, triggerType);
		gameEvent.damage = damage;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.TargetItem = target;
		gameEvent.breakItemOriginalDefendCounter = originalDefendCounter;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerAddChapterResolve(Item target, object source, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AddChapterResolve, triggerType);
		gameEvent.TargetItem = target;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerTurnStartCoolDownResolve(Item target, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.TurnStartCoolDownResolve, triggerType);
		gameEvent.TargetItem = target;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo YinYangConvertRequirementResolve(object source, int amount, TrigramsManager.YinYangType yinYangType, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.YinYangConvertRequirementResolve, triggerType);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.boolValue = yinYangType == TrigramsManager.YinYangType.Yang;
		gameEvent.amount = amount;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo YinYangConsumeRequirementResolve(object source, int amount, TrigramsManager.YinYangType yinYangType, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.YinYangConsumeRequirementResolve, triggerType);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.boolValue = yinYangType == TrigramsManager.YinYangType.Yang;
		gameEvent.amount = amount;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerDamageHealthChangeResolve(Damage damage, BattleFieldTransform target, int healthDelta, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.DamageHealthChangeResolve, triggerType);
		gameEvent.deltaValue = healthDelta;
		gameEvent.damage = damage;
		gameEvent.TargetBft = target;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, damage.source);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerSpellCastCoolDownResolve(SpellCastInfo castInfo, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SpellCastCoolDownResolve, triggerType);
		gameEvent.spellInfo = castInfo;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.SourceItem = castInfo.spellItem;
		gameEvent.TargetItem = castInfo.targetItem;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerSpellCastEffectResolve(SpellCastInfo castInfo, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SpellCastEffectResolve, triggerType);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.spellInfo = castInfo;
		gameEvent.SourceItem = castInfo.spellItem;
		gameEvent.TargetItem = castInfo.targetItem;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerManualSelectCostResolve(Item source, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ManualSelectCostResolve, triggerType);
		gameEvent.SourceItem = source;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerGenerateOverflowDamageCommandResolve(Damage damage)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.GenerateOverflowDamageCommandResolveInfo);
		gameEvent.triggerType = GameEvent.TriggerType.Real;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.damage = damage;
		gameEvent.amount = damage.subCounter;
		gameEvent.TargetItem = damage.targetItem;
		if (gameEvent.TargetItem == null)
		{
			gameEvent.TargetBft = damage.targetBFT;
		}
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, damage.source);
		gameEvent.spellInfo = damage.sourceSpellInfo;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerEffectMarkResolve(Item target)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.EffectMarkResolve);
		gameEvent.TargetItem = target;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerAddItemToContainerResolve(AddItemEventInfo addInfo)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.AddItemToContainerResolve);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, addInfo.source);
		gameEvent.TargetItem = addInfo.addItem;
		gameEvent.DestinationContainer = addInfo.targetContainer;
		gameEvent.addItemInfo = addInfo;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerDrawItemReplaceResolve(Item oldDrawItem, BattleFieldTransform bft)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.DrawItemReplaceResolve);
		gameEvent.triggerType = GameEvent.TriggerType.Real;
		gameEvent.TargetItem = oldDrawItem;
		gameEvent.SetEveObj(GameEvent.EveObjType.Origin, bft);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerConsumeResetCounterResolve(Item targetItem)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ConsumeResetCounterResolve);
		gameEvent.TargetItem = targetItem;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerBftMaxHealthChangeResolve(BattleFieldTransform bft, int delta, object source)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.BftMaxHealthChangeResolve);
		gameEvent.TargetBft = bft;
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.deltaValue = delta;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerCoinChangeResolve(int delta)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.CoinChangeResolve);
		gameEvent.deltaValue = delta;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerCrystalChangeResolve(int delta)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.CrystalChangeResolve);
		gameEvent.deltaValue = delta;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerCanCastWithoutFulfillCostResolve(SpellCastInfo spellInfo)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.CanCastWithoutFulfillCostResolve);
		gameEvent.spellInfo = spellInfo;
		gameEvent.SourceItem = spellInfo.spellItem;
		gameEvent.TargetItem = spellInfo.targetItem;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerItemCastAbleInCoolDownResolve(Item targetItem, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ItemCastAbleInCoolDownResolve);
		gameEvent.TargetItem = targetItem;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.triggerType = triggerType;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerSkillCastCoolDownResolve(GameSkill skill)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SkillCastCoolDownResolve);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.TargetSkill = skill;
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerSpawnItemResolve(Item oldDrawItem, ItemContainer targetContainer)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.SpawnItemResolve);
		gameEvent.triggerType = GameEvent.TriggerType.Real;
		gameEvent.TargetItem = oldDrawItem;
		gameEvent.DestinationContainer = targetContainer;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerChangeStanceValueResolve(Stance deltaStance, object source, GameEvent.TriggerType triggerType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ChangeStanceValueResolve, triggerType);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.SetEveObj(GameEvent.EveObjType.Target, deltaStance);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerChangeStanceValueWithChangeTypeResolve(Stance deltaStance, object source, GameEvent.TriggerType triggerType, Stance.ChangeType stanceChangeType)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ChangeStanceValueWithChangeTypeResolve, triggerType);
		gameEvent.SetEveObj(GameEvent.EveObjType.Source, source);
		gameEvent.SetEveObj(GameEvent.EveObjType.Target, deltaStance);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		gameEvent.stringValue = stanceChangeType.ToString();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerDeathBlowEnterCrystalFreezeResolve(BattleFieldTransform target)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.DeathBlowEnterCrystalFreezeResolve);
		gameEvent.TargetBft = target;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerChangeTempHealthResolve(int delta, BattleFieldTransform bft)
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.ChangeTempHealthResolve);
		gameEvent.TargetBft = bft;
		gameEvent.deltaValue = delta;
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	public GameEvent.ResolveInfo TriggerRepeatStanceAbilityEffectResolve()
	{
		GameEvent gameEvent = GenerateGameEvent(GameEvent.Type.RepeatStanceAbilityEffectResolve);
		gameEvent.resolveInfo = new GameEvent.ResolveInfo();
		TriggerGameEvent(gameEvent);
		return gameEvent.resolveInfo;
	}

	private void EnqueueAction(GameAction action)
	{
		gameState.actionManager.EnqueueAction(action);
	}

	private GameEvent GenerateGameEvent(GameEvent.Type type, GameEvent.TriggerType triggerType = GameEvent.TriggerType.Real)
	{
		return new GameEvent(type, triggerType)
		{
			gameState = gameState
		};
	}

	private void TriggerGameEvent(GameEvent eve)
	{
		try
		{
			if (mockObjectForTextDisplay)
			{
				TriggerMockGameEvent(eve);
			}
			else
			{
				gameState.applicationState.eventManager.gameEvent.TriggerEvent(eve.type, eve);
			}
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, "GameEvent异常", ThinkingAnalyticsExtends.ExceptionType.GameEvent);
		}
	}

	private static IEnumerable<Item> GetAllNotifiableItems(GameEvent eve)
	{
		List<Item> res = new List<Item>();
		eve.gameState.battleState?.PlayerTableContainer?.GetAllItems(orderReverse: true).ForEach(delegate(Item it)
		{
			res.AddUnique(it);
		});
		eve.gameState.battleState?.PlayerDrawPileContainer?.GetAllItems(orderReverse: true).ForEach(delegate(Item it)
		{
			res.AddUnique(it);
		});
		eve.gameState.battleState?.PlayerWastePileContainer?.GetAllItems(orderReverse: true).ForEach(delegate(Item it)
		{
			res.AddUnique(it);
		});
		eve.gameState.battleState?.PlayerExilePileContainer?.GetAllItems(orderReverse: true).ForEach(delegate(Item it)
		{
			res.AddUnique(it);
		});
		eve.gameState.battleState?.PlayerDeckPileContainer?.GetAllItems(orderReverse: true).ForEach(delegate(Item it)
		{
			res.AddUnique(it);
		});
		eve.gameState.battleState?.PlayerCompanionContainer?.GetAllItems(orderReverse: true).ForEach(delegate(Item it)
		{
			res.AddUnique(it);
		});
		eve.gameState.battleState?.EnemyTableContainer?.GetAllItems(orderReverse: true).ForEach(delegate(Item it)
		{
			res.AddUnique(it);
		});
		eve.gameState.battleState?.EnemyDrawPileContainer?.GetAllItems(orderReverse: true).ForEach(delegate(Item it)
		{
			res.AddUnique(it);
		});
		eve.gameState.battleState?.EnemyWastePileContainer?.GetAllItems(orderReverse: true).ForEach(delegate(Item it)
		{
			res.AddUnique(it);
		});
		eve.gameState.battleState?.EnemyExilePileContainer?.GetAllItems(orderReverse: true).ForEach(delegate(Item it)
		{
			res.AddUnique(it);
		});
		eve.gameState.battleState?.EnemyDeckPileContainer?.GetAllItems(orderReverse: true).ForEach(delegate(Item it)
		{
			res.AddUnique(it);
		});
		eve.gameState.battleState?.EnemyCompanionContainer?.GetAllItems(orderReverse: true).ForEach(delegate(Item it)
		{
			res.AddUnique(it);
		});
		return res;
	}

	private void UpdateSubscribeGameEventDic(GameEvent eve)
	{
		subscribeGameEventObjectDic.Clear();
		if (itemSubscribeGameEventDic_stable == null)
		{
			itemSubscribeGameEventDic_stable = new Dictionary<GameEvent.Type, HashSet<IGameEventSubscriber>>();
			gameState.applicationState.achievementManager.achievements.OrEmptyIfNull().ForEach(TryAddToStableSubScribeDic);
		}
		TryAddToStableSubScribeDic(eve.gameState.contractState.selectedContract);
		foreach (DifficultyInfo unlockedDifficulty in eve.gameState.contractState.selectedContract.GetUnlockedDifficulties(gameState.applicationState))
		{
			TryAddToStableSubScribeDic(unlockedDifficulty);
		}
		foreach (KeyValuePair<GameEvent.Type, HashSet<IGameEventSubscriber>> item in itemSubscribeGameEventDic_stable)
		{
			subscribeGameEventObjectDic.Add(item.Key, new HashSet<IGameEventSubscriber>(item.Value));
		}
		TryAddToSubScribeDic(eve.gameState.stanceManager.currentStance);
		eve.gameState.playerState.bft.influences.ForEach(TryAddToSubScribeDic);
		eve.gameState.startGameBuffs.ForEach(TryAddToSubScribeDic);
		TryAddToSubScribeDic(eve.gameState.battleState.enemy);
		eve.gameState.battleState?.enemy?.bft.influences.ForEach(TryAddToSubScribeDic);
		if (gameState.playerState.MasteryInThisGame != null)
		{
			TryAddToSubScribeDic(gameState.playerState.MasteryInThisGame);
		}
		GetAllNotifiableItems(eve).ForEach(delegate(Item item)
		{
			TryAddToSubScribeDic(item);
			item.Enchants.OrEmptyIfNull().ForEach(TryAddToSubScribeDic);
			item.Runes.OrEmptyIfNull().ForEach(TryAddToSubScribeDic);
			item.tagInstances.OrEmptyIfNull().ForEach(TryAddToSubScribeDic);
		});
		gameState.playerState.AllSkills.ForEach(delegate(GameSkill skill)
		{
			if (skill != null)
			{
				TryAddToSubScribeDic(skill);
			}
		});
		gameState.playerState.ActivatedSkills.ForEach(delegate(GameSkill skill)
		{
			if (skill == null)
			{
				return;
			}
			foreach (SkillWord item2 in skill.skillWords.OrEmptyIfNull())
			{
				TryAddToSubScribeDic(item2);
			}
		});
		TryAddToSubScribeDic(eve.gameState.currentEncounter?.environmentInfluence);
		static void TryAddToDic(IGameEventSubscriber sub, Dictionary<GameEvent.Type, HashSet<IGameEventSubscriber>> dic)
		{
			if (sub == null)
			{
				return;
			}
			foreach (GameEvent.Type item3 in sub.GetAllSubscribeGameEventType().Distinct())
			{
				if (!dic.ContainsKey(item3))
				{
					dic.Add(item3, new HashSet<IGameEventSubscriber>());
				}
				if (!dic[item3].Contains(sub))
				{
					dic[item3].Add(sub);
				}
			}
		}
		void TryAddToStableSubScribeDic(IGameEventSubscriber sub)
		{
			TryAddToDic(sub, itemSubscribeGameEventDic_stable);
		}
		void TryAddToSubScribeDic(IGameEventSubscriber sub)
		{
			TryAddToDic(sub, subscribeGameEventObjectDic);
		}
	}

	public HashSet<IGameEventSubscriber> GetEventRelatedTriggers(GameEvent eve)
	{
		HashSet<IGameEventSubscriber> hashSet = new HashSet<IGameEventSubscriber>();
		List<object> list = eve.eveObjDic.Values.ToList();
		if (eve.spellInfo?.spellItem != null)
		{
			list.Add(eve.spellInfo.spellItem);
		}
		foreach (object item3 in list)
		{
			if (item3 is IGameEventSubscriber item)
			{
				hashSet.Add(item);
				if (item3 is Item item2)
				{
					hashSet.AddRange(item2.Enchants.OrEmptyIfNull());
					hashSet.AddRange(item2.Runes.OrEmptyIfNull());
					hashSet.AddRange(item2.tagInstances.OrEmptyIfNull());
				}
			}
		}
		return hashSet;
	}

	private IEnumerable<IGameEventSubscriber> GetAllGameEventSubscribers(GameEvent eve)
	{
		if (AllToUpdateNotifyGameEvent.Contains(eve.type))
		{
			UpdateSubscribeGameEventDic(eve);
		}
		if (eve.triggerType == GameEvent.TriggerType.OutBattleDisplay)
		{
			return GetEventRelatedTriggers(eve).Distinct();
		}
		subscribeGameEventObjectDic.TryGetValue(eve.type, out var value);
		HashSet<IGameEventSubscriber> hashSet = ((value == null) ? new HashSet<IGameEventSubscriber>() : new HashSet<IGameEventSubscriber>(value));
		foreach (IGameEventSubscriber eventRelatedTrigger in GetEventRelatedTriggers(eve))
		{
			hashSet.Add(eventRelatedTrigger);
		}
		return hashSet.Distinct();
	}

	private void TriggerGameEventForNotifiables(GameEvent.Type type, GameEvent eve)
	{
		if (gameState.actionManager == null || ShouldSuppressTriggerAfterEnemyDeath(eve))
		{
			return;
		}
		List<IGameEventSubscriber> list = GetAllGameEventSubscribers(eve).Distinct().ToList().Where(delegate(IGameEventSubscriber t)
		{
			eve.SetCaster(t);
			return t.CheckTriggerGameEventAble(eve);
		})
			.ToList();
		eve.SetCaster(null);
		if (eve.IsInstantTriggerEvent())
		{
			foreach (IGameEventSubscriber item in list)
			{
				TriggerWithGameEvent(eve, item);
			}
			onTriggerGameEvent?.Invoke(eve);
			return;
		}
		foreach (IGameEventSubscriber trigger in list)
		{
			EnqueueAction(new ActionAction(delegate
			{
				TriggerWithGameEvent(eve, trigger);
			}));
		}
		EnqueueAction(new ActionAction(delegate
		{
			onTriggerGameEvent?.Invoke(eve);
		}));
	}

	private bool ShouldSuppressTriggerAfterEnemyDeath(GameEvent eve)
	{
		BattleStateInfo battleState = gameState.battleState;
		if (battleState == null || battleState.enemy?.deathPending != true)
		{
			return false;
		}
		if (eve.type != GameEvent.Type.AfterEnemyDefeated && eve.type != GameEvent.Type.SkillDurabilityChange && eve.type != GameEvent.Type.BeforeBattlEncounterEnd && eve.type != GameEvent.Type.BeforeBattleEnd)
		{
			return eve.type != GameEvent.Type.AfterBattleEnd;
		}
		return false;
	}

	private void TriggerWithGameEvent(GameEvent eve, IGameEventSubscriber trigger)
	{
		try
		{
			eve.SetCaster(trigger);
			trigger.TriggerGameEvent(eve);
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, $"GameEvent Trigger Exception：{trigger}-{eve.type}");
		}
	}

	public void TriggerMockGameEvent(GameEvent eve)
	{
		foreach (IGameEventSubscriber eventRelatedTrigger in GetEventRelatedTriggers(eve))
		{
			eve.SetCaster(eventRelatedTrigger);
			eventRelatedTrigger.TriggerGameEvent(eve);
		}
	}

	private void ClearBlockyDelicacyFlyVfxOnUltimatePurification(SpellCastInfo castInfo)
	{
		Item item = castInfo?.spellItem;
		BattleFieldTransform battleFieldTransform = item?.OwnerBft;
		if (castInfo != null && castInfo.casterType == SpellCastInfo.CasterType.Enemy && battleFieldTransform != null && battleFieldTransform.isEnemy && IsUltimatePurification(item))
		{
			BlockyDelicacyPersistentFlyVfx.ClearFor(gameState?.applicationState, battleFieldTransform);
		}
	}

	private static bool IsUltimatePurification(Item item)
	{
		if (item?.def != null)
		{
			if (!MatchesUltimatePurificationItemName(item.ItemName) && !MatchesUltimatePurificationItemName(item.ItemNameWithUpgrade))
			{
				return MatchesUltimatePurificationItemName(item.def.name);
			}
			return true;
		}
		return false;
	}

	private static bool MatchesUltimatePurificationItemName(string itemName)
	{
		if (string.IsNullOrEmpty(itemName))
		{
			return false;
		}
		foreach (string ultimatePurificationItemName in UltimatePurificationItemNames)
		{
			if (itemName == ultimatePurificationItemName || itemName.StartsWith(ultimatePurificationItemName + "+", StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}
}
