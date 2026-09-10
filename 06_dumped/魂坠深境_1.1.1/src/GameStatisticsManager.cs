using System;
using System.Collections.Generic;

public class GameStatisticsManager : GlobalGameManager
{
	public class GameModeStatisticsInfo
	{
		[SaveField(null, null)]
		public int gameCount;

		[SaveField(null, null)]
		public int winGameCount;
	}

	public class PlayerClassStatisticsInfo
	{
		[SaveField(null, null)]
		public long castItemCount;

		[SaveField(null, null)]
		public long coinGetCount;

		[SaveField(null, null)]
		public long crystalGetCount;

		[SaveField(null, null)]
		public long damageCount;

		[SaveField(null, null)]
		public int defeatEnemyCount;

		[SaveField(null, null)]
		public int encounterEventCount;

		[SaveField(null, null)]
		public int gameCount;

		[SaveField(null, null)]
		public int winGameCount;

		[SaveField(null, null)]
		public DifficultyLevel highestFinishedDifficulty = DifficultyLevel.Not;

		[SaveField(null, null)]
		public int currentWinStreak;

		[SaveField(null, null)]
		public int maxWinStreak;

		[SaveField(null, null)]
		public Dictionary<int, GameModeStatisticsInfo> difficultyStatistics = new Dictionary<int, GameModeStatisticsInfo>();
	}

	[SaveField(null, null)]
	public long castItemCount;

	[SaveField(null, null)]
	public long coinGetCount;

	[SaveField(null, null)]
	public long crystalGetCount;

	[SaveField(null, null)]
	public long damageCount;

	[SaveField(null, null)]
	public int defeatEnemyCount;

	[SaveField(null, null)]
	public int encounterEventCount;

	[SaveField(null, null)]
	public int gameCount;

	[SaveField(null, null)]
	public int winGameCount;

	[SaveField(null, null)]
	public DifficultyLevel highestFinishedDifficulty = DifficultyLevel.Not;

	[SaveField(null, null)]
	public int currentWinStreak;

	[SaveField(null, null)]
	public int maxWinStreak;

	[SaveField(null, null)]
	public Dictionary<int, GameModeStatisticsInfo> difficultyStatistics = new Dictionary<int, GameModeStatisticsInfo>();

	[SaveField(null, null)]
	public Dictionary<string, PlayerClassStatisticsInfo> classStatistics = new Dictionary<string, PlayerClassStatisticsInfo>();

	[SaveField(null, null)]
	public int startNewGameTimesSum;

	public override void Init(ApplicationStateInfo applicationState)
	{
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeGameStart, _OnGameStart);
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeGameEnd, _OnGameFinish);
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, _OnBattleEnd);
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.EnterLocation, _OnEnterLocation);
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterSpellCast, _OnItemCast);
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterDamage, _OnAfterDamage);
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.ChangeCoin, _OnGetCoin);
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.ChangeCrystal, _OnGetCrystal);
	}

	public override void Dispose()
	{
		base.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeGameStart, _OnGameStart);
		base.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeGameEnd, _OnGameFinish);
		base.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.BeforeBattleEnd, _OnBattleEnd);
		base.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.EnterLocation, _OnEnterLocation);
		base.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterSpellCast, _OnItemCast);
		base.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterDamage, _OnAfterDamage);
		base.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.ChangeCoin, _OnGetCoin);
		base.applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.ChangeCrystal, _OnGetCrystal);
	}

	private void _OnGameStart(GameEvent.Type type, GameEvent data)
	{
		if (ShouldRecordPersistentStatistics(data?.gameState?.newGameConfigData))
		{
			startNewGameTimesSum++;
		}
	}

	private void _OnGameFinish(GameEvent.Type type, GameEvent data)
	{
		if (!ShouldRecordPersistentStatistics(data?.gameState?.newGameConfigData))
		{
			return;
		}
		gameCount++;
		DifficultyLevel difficulty = GetDifficulty(data);
		if (difficultyStatistics == null)
		{
			difficultyStatistics = new Dictionary<int, GameModeStatisticsInfo>();
		}
		AddGameModeResult(difficultyStatistics, difficulty, data.succesGame);
		ApplyFinishResult(ref highestFinishedDifficulty, ref currentWinStreak, ref maxWinStreak, difficulty, data.succesGame);
		PlayerClassStatisticsInfo currentClassStatistics = GetCurrentClassStatistics(data);
		if (currentClassStatistics != null)
		{
			currentClassStatistics.gameCount++;
			PlayerClassStatisticsInfo playerClassStatisticsInfo = currentClassStatistics;
			if (playerClassStatisticsInfo.difficultyStatistics == null)
			{
				playerClassStatisticsInfo.difficultyStatistics = new Dictionary<int, GameModeStatisticsInfo>();
			}
			AddGameModeResult(currentClassStatistics.difficultyStatistics, difficulty, data.succesGame);
			ApplyFinishResult(ref currentClassStatistics.highestFinishedDifficulty, ref currentClassStatistics.currentWinStreak, ref currentClassStatistics.maxWinStreak, difficulty, data.succesGame);
		}
		if (data.succesGame)
		{
			winGameCount++;
			if (currentClassStatistics != null)
			{
				currentClassStatistics.winGameCount++;
			}
		}
	}

	public static bool ShouldRecordPersistentStatistics(NewGameConfigData config)
	{
		if (config == null)
		{
			return true;
		}
		return config.dailyChallengeContext?.isDailyChallenge != true;
	}

	private void _OnAfterDamage(GameEvent.Type type, GameEvent data)
	{
		if (type == GameEvent.Type.AfterDamage && data.damage != null && data.SourceOwner == data.gameState.playerState.bft)
		{
			damageCount += data.damage.subCounter;
			AddClassStatistic(data, delegate(PlayerClassStatisticsInfo info)
			{
				info.damageCount += data.damage.subCounter;
			});
		}
	}

	private void _OnGetCoin(GameEvent.Type type, GameEvent data)
	{
		if (type == GameEvent.Type.ChangeCoin && data.deltaValue > 0)
		{
			coinGetCount += data.deltaValue;
			AddClassStatistic(data, delegate(PlayerClassStatisticsInfo info)
			{
				info.coinGetCount += data.deltaValue;
			});
		}
	}

	private void _OnGetCrystal(GameEvent.Type type, GameEvent data)
	{
		if (type == GameEvent.Type.ChangeCrystal && data.deltaValue > 0)
		{
			crystalGetCount += data.deltaValue;
			AddClassStatistic(data, delegate(PlayerClassStatisticsInfo info)
			{
				info.crystalGetCount += data.deltaValue;
			});
		}
	}

	private void _OnItemCast(GameEvent.Type type, GameEvent data)
	{
		if (type != GameEvent.Type.AfterSpellCast)
		{
			return;
		}
		SpellCastInfo spellInfo = data.spellInfo;
		if (spellInfo != null && spellInfo.casterType == SpellCastInfo.CasterType.Player)
		{
			castItemCount++;
			AddClassStatistic(data, delegate(PlayerClassStatisticsInfo info)
			{
				info.castItemCount++;
			});
		}
	}

	private void _OnBattleEnd(GameEvent.Type type, GameEvent data)
	{
		if (data.battleFinishType == FinishBattleEncounterAction.FinishType.Win)
		{
			defeatEnemyCount++;
			AddClassStatistic(data, delegate(PlayerClassStatisticsInfo info)
			{
				info.defeatEnemyCount++;
			});
		}
	}

	private void _OnEnterLocation(GameEvent.Type type, GameEvent data)
	{
		if (data.TargetLocation.type == PathLocation.LocationType.Event)
		{
			encounterEventCount++;
			AddClassStatistic(data, delegate(PlayerClassStatisticsInfo info)
			{
				info.encounterEventCount++;
			});
		}
	}

	public PlayerClassStatisticsInfo GetClassStatistics(CharacterClassDef characterClass)
	{
		if (characterClass == null || classStatistics == null)
		{
			return null;
		}
		classStatistics.TryGetValue(characterClass.FullID, out var value);
		return value;
	}

	private PlayerClassStatisticsInfo GetOrCreateClassStatistics(CharacterClassDef characterClass)
	{
		if (characterClass == null)
		{
			return null;
		}
		if (classStatistics == null)
		{
			classStatistics = new Dictionary<string, PlayerClassStatisticsInfo>();
		}
		if (classStatistics.TryGetValue(characterClass.FullID, out var value))
		{
			return value;
		}
		value = new PlayerClassStatisticsInfo();
		classStatistics.Add(characterClass.FullID, value);
		return value;
	}

	private PlayerClassStatisticsInfo GetCurrentClassStatistics(GameEvent data)
	{
		return GetOrCreateClassStatistics(GetCharacterClass(data?.gameState));
	}

	private CharacterClassDef GetCharacterClass(GameStateInfo gameState)
	{
		object obj = gameState?.playerState?.character?.def?.attachClass;
		if (obj == null)
		{
			ApplicationStateInfo applicationStateInfo = base.applicationState;
			if (applicationStateInfo == null)
			{
				return null;
			}
			CharacterManager characterManager = applicationStateInfo.characterManager;
			if (characterManager == null)
			{
				return null;
			}
			PlayerCharacterState currentCharacter = characterManager.currentCharacter;
			if (currentCharacter == null)
			{
				return null;
			}
			PlayerCharacterDef def = currentCharacter.def;
			if ((object)def == null)
			{
				return null;
			}
			obj = def.attachClass;
		}
		return (CharacterClassDef)obj;
	}

	private DifficultyLevel GetDifficulty(GameEvent data)
	{
		return data?.gameState?.contractState?.selectedContract?.difficultyLevel ?? DifficultyLevel.Not;
	}

	private void AddClassStatistic(GameEvent data, Action<PlayerClassStatisticsInfo> addStatistic)
	{
		PlayerClassStatisticsInfo currentClassStatistics = GetCurrentClassStatistics(data);
		if (currentClassStatistics != null)
		{
			addStatistic(currentClassStatistics);
		}
	}

	private static GameModeStatisticsInfo GetOrCreateGameModeStatistics(Dictionary<int, GameModeStatisticsInfo> statistics, DifficultyLevel difficulty)
	{
		if (statistics == null)
		{
			statistics = new Dictionary<int, GameModeStatisticsInfo>();
		}
		if (statistics.TryGetValue((int)difficulty, out var value))
		{
			return value;
		}
		value = new GameModeStatisticsInfo();
		statistics.Add((int)difficulty, value);
		return value;
	}

	private static void AddGameModeResult(Dictionary<int, GameModeStatisticsInfo> statistics, DifficultyLevel difficulty, bool success)
	{
		GameModeStatisticsInfo orCreateGameModeStatistics = GetOrCreateGameModeStatistics(statistics, difficulty);
		orCreateGameModeStatistics.gameCount++;
		if (success)
		{
			orCreateGameModeStatistics.winGameCount++;
		}
	}

	private static void ApplyFinishResult(ref DifficultyLevel highestDifficulty, ref int currentStreak, ref int maxStreak, DifficultyLevel difficulty, bool success)
	{
		if (!success)
		{
			currentStreak = 0;
			return;
		}
		if (difficulty > highestDifficulty)
		{
			highestDifficulty = difficulty;
		}
		currentStreak++;
		if (currentStreak > maxStreak)
		{
			maxStreak = currentStreak;
		}
	}
}
