using System;
using System.Collections.Generic;
using System.Linq;

public class InGameRecorderManager : InGameManager
{
	[SaveField(null, null)]
	public int battleInitPlayerHealth;

	[SaveField(null, null)]
	public int playerHealthDamageThisBattle;

	[SaveField(null, null)]
	public long battleInitTimeTick;

	[SaveField(null, null)]
	public HashSet<Item> enteredTableItemsThisBattle;

	[SaveField(null, null)]
	public List<GameSkill> castedSkillsThisBattle;

	[SaveField(null, null)]
	public List<Item> castedItemsThisRound;

	[SaveField(null, null)]
	public List<GameSkill> castedSkillsThisRound;

	[SaveField(null, null)]
	public List<Damage> damageLastRound;

	[SaveField(null, null)]
	public List<Damage> damageThisRound;

	[SaveField(null, null)]
	public long roundStartTimeTick;

	[SaveField(null, null)]
	public List<Item> overflowConsumeItemsThisRound;

	[SaveField(null, null)]
	public List<PathLocation> currentLevelVisitedLocations;

	[SaveField(null, null)]
	public List<MatterInfo> inGameMatterRewards;

	[SaveField(null, null)]
	public bool enabledJournalRecord;

	[SaveField(null, null)]
	public List<ICTAsset> discoveredObjectThisGame;

	[SaveField(null, null)]
	public Dictionary<int, int> gemConfigs;

	[SaveField(null, null)]
	public GameJournal gameJournal;

	[SaveField(null, null)]
	public List<EnemyInfoData> encounteredEnemies;

	[SaveField(null, null)]
	public List<InfluenceDef> generatedInfluence;

	[SaveField(null, null)]
	public List<ItemDefinition> generatedItems;

	[SaveField(null, null)]
	public List<SkillDef> generatedSkills;

	[SaveField(null, null)]
	public List<ItemDefinition> lastGenerateItemBonus;

	[SaveField(null, null)]
	public List<EnvironmentInfluenceDef> generatedEnvironmentInfluences;

	[SaveField(null, null)]
	public List<WorldEvent> generatedWorldEventTemplates;

	[SaveField(null, null)]
	public long locationInitTimeTick;

	[SaveField(null, null)]
	public List<Item> playerCastedItemsThisRound;

	[SaveField(null, null)]
	public List<GameRecord.ObjectValueRecord> playerGetMostDamageFromEnemyRecords;

	[SaveField(null, null)]
	public List<GameRecord.ObjectValueRecord> playerSourceDamageSumRecords;

	[SaveField(null, null)]
	public List<GameRecord.BossBlessStatistic> bossBlessStatistics;

	[SaveField(null, null)]
	public int sumGetCoin;

	[SaveField(null, null)]
	public int sumGetCrystal;

	[SaveField(null, null)]
	public int playerRealHealthDamageThisGame;

	[SaveField(null, null)]
	public int dailyChallengeMaximumUsedWillPower;

	public int gemTotalCounts => gemConfigs.Sum((KeyValuePair<int, int> c) => c.Value);

	public List<Item> damagedItemsThisTurn => damageThisRound.FindAll((Damage d) => d.targetItem != null).ConvertAll((Damage t) => t.targetItem);

	public override void Init(GameStateInfo gameState)
	{
		if (gameJournal == null)
		{
			gameJournal = new GameJournal();
		}
		if (castedItemsThisRound == null)
		{
			castedItemsThisRound = new List<Item>();
		}
		if (overflowConsumeItemsThisRound == null)
		{
			overflowConsumeItemsThisRound = new List<Item>();
		}
		if (playerCastedItemsThisRound == null)
		{
			playerCastedItemsThisRound = new List<Item>();
		}
		if (lastGenerateItemBonus == null)
		{
			lastGenerateItemBonus = new List<ItemDefinition>();
		}
		if (currentLevelVisitedLocations == null)
		{
			currentLevelVisitedLocations = new List<PathLocation>();
		}
		if (damageThisRound == null)
		{
			damageThisRound = new List<Damage>();
		}
		if (castedSkillsThisRound == null)
		{
			castedSkillsThisRound = new List<GameSkill>();
		}
		if (enteredTableItemsThisBattle == null)
		{
			enteredTableItemsThisBattle = new HashSet<Item>();
		}
		if (castedSkillsThisBattle == null)
		{
			castedSkillsThisBattle = new List<GameSkill>();
		}
		if (encounteredEnemies == null)
		{
			encounteredEnemies = new List<EnemyInfoData>();
		}
		if (generatedInfluence == null)
		{
			generatedInfluence = new List<InfluenceDef>();
		}
		if (generatedSkills == null)
		{
			generatedSkills = new List<SkillDef>();
		}
		if (generatedItems == null)
		{
			generatedItems = new List<ItemDefinition>();
		}
		if (generatedEnvironmentInfluences == null)
		{
			generatedEnvironmentInfluences = new List<EnvironmentInfluenceDef>();
		}
		if (generatedWorldEventTemplates == null)
		{
			generatedWorldEventTemplates = new List<WorldEvent>();
		}
		if (discoveredObjectThisGame == null)
		{
			discoveredObjectThisGame = new List<ICTAsset>();
		}
		if (gemConfigs == null)
		{
			gemConfigs = new Dictionary<int, int>();
		}
		if (inGameMatterRewards == null)
		{
			inGameMatterRewards = new List<MatterInfo>();
		}
		if (bossBlessStatistics == null)
		{
			bossBlessStatistics = new List<GameRecord.BossBlessStatistic>();
		}
		if (playerSourceDamageSumRecords == null)
		{
			playerSourceDamageSumRecords = new List<GameRecord.ObjectValueRecord>();
		}
		if (playerGetMostDamageFromEnemyRecords == null)
		{
			playerGetMostDamageFromEnemyRecords = new List<GameRecord.ObjectValueRecord>();
		}
		gameState.gameEventManager.onTriggerGameEvent += RecordEvent;
		gameState.applicationState.eventManager.objectDiscoveredEvent.AddListener(RecordDiscoverObjectEvent);
		UpdateDailyChallengeMaximumUsedWillPower();
	}

	public void TrackJournalRecord(GameJournalRecord record)
	{
		if (enabledJournalRecord)
		{
			gameJournal.records.Add(record);
			record.OnTrack(this);
		}
	}

	public GameRecord GenerateGameRecord(bool success, bool finished)
	{
		bool valueOrDefault = base.gameState?.applicationState?.guideManager?.IsAfterIntroFirstGame == true;
		return GenerateGameRecord(success, finished, valueOrDefault);
	}

	public GameRecord GenerateGameRecord(bool success, bool finished, bool isAfterIntroFirstGame)
	{
		if (base.gameState == null)
		{
			return null;
		}
		GameRecord gameRecord = new GameRecord();
		gameRecord.playerId = Bootstrap.instance.CurrentAccountID;
		gameRecord.playerName = base.gameState.applicationState.playerInfoManager.DataName;
		gameRecord.gameGuid = base.gameState.GameGUID;
		gameRecord.success = success;
		gameRecord.finished = finished;
		gameRecord.contract = base.gameState.contractState.selectedContract;
		gameRecord.dailyChallengeContext = base.gameState.newGameConfigData?.dailyChallengeContext;
		gameRecord.progress = base.gameState.dungeonMapManager.currentGlobalProgress;
		gameRecord.stage = base.gameState.contractState.stage;
		gameRecord.gameTime = base.gameState.gameTimer.GetTime();
		gameRecord.gameJournal = gameJournal;
		gameRecord.discoveredObjects = discoveredObjectThisGame;
		gameRecord.inGameMatterRewards = inGameMatterRewards;
		gameRecord.pvpChallengeGameRecord = base.applicationState.pvpModeManager.challengeRecordThisGame;
		gameRecord.finishTime = DateTime.UtcNow;
		gameRecord.cheatTamperCount = ProtectedValueGuard.TamperCount;
		gameRecord.cheatMismatchReadCount = ProtectedValueGuard.MismatchReadCount;
		gameRecord.deck = new List<GameRecord.ItemRecord>();
		foreach (Item allItem in base.gameState.playerState.DeckContainer.GetAllItems())
		{
			GameRecord.ItemRecord item = new GameRecord.ItemRecord(allItem);
			gameRecord.deck.Add(item);
		}
		gameRecord.influences = new List<GameRecord.InfluenceRecord>();
		foreach (Influence influence in base.gameState.playerState.bft.influences)
		{
			gameRecord.influences.Add(new GameRecord.InfluenceRecord(influence));
		}
		gameRecord.skillInfos = new List<GameRecord.SkillRecord>();
		foreach (GameSkill allSkill in base.gameState.playerState.AllSkills)
		{
			GameRecord.SkillRecord item2 = new GameRecord.SkillRecord(allSkill, base.gameState.playerState.ActivatedSkills.Contains(allSkill), base.gameState.playerState.consumedSkills.Contains(allSkill));
			gameRecord.skillInfos.Add(item2);
		}
		gameRecord.gameAchievedAchievements = base.gameState.applicationState.achievementManager.GetAllGameAchivedAchievements().ConvertAll((Achievement a) => a.achievementDef);
		gameRecord.character = base.gameState.playerState.CharacterDef;
		gameRecord.characterUsageMode = base.gameState.newGameConfigData?.characterUsageMode ?? CharacterUsageMode.Owned;
		gameRecord.mostDamageObjects = new List<GameRecord.ObjectValueRecord>();
		if (playerSourceDamageSumRecords != null)
		{
			List<GameRecord.ObjectValueRecord> list = new List<GameRecord.ObjectValueRecord>(playerSourceDamageSumRecords);
			list.Sort((GameRecord.ObjectValueRecord a, GameRecord.ObjectValueRecord b) => b.value.CompareTo(a.value));
			for (int num = 0; num < 3 && num < list.Count; num++)
			{
				if (list[num] != null)
				{
					gameRecord.mostDamageObjects.Add(list[num]);
				}
			}
		}
		if (playerGetMostDamageFromEnemyRecords != null && playerGetMostDamageFromEnemyRecords.Count > 0)
		{
			int maxEnemyDamage = playerGetMostDamageFromEnemyRecords.Max((GameRecord.ObjectValueRecord t) => t.value);
			GameRecord.ObjectValueRecord mostDamageEnemy = playerGetMostDamageFromEnemyRecords.Find((GameRecord.ObjectValueRecord t) => t.value == maxEnemyDamage);
			gameRecord.mostDamageEnemy = mostDamageEnemy;
		}
		gameRecord.expChangeRecords = gameRecord.GetExpChangeRecords(base.gameState.applicationState, isAfterIntroFirstGame);
		gameRecord.exp = GameRecord.ExpChangeRecord.GetExpSum(gameRecord.expChangeRecords);
		if (gameRecord.IsDailyChallenge)
		{
			UpdateDailyChallengeMaximumUsedWillPower();
			gameRecord.dailyChallengeSettlementSnapshot = new DailyChallengeSettlementSnapshot
			{
				completionSeconds = Math.Max(0, (int)new TimeSpan(gameRecord.gameTime).TotalSeconds),
				deckCount = gameRecord.deck.Count,
				blessingCount = gameRecord.influences.Count,
				realHealthDamageTaken = playerRealHealthDamageThisGame,
				finalCoinCount = base.gameState.playerState.coinCount,
				finalCrystalCount = base.gameState.playerState.crystalCount,
				remainingWillPower = base.gameState.playerState.RemainWillPower,
				hasMaximumUsedWillPower = true,
				maximumUsedWillPower = dailyChallengeMaximumUsedWillPower,
				baseExperience = DailyChallengeRecordMetrics.GetBaseExperience(gameRecord.expChangeRecords),
				defeatedStage7FinalBoss = (gameRecord.success && gameRecord.stage >= ScriptableObjectSingleton<DungeonGenerationManager>.instance.finalFinishGameStage)
			};
		}
		gameRecord.enteredNodePoint = base.gameState.classMatterManager.currentEnteredNodePoint;
		gameRecord.gemConfigs = gemConfigs;
		gameRecord.bossBlessStatistics = ((!CanUseBossBlessAutoBalance()) ? new List<GameRecord.BossBlessStatistic>() : (bossBlessStatistics?.ConvertAll((GameRecord.BossBlessStatistic statistic) => statistic?.Clone()) ?? new List<GameRecord.BossBlessStatistic>()));
		gameRecord.commonCurrency = GameRecord.ExpChangeRecord.GetExpSum(gameRecord.expChangeRecords);
		int num2 = FixedContractRouteDef.ResolveCompletionStage(base.gameState.newGameConfigData?.ResolvedFixedContractRoute, ScriptableObjectSingleton<DungeonGenerationManager>.instance.baseFinishGameStage);
		gameRecord.skipUnfinishedNewbieClear = base.gameState.contractState.stage < num2;
		if (gameRecord.success && !gameRecord.skipUnfinishedNewbieClear)
		{
			gameRecord.contractFinishRewards = gameRecord.contract.GetSuccessReward(base.gameState.applicationState, gameRecord.character, gameRecord.AllowsDifficultyProgression);
		}
		return gameRecord;
	}

	public override void Dispose()
	{
		base.gameState.gameEventManager.onTriggerGameEvent -= RecordEvent;
		base.gameState.applicationState.eventManager.objectDiscoveredEvent.RemoveListener(RecordDiscoverObjectEvent);
	}

	public void RecordPlayerRealHealthDamage(int amount)
	{
		checked
		{
			if (amount > 0)
			{
				playerRealHealthDamageThisGame += amount;
			}
		}
	}

	private void RecordEvent(GameEvent eve)
	{
		if (!eve.IsResolveGameEvent())
		{
			GameEvent.Type type = eve.type;
			if (type == GameEvent.Type.AddInfluence || type == GameEvent.Type.RemoveInfluence || type == GameEvent.Type.ChangeWillPower)
			{
				UpdateDailyChallengeMaximumUsedWillPower();
			}
			RecordCastedItemThisRound(eve);
			RecordDamagedThisRound(eve);
			RecordVisitedLocations(eve);
			RecordPlayerDamageSource(eve);
			RecordPlayerGetMostDamage(eve);
			RecordPlayerHealthDamageThisBattle(eve);
			RecordBattleStartInfo(eve);
			RecordCastSkillThisBattle(eve);
			RecordCastSkillThisRound(eve);
			RecordRoundStartTime(eve);
			RecordOverflowItemThisRound(eve);
			RecordLocationInfo(eve);
			RecordSumCoinAndCrystal(eve);
			RecordEnterTableItemsRecord(eve);
		}
	}

	private void UpdateDailyChallengeMaximumUsedWillPower()
	{
		GameStateInfo gameStateInfo = base.gameState;
		if (gameStateInfo != null && gameStateInfo.newGameConfigData?.dailyChallengeContext?.isDailyChallenge == true && base.gameState.playerState != null)
		{
			dailyChallengeMaximumUsedWillPower = DailyChallengeWillPowerMetrics.UpdateMaximum(dailyChallengeMaximumUsedWillPower, base.gameState.playerState.CurrentUsedWillPower);
		}
	}

	private void RecordDiscoverObjectEvent(ICTAsset target)
	{
		RecordDiscoveredObjectForUpload(target);
	}

	public void RecordDiscoveredObjectForUpload(ICTAsset target)
	{
		if (target != null)
		{
			discoveredObjectThisGame.AddUnique(target);
		}
	}

	public void RecordBossBlessExposure(InfluenceDef bless)
	{
		if (CanUseBossBlessAutoBalance() && !(bless == null) && !string.IsNullOrEmpty(bless.FullID))
		{
			GetOrCreateBossBlessStatistic(bless.FullID).ExposureCount++;
		}
	}

	public void RecordBossBlessPick(InfluenceDef bless)
	{
		if (CanUseBossBlessAutoBalance() && !(bless == null) && !string.IsNullOrEmpty(bless.FullID))
		{
			GetOrCreateBossBlessStatistic(bless.FullID).PickCount++;
		}
	}

	private GameRecord.BossBlessStatistic GetOrCreateBossBlessStatistic(string blessId)
	{
		if (bossBlessStatistics == null)
		{
			bossBlessStatistics = new List<GameRecord.BossBlessStatistic>();
		}
		GameRecord.BossBlessStatistic bossBlessStatistic = bossBlessStatistics.Find((GameRecord.BossBlessStatistic item) => item != null && item.BlessId == blessId);
		if (bossBlessStatistic != null)
		{
			return bossBlessStatistic;
		}
		bossBlessStatistic = new GameRecord.BossBlessStatistic(blessId);
		bossBlessStatistics.Add(bossBlessStatistic);
		return bossBlessStatistic;
	}

	private bool CanUseBossBlessAutoBalance()
	{
		return (base.gameState.contractState?.selectedContract?.contractDef?.contractRule)?.CanUseBossBlessAutoBalance(base.gameState) ?? true;
	}

	private void RecordEnterTableItemsRecord(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.AddItem && eve.DestinationContainer.type == ItemContainer.ContainerType.Table)
		{
			enteredTableItemsThisBattle.Add(eve.TargetItem);
		}
		if (eve.type == GameEvent.Type.BeforeBattleEnd)
		{
			enteredTableItemsThisBattle.Clear();
		}
	}

	private void RecordCastedItemThisRound(GameEvent eve)
	{
		GameEvent.Type type = eve.type;
		if (type == GameEvent.Type.BeforeBattleEnd || type == GameEvent.Type.PlayerTurnStart)
		{
			castedItemsThisRound.Clear();
			playerCastedItemsThisRound.Clear();
		}
		else if (eve.type == GameEvent.Type.BeforeSpellCast)
		{
			Item spellItem = eve.spellInfo.spellItem;
			castedItemsThisRound.Add(spellItem);
			BattleFieldTransform ownerBft = spellItem.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				playerCastedItemsThisRound.Add(spellItem);
			}
		}
	}

	private void RecordDamagedThisRound(GameEvent eve)
	{
		GameEvent.Type type = eve.type;
		if (type == GameEvent.Type.BeforeBattleEnd || type == GameEvent.Type.PlayerTurnStart)
		{
			damageLastRound = damageThisRound;
			damageThisRound = new List<Damage>();
		}
		else if (eve.type == GameEvent.Type.AfterDamage)
		{
			damageThisRound.Add(eve.damage);
		}
	}

	private void RecordVisitedLocations(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.EnterLocation)
		{
			currentLevelVisitedLocations.Add(eve.TargetLocation);
		}
		if (eve.type == GameEvent.Type.BeforeEnterNextStage)
		{
			currentLevelVisitedLocations.Clear();
		}
	}

	private void RecordPlayerDamageSource(GameEvent eve)
	{
		if (eve.type != GameEvent.Type.AfterDamage || eve.damage.source == null || !(eve.damage?.source is IBftOwnable { OwnerBft: { isPlayer: not false } }))
		{
			return;
		}
		ICTAsset sourcePersistantObject = GetPersistantObject(eve.damage.source);
		if (sourcePersistantObject != null)
		{
			playerSourceDamageSumRecords.FindOrAdd((GameRecord.ObjectValueRecord t) => t.target == sourcePersistantObject, () => new GameRecord.ObjectValueRecord(sourcePersistantObject, 0)).value += eve.damage.subCounter;
		}
	}

	private void RecordPlayerGetMostDamage(GameEvent eve)
	{
		if (eve.type != GameEvent.Type.AfterDamage)
		{
			return;
		}
		BattleFieldTransform targetBFT = eve.damage.targetBFT;
		if (targetBFT == null || !targetBFT.isPlayer)
		{
			return;
		}
		targetBFT = eve.damage.SourceOwnerBft;
		if (targetBFT != null && !targetBFT.isPlayer && targetBFT.characterAI != null)
		{
			playerGetMostDamageFromEnemyRecords.FindOrAdd((GameRecord.ObjectValueRecord t) => t.target == GetPersistantObject(eve.damage.SourceOwnerBft), () => new GameRecord.ObjectValueRecord(GetPersistantObject(eve.damage.SourceOwnerBft), 0)).value += eve.damage.subCounter;
		}
	}

	private void RecordPlayerHealthDamageThisBattle(GameEvent eve)
	{
		GameEvent.Type type = eve.type;
		if (type == GameEvent.Type.BeforeBattleEnd || type == GameEvent.Type.BattleStart)
		{
			playerHealthDamageThisBattle = 0;
		}
		else
		{
			if (eve.type != GameEvent.Type.AfterDamage)
			{
				return;
			}
			Damage damage = eve.damage;
			if (damage != null && damage.isHealthDamage)
			{
				BattleFieldTransform targetBFT = damage.targetBFT;
				if (targetBFT != null && targetBFT.isPlayer)
				{
					playerHealthDamageThisBattle += eve.damage.subCounter;
				}
			}
		}
	}

	private void RecordBattleStartInfo(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.BattleStart)
		{
			battleInitPlayerHealth = base.gameState.playerState.bft.health;
			battleInitTimeTick = base.gameState.gameTimer.GetTime();
			encounteredEnemies.Add(base.gameState.currentEncounter.enemyAI.component.GetInfoData(base.gameState.applicationState));
		}
	}

	private void RecordLocationInfo(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.EnterLocation)
		{
			locationInitTimeTick = base.gameState.gameTimer.GetTime();
		}
	}

	private void RecordSumCoinAndCrystal(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.ChangeCoin && eve.deltaValue > 0)
		{
			sumGetCoin += eve.deltaValue;
		}
		if (eve.type == GameEvent.Type.ChangeCrystal && eve.deltaValue > 0)
		{
			sumGetCrystal += eve.deltaValue;
		}
	}

	private void RecordCastSkillThisBattle(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.BeforeBattleEnd || eve.type == GameEvent.Type.BattleStart)
		{
			castedSkillsThisBattle.Clear();
		}
		else if (eve.type == GameEvent.Type.AfterSkillCast)
		{
			castedSkillsThisBattle.Add(eve.SourceSkill);
		}
	}

	private void RecordCastSkillThisRound(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.BeforeBattleEnd || eve.type == GameEvent.Type.PlayerTurnStart)
		{
			castedSkillsThisRound.Clear();
		}
		else if (eve.type == GameEvent.Type.AfterSkillCast)
		{
			castedSkillsThisRound.Add(eve.SourceSkill);
		}
	}

	private void RecordRoundStartTime(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.BattleStart || eve.type == GameEvent.Type.PlayerTurnStart)
		{
			roundStartTimeTick = base.gameState.gameTimer.GetTime();
		}
	}

	private void RecordOverflowItemThisRound(GameEvent eve)
	{
		if (eve.type == GameEvent.Type.BeforeConsume && eve.consumeType == ConsumeItemAction.ConsumeType.Overflow)
		{
			BattleFieldTransform ownerBft = eve.TargetItem.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				overflowConsumeItemsThisRound.Add(eve.TargetItem);
			}
		}
		if (eve.type == GameEvent.Type.BattleStart || eve.type == GameEvent.Type.PlayerTurnStart)
		{
			overflowConsumeItemsThisRound.Clear();
		}
	}

	private ICTAsset GetPersistantObject(object obj)
	{
		if (!(obj is Item item))
		{
			if (!(obj is GameSkill gameSkill))
			{
				if (!(obj is Influence influence))
				{
					if (!(obj is Enchant enchant))
					{
						if (!(obj is Rune rune))
						{
							if (!(obj is SkillWord skillWord))
							{
								if (obj is BattleFieldTransform battleFieldTransform)
								{
									return (battleFieldTransform.characterAI?.component != null) ? battleFieldTransform.characterAI.component.GetInfoData(base.gameState.applicationState) : null;
								}
								return null;
							}
							return skillWord.def;
						}
						return rune.def;
					}
					return enchant.def;
				}
				return influence.def;
			}
			return gameSkill.skillDef;
		}
		return item.GetDef();
	}
}
