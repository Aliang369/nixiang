public class StopBattleAction : ExecuteAction
{
	private readonly FinishBattleEncounterAction.FinishType type;

	public StopBattleAction(FinishBattleEncounterAction.FinishType type)
	{
		this.type = type;
	}

	protected override void Update()
	{
		CharacterAI characterAI = gameState.battleState?.enemy;
		if (characterAI == null)
		{
			TraceLog.Error("[StopBattleAction] Update() - enemy == null");
			return;
		}
		BattleFieldTransform playerBft = gameState.playerState.bft;
		if (playerBft == null)
		{
			TraceLog.Error("[StopBattleAction] Update() - playerBft == null");
			return;
		}
		if (!gameState.IsCharacterTrialGame)
		{
			ThinkingAnalyticsExtends.TrackBattleOver(gameState, type);
			ThinkingAnalyticsExtends.TrackRoundOver(gameState, battleStop: true);
			gameState.gameEventManager.TriggerBeforeBattleEnd(type);
			gameState.recordManager.TrackJournalRecord(new FinishBattleRecord(characterAI.component.GetInfoData(gameState.applicationState), type));
		}
		if (type == FinishBattleEncounterAction.FinishType.Win)
		{
			EnqueueAction(new PlayPerformanceAction(null, characterAI.bft, EnemyAnimationType.death, waitForCompletion: true));
		}
		EnqueueAction(new ActionAction(delegate
		{
			gameState.battleState.StopBattleState();
		}));
		EnqueueAction(new RefreshAllSkillAction());
		foreach (GameSkill activatedSkill in gameState.playerState.ActivatedSkills)
		{
			if (activatedSkill.DurabilityType == SkillDef.SkillPropertyType.BattleDurability)
			{
				EnqueueAction(new ChangeSkillDurabilityAction(activatedSkill, -1));
			}
		}
		EnqueueAction(ChangeTempHealthAction.CreateBattleEndCleanup(playerBft));
		EnqueueAction(new ActionAction(delegate
		{
			if (playerBft.tableContainer != null)
			{
				playerBft.tableContainer.tempFixedSlotCount = 0;
			}
			foreach (Item allItem in playerBft.tableContainer.GetAllItems(orderReverse: true))
			{
				playerBft.tableContainer.RemoveItem(allItem, null, clean: true);
			}
			foreach (Item allItem2 in playerBft.drawPileItemContainer.GetAllItems(orderReverse: true))
			{
				playerBft.drawPileItemContainer.RemoveItem(allItem2, null, clean: true);
			}
			foreach (Item allItem3 in playerBft.wastePileItemContainer.GetAllItems(orderReverse: true))
			{
				playerBft.wastePileItemContainer.RemoveItem(allItem3, null, clean: true);
			}
			foreach (Item allItem4 in playerBft.companionContainer.GetAllItems(orderReverse: true))
			{
				playerBft.companionContainer.RemoveItem(allItem4, null, clean: true);
			}
		}));
		if (!gameState.IsCharacterTrialGame)
		{
			EnqueueAction(new ActionAction(delegate
			{
				gameState.gameEventManager.TriggerAfterBattleEnd(type);
			}));
		}
		if (!gameState.IsCharacterTrialGame && characterAI.enemyEncounterType == EnemyEncounterType.Boss && type == FinishBattleEncounterAction.FinishType.Win)
		{
			EnqueueAction(new FinishHintAction(ScriptableObjectSingleton<TextDataPreset>.instance.term_defeat_enemy, halt: true));
		}
	}
}
