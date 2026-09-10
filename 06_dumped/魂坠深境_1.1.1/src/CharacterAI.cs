using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class CharacterAI : IDisposable, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber, IBftOwnable
{
	public BattleFieldTransform bft;

	public CharacterAIComponent component;

	public string overrideEnemyName;

	public EnemyEncounterType enemyEncounterType;

	public float enemyProgressLevel;

	public int firstPhaseMaxHealth;

	public int secondPhaseMaxHealth;

	public int turnDrawCount;

	public bool inPhase2;

	public List<Item> phase2Items;

	public List<Item> castedItemThisTurn = new List<Item>();

	public List<Item> stayWaitCoolDownItemsThisTurn = new List<Item>();

	public List<Item> thisTurnStartItems = new List<Item>();

	private bool currentIntentFinishFlag;

	public float idleDialogueTimer;

	public bool dead;

	public bool deathPending;

	public DateTime deathPendingSinceUtc;

	public BattleFieldTransformDisplay BftDisplay
	{
		get
		{
			if (!(component == null))
			{
				return component.BftDisplay;
			}
			return null;
		}
	}

	public string EnemyName => overrideEnemyName ?? component.EnemyName;

	public GameStateInfo GameState => bft.gameState;

	public bool SecondPhaseAbilityEnable
	{
		get
		{
			if ((enemyEncounterType == EnemyEncounterType.Boss || enemyEncounterType == EnemyEncounterType.Elite) && GameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableEliteSecondPhaseAbility) && component.secondPhaseAbilities != null)
			{
				return component.secondPhaseAbilities.Count > 0;
			}
			return false;
		}
	}

	public BattleFieldTransform OwnerBft => BftDisplay.attachBft;

	private CharacterAI()
	{
	}

	private CharacterAI(BattleFieldTransform bft, CharacterAIComponent component)
	{
		this.bft = bft;
		this.component = component;
		GameState.gameEventManager.onTriggerGameEvent += _OnTriggerEvent;
		GameState.actionManager.actionQueueCleanEvent += _OnActionQueueCleanUpdate;
		bft.Attach(this);
	}

	public CharacterAI(BattleFieldTransform bft, CharacterAIComponent component, CharacterAIComponent.EnemySpawnSetup setup, EnemyEncounterType encounterType)
		: this(bft, component)
	{
		EnemyInfoData infoData = component.GetInfoData(GameState.applicationState);
		EnemyInitInfo enemyInitInfo = new EnemyInitInfo(GameState, encounterType, setup, infoData);
		enemyEncounterType = encounterType;
		enemyProgressLevel = enemyInitInfo.enemyProgressLevel;
		int enemyTotalHealthValue = ScriptableObjectSingleton<GameStatisticData>.instance.GetEnemyTotalHealthValue(enemyInitInfo);
		int enemyFirstPhaseHealthValue = ScriptableObjectSingleton<GameStatisticData>.instance.GetEnemyFirstPhaseHealthValue(enemyInitInfo);
		int enemySecondPhaseHealthValue = ScriptableObjectSingleton<GameStatisticData>.instance.GetEnemySecondPhaseHealthValue(enemyInitInfo);
		if (encounterType == EnemyEncounterType.Elite || encounterType == EnemyEncounterType.Boss)
		{
			firstPhaseMaxHealth = enemyFirstPhaseHealthValue;
			secondPhaseMaxHealth = enemySecondPhaseHealthValue;
			bft.maxHealth = firstPhaseMaxHealth;
			bft.health = firstPhaseMaxHealth;
			bft.extraHealthPoint = 1;
		}
		else
		{
			firstPhaseMaxHealth = enemyTotalHealthValue;
			secondPhaseMaxHealth = 0;
			bft.maxHealth = enemyTotalHealthValue;
			bft.health = enemyTotalHealthValue;
		}
		turnDrawCount = setup.difficultyInfo.intentSlotCount;
		List<Item> list = enemyInitInfo.GenerateInitDeckItems();
		phase2Items = enemyInitInfo.GeneratePhase2SpawnItems();
		foreach (Item item in list)
		{
			EnqueueAction(new SpawnItemAction(item, bft.deckContainer));
		}
		EnqueueAction(new InitDrawPileAction(bft));
	}

	public CharacterAI(BattleFieldTransform bft, CharacterAIComponent component, PVPDefender defender)
		: this(bft, component)
	{
		PVPDefenderProperty pVPDefenderProperty = component.pvpEnemyAvatarConfig.GenerateBaseDefenderProperty(defender.deckPreset);
		enemyProgressLevel = 1f;
		enemyEncounterType = EnemyEncounterType.Normal;
		firstPhaseMaxHealth = pVPDefenderProperty.health;
		secondPhaseMaxHealth = 0;
		bft.maxHealth = pVPDefenderProperty.health;
		bft.health = pVPDefenderProperty.health;
		turnDrawCount = pVPDefenderProperty.turnDrawCount;
		overrideEnemyName = defender.defenderName;
		foreach (Item allItem in defender.deckPreset.GetAllItems())
		{
			EnqueueAction(new SpawnItemAction(allItem, bft.deckContainer));
		}
		foreach (Influence allInfluence in defender.deckPreset.GetAllInfluences())
		{
			EnqueueAction(new AddInfluenceAction(allInfluence, bft));
		}
		EnqueueAction(new InitDrawPileAction(bft));
	}

	public void Dispose()
	{
		if (GameState != null)
		{
			GameState.gameEventManager.onTriggerGameEvent -= _OnTriggerEvent;
			GameState.actionManager.actionQueueCleanEvent -= _OnActionQueueCleanUpdate;
			if (component != null)
			{
				UnityEngine.Object.Destroy(component.gameObject);
			}
		}
	}

	public void _OnActionQueueCleanUpdate()
	{
		if (!deathPending)
		{
			TryEnterDeathPending();
		}
		if (deathPending)
		{
			Die();
		}
	}

	public void TryEnterDeathPending(object source = null, Damage damage = null)
	{
		if (!dead && !deathPending && (int)bft.health <= 0 && !GameState.gameEventManager.TriggerDeathCheckResolve(null, bft, GameEvent.TriggerType.Real).reject)
		{
			deathPending = true;
			deathPendingSinceUtc = DateTime.UtcNow;
			GameState.gameEventManager.TriggerAfterEnemyDefeated(bft, source, damage);
		}
	}

	public void Die()
	{
		if (!dead)
		{
			EnqueueAction(new FinishBattleEncounterAction(FinishBattleEncounterAction.FinishType.Win));
			dead = true;
		}
	}

	private void _OnTriggerEvent(GameEvent eve)
	{
		if (eve.IsResolveGameEvent())
		{
			return;
		}
		if (eve.type == GameEvent.Type.AfterDeathblow && eve.TargetBft == bft)
		{
			TryEnterDeathPending(eve.GetEveObj(GameEvent.EveObjType.Source), eve.damage);
		}
		TriggerDialogue(eve);
		if (eve.type == GameEvent.Type.BeforeBattleEnd && eve.battleFinishType == FinishBattleEncounterAction.FinishType.Win && !BftDisplay.HasDefeatDialogTrigger())
		{
			TriggerDefeatedDialog();
		}
		else if (eve.type == GameEvent.Type.AfterDamage)
		{
			BattleFieldTransform targetBft = eve.TargetBft;
			if (targetBft != null && targetBft.isEnemy && !BftDisplay.HasHitDialogTrigger())
			{
				TriggerHitDialog();
			}
		}
	}

	public DialoguePlayer GetOrAddDialogPlayer()
	{
		return bft.characterAI.component.GetOrAddDialoguePlayer();
	}

	private void TriggerDialogue(GameEvent eve)
	{
		DialoguePlayer orAddDialogPlayer = GetOrAddDialogPlayer();
		if (!(orAddDialogPlayer != null))
		{
			return;
		}
		foreach (DialogueTrigger item in component.dialogueTriggers.OrEmptyIfNull())
		{
			if (item.triggerType != DialogueTrigger.TriggerType.Idle && item.triggerType != DialogueTrigger.TriggerType.Defeated && item.triggerType != DialogueTrigger.TriggerType.Hit && item.triggerType != DialogueTrigger.TriggerType.CastItem && item.CheckGameEventTriggerAble(eve, orAddDialogPlayer))
			{
				if (item.triggerType == DialogueTrigger.TriggerType.KillPlayer)
				{
					item.Trigger(orAddDialogPlayer, eve.gameState, direct: true);
				}
				else
				{
					item.Trigger(orAddDialogPlayer, eve.gameState);
				}
			}
		}
	}

	public void TriggerDialogue(string eventName, ItemDefinition itemDefinition)
	{
		DialoguePlayer orAddDialogPlayer = GetOrAddDialogPlayer();
		if (orAddDialogPlayer == null)
		{
			return;
		}
		foreach (DialogueTrigger item in component.dialogueTriggers.OrEmptyIfNull())
		{
			if (item.CheckAnimEventTriggerAble(orAddDialogPlayer, eventName, itemDefinition))
			{
				item.Trigger(orAddDialogPlayer, GameState);
				break;
			}
		}
	}

	public void TriggerIdleDialog()
	{
		DialoguePlayer dialoguePlayer = GetOrAddDialogPlayer();
		if (!(dialoguePlayer == null))
		{
			List<DialogueTrigger> list = component.dialogueTriggers.OrEmptyIfNull().ToList().FindAll((DialogueTrigger t) => t.triggerType == DialogueTrigger.TriggerType.Idle && t.CheckIdleTriggerAble(GameState, dialoguePlayer));
			if (!list.IsNullOrEmpty())
			{
				list.SelectRandom().Trigger(dialoguePlayer, GameState);
			}
		}
	}

	public void TriggerDefeatedDialog()
	{
		DialoguePlayer dialoguePlayer = GetOrAddDialogPlayer();
		if (!(dialoguePlayer == null))
		{
			List<DialogueTrigger> list = component.dialogueTriggers.OrEmptyIfNull().ToList().FindAll((DialogueTrigger t) => t.triggerType == DialogueTrigger.TriggerType.Defeated && t.CheckDefeatedTriggerAble(GameState, dialoguePlayer));
			if (!list.IsNullOrEmpty())
			{
				list.SelectRandom().Trigger(dialoguePlayer, GameState);
			}
		}
	}

	public void TriggerHitDialog()
	{
		DialoguePlayer dialoguePlayer = GetOrAddDialogPlayer();
		if (!(dialoguePlayer == null))
		{
			List<DialogueTrigger> list = component.dialogueTriggers.OrEmptyIfNull().ToList().FindAll((DialogueTrigger t) => t.triggerType == DialogueTrigger.TriggerType.Hit && t.CheckHitTriggerAble(GameState, dialoguePlayer));
			if (!list.IsNullOrEmpty())
			{
				list.SelectRandom().Trigger(dialoguePlayer, GameState);
			}
		}
	}

	private void EnqueueAction(GameAction action)
	{
		GameState.actionManager.EnqueueAction(action);
	}

	private void ExecuteIntent(Item intentItem)
	{
		currentIntentFinishFlag = false;
		ItemDisplayBase display = ItemDisplayBase.GetActiveItemSlotDisplay(intentItem);
		EnqueueAction(new ActionAction(delegate
		{
			GameState.UIManager.enemyBftUIDisplay.canvasGroup.alpha = 0.2f;
			if (display != null)
			{
				display.GetComponent<CanvasGroup>().ignoreParentGroups = true;
			}
		}));
		EnqueueAction(new ActionAction(delegate
		{
			GameState.UIManager.enemyBftUIDisplay.tableItemArea.OnItemSpell(intentItem);
		}));
		if (SpiderContractEnemyAbilityUtility.SuppressInitialIntentPerformance(intentItem))
		{
			EnqueueAction(new ActionAction(EffectAction));
		}
		else
		{
			EnqueueAction(new PlayPerformanceAction(intentItem, bft, EnemyAnimationType.none, waitForCompletion: true, EffectAction));
		}
		void EffectAction()
		{
			EnqueueAction(new ActionAction(delegate
			{
				RealSpellCastInfo.TriggerSpellCast(GameState, intentItem, null, SpellCastInfo.CasterType.Enemy);
			}));
			EnqueueAction(new ActionAction(delegate
			{
				GameState.UIManager.enemyBftUIDisplay.canvasGroup.alpha = 1f;
				if (display != null)
				{
					display.GetComponent<CanvasGroup>().ignoreParentGroups = false;
				}
			}));
			currentIntentFinishFlag = true;
		}
	}

	private IEnumerator<CustomYieldInstruction> RefreshAllIntent()
	{
		int stayCount = 0;
		foreach (Item allItem in bft.tableContainer.GetAllItems())
		{
			bool flag = !thisTurnStartItems.Contains(allItem);
			if (allItem.FromDeck)
			{
				if (allItem.blackboard.StayInTable || stayWaitCoolDownItemsThisTurn.Contains(allItem))
				{
					stayCount++;
				}
				else
				{
					EnqueueAction(new ConsumeItemAction(allItem, ConsumeItemAction.ConsumeType.Effect));
				}
			}
			else if (!flag && !allItem.blackboard.StayInTable)
			{
				EnqueueAction(new ConsumeItemAction(allItem, ConsumeItemAction.ConsumeType.Effect));
			}
		}
		yield return new WaitUntil(() => GameState.actionManager.isEmpty);
		EnqueueAction(new ConsumeOverflowSlotAction(bft.tableContainer));
		foreach (Item allCoolDownItem in bft.GetAllCoolDownItems())
		{
			if (allCoolDownItem.IsCoolDowning)
			{
				EnqueueAction(new ChangeCoolDownAction(allCoolDownItem, null, -1));
			}
		}
		int num = bft.characterAI.turnDrawCount;
		num -= stayCount;
		GameEvent.ResolveInfo resolveInfo = GameState.gameEventManager.TriggerTurnDrawResolveInfo(bft, GameEvent.TriggerType.Real);
		num += resolveInfo.deltaValue;
		if (resolveInfo.reject)
		{
			num = 0;
		}
		for (int num2 = 0; num2 < num; num2++)
		{
			EnqueueAction(new DrawFromPileAction(bft, null));
		}
		castedItemThisTurn.Clear();
		stayWaitCoolDownItemsThisTurn.Clear();
		yield return new WaitUntil(() => GameState.actionManager.isEmpty);
	}

	private IEnumerator<CustomYieldInstruction> ExecuteIntentOneByOne()
	{
		yield return new WaitUntil(() => GameState.actionManager.isEmpty);
		castedItemThisTurn.Clear();
		stayWaitCoolDownItemsThisTurn.Clear();
		thisTurnStartItems.Clear();
		foreach (Item allItem in bft.tableContainer.GetAllItems())
		{
			thisTurnStartItems.Add(allItem);
		}
		while (true)
		{
			if (GameState.battleState.endEnemyTurnFlag)
			{
				GameState.battleState.endEnemyTurnFlag = false;
				break;
			}
			if (!GameState.battleState.isInBattle)
			{
				break;
			}
			Item castableItem = bft.tableContainer.GetAllItems(orderReverse: true).FirstOrDefault((Item item) => item.HasSpell && item.CoolDowned && !castedItemThisTurn.Contains(item));
			if (castableItem == null || bft.crystalFreeze)
			{
				break;
			}
			EnqueueAction(new ActionAction(delegate
			{
				ExecuteIntent(castableItem);
			}));
			yield return new WaitUntil(() => GameState.actionManager.isEmpty && currentIntentFinishFlag);
			yield return new WaitUntil(() => GameState.actionManager.isEmpty);
			castedItemThisTurn.AddUnique(castableItem);
		}
		foreach (Item allItem2 in bft.tableContainer.GetAllItems())
		{
			if (allItem2.IsCoolDowning && !castedItemThisTurn.Contains(allItem2) && !stayWaitCoolDownItemsThisTurn.Contains(allItem2))
			{
				stayWaitCoolDownItemsThisTurn.Add(allItem2);
			}
		}
		yield return new WaitUntil(() => GameState.actionManager.isEmpty);
	}

	public void InitAfterBattleStart()
	{
		foreach (Item allItem in bft.tableContainer.GetAllItems(orderReverse: true))
		{
			if (allItem.FromDeck && !allItem.blackboard.StayInTable)
			{
				EnqueueAction(new ConsumeItemAction(allItem, ConsumeItemAction.ConsumeType.Effect));
			}
		}
		if (bft.crystalFreeze)
		{
			return;
		}
		int num = turnDrawCount;
		GameEvent.ResolveInfo resolveInfo = GameState.gameEventManager.TriggerTurnDrawResolveInfo(bft, GameEvent.TriggerType.Real);
		num += resolveInfo.deltaValue;
		if (resolveInfo.reject)
		{
			num = 0;
		}
		List<Item> allItems = bft.drawPileItemContainer.GetAllItems();
		EnemyInfoData enemyInfoData = component?.GetInfoData(GameState.applicationState);
		ContractDef contractDef = GameState.contractState?.selectedContract?.contractDef;
		HashSet<Item> hashSet = new HashSet<Item>(allItems.Where((Item item) => item.blackboard.InitSpawn && (contractDef?.IsEnemyInitialItemOutsideIntentLimit(GameState, enemyInfoData, enemyEncounterType, item) ?? false)));
		EnqueueAction(new ChangeSlotCountAction(bft.tableContainer, num + hashSet.Count, temp: false));
		int num2 = 0;
		foreach (Item item2 in allItems)
		{
			if (!GameState.gameEventManager.TriggerAddInherentItemsResolveInfo(item2).reject)
			{
				EnqueueAction(new TransferItemAction(item2, bft.tableContainer, null, TransferItemAction.TransferAnimType.Transfer));
				num2++;
			}
		}
		foreach (Item item3 in allItems)
		{
			if (!item3.blackboard.InitSpawn)
			{
				continue;
			}
			bool flag = hashSet.Contains(item3);
			if (flag || num2 < num)
			{
				EnqueueAction(new DrawSpecificItemFromPileAction(bft, item3, null));
				if (!flag)
				{
					num2++;
				}
			}
		}
		if (!GameState.applicationState.guideManager.blockEnemyDrawItems)
		{
			for (int num3 = num2; num3 < num; num3++)
			{
				EnqueueAction(new DrawFromPileAction(bft, null));
			}
		}
		castedItemThisTurn.Clear();
		stayWaitCoolDownItemsThisTurn.Clear();
		EnqueueAction(new ActionAction(delegate
		{
			foreach (Item item in bft.drawPileItemContainer.GetAllItems(orderReverse: true))
			{
				ItemBlackBoard blackboard = item.blackboard;
				if (blackboard != null && blackboard.BattleStartCast)
				{
					EnqueueAction(new ActionAction(delegate
					{
						ExecuteIntent(item);
					}));
				}
			}
		}));
	}

	public IEnumerator EnemyTurnCor()
	{
		EnqueueAction(new ChangeTempHealthAction(-999, GameState.battleState.enemy.bft));
		EnqueueAction(new ActionAction(delegate
		{
			GameState.gameEventManager.TriggerEnemyTurnStart();
		}));
		if (bft.crystalFreeze)
		{
			EnqueueAction(new ExitCrystalFreezeAction(bft));
		}
		yield return new WaitUntil(() => GameState.actionManager.isEmpty);
		yield return ExecuteIntentOneByOne();
		yield return new WaitUntil(() => GameState.actionManager.isEmpty);
		EnqueueAction(new ActionAction(delegate
		{
			GameState.gameEventManager.TriggerEnemyTurnEnd();
		}));
		yield return new WaitUntil(() => GameState.actionManager.isEmpty);
		if (!bft.crystalFreeze && !GameState.applicationState.guideManager.blockEnemyDrawItems)
		{
			yield return RefreshAllIntent();
		}
		yield return new WaitUntil(() => GameState.actionManager.isEmpty);
		GameState.battleState.endEnemyTurnFlag = false;
		GameState.actionManager.EnqueueAction(new WaitAction(0.25f));
	}

	public void FrameUpdate()
	{
		if (BftDisplay.HasIdleDialogTrigger())
		{
			return;
		}
		DialoguePlayer orAddDialogPlayer = GetOrAddDialogPlayer();
		if (!GameState.actionManager.isEmpty)
		{
			return;
		}
		BattleStateInfo battleState = GameState.battleState;
		if (battleState != null && battleState.turnPhase == TurnPhase.PlayerControlPhase && orAddDialogPlayer != null && !orAddDialogPlayer.IsPlaying(GameState))
		{
			idleDialogueTimer += Time.deltaTime;
			if (idleDialogueTimer > ScriptableObjectSingleton<VFXDataPreset>.instance.idleDialogueTimeInterval)
			{
				idleDialogueTimer = 0f;
				BftDisplay.attachBft.characterAI.TriggerIdleDialog();
			}
		}
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public List<IEffectNode> GetChildren()
	{
		return component.GetChildren();
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		foreach (Ability item in component.secondPhaseAbilities.OrEmptyIfNull())
		{
			foreach (GameEvent.Type item2 in item.GetAllSubscribeGameEventType())
			{
				yield return item2;
			}
		}
		foreach (Ability item3 in component.stableAbilities.OrEmptyIfNull())
		{
			foreach (GameEvent.Type item4 in item3.GetAllSubscribeGameEventType())
			{
				yield return item4;
			}
		}
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (component.secondPhaseAbilities != null && SecondPhaseAbilityEnable && inPhase2)
		{
			foreach (Ability secondPhaseAbility in component.secondPhaseAbilities)
			{
				if (secondPhaseAbility.CheckTriggerGameEventAble(eve))
				{
					return true;
				}
			}
		}
		if (component.stableAbilities != null)
		{
			foreach (Ability stableAbility in component.stableAbilities)
			{
				if (stableAbility.CheckTriggerGameEventAble(eve))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		if (component.secondPhaseAbilities != null && SecondPhaseAbilityEnable && inPhase2)
		{
			foreach (Ability secondPhaseAbility in component.secondPhaseAbilities)
			{
				if (secondPhaseAbility.CheckTriggerGameEventAble(eve))
				{
					secondPhaseAbility.TriggerGameEvent(eve);
				}
			}
		}
		if (component.stableAbilities == null)
		{
			return;
		}
		foreach (Ability stableAbility in component.stableAbilities)
		{
			if (stableAbility.CheckTriggerGameEventAble(eve))
			{
				stableAbility.TriggerGameEvent(eve);
			}
		}
	}
}
