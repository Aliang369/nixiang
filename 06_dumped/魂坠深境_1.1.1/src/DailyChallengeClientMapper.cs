using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;

public static class DailyChallengeClientMapper
{
	public static DailyChallengePageState ToPageState(DailyChallengeConfigDTO config, DailyChallengePlayerStatusDTO status, IEnumerable<DailyChallengeRankEntryDTO> ranking, ApplicationStateInfo applicationState, DailyChallengeClientConfig clientConfig, DateTime utcNow)
	{
		if (config == null)
		{
			throw new ArgumentNullException("config");
		}
		int num = Math.Max(0, config.WeeklyClaimLimit);
		int num2 = Math.Clamp(status?.WeeklyRemainingClaims ?? num, 0, num);
		int count = Math.Max(1, clientConfig?.leaderboardRowLimit ?? 10);
		DailyChallengePageState dailyChallengePageState = new DailyChallengePageState
		{
			challengeId = config.ChallengeId,
			gameplayContract = ResolveAsset<ContractDef>(applicationState, config.RuleContractId),
			scoreCompetitionContract = clientConfig?.ResolveScoreContract(config.ScoreContractId)?.ToPresentation(),
			rotationResetUtc = NormalizeUtc(config.EndTime),
			settlementUtc = NormalizeUtc(config.EndTime),
			weeklyResetUtc = DailyChallengeClock.GetNextWeeklyResetUtc(utcNow, clientConfig?.utcOffsetHours ?? 8, clientConfig?.dailyResetHour ?? 4, clientConfig?.weeklyResetDay ?? DayOfWeek.Monday),
			rewardClaimedThisWeek = num - num2,
			rewardLimitPerWeek = num,
			claimedToday = (status?.HasChallenged ?? false),
			leaderboardRewardText = string.Empty,
			completionRewards = (config.CompletionRewards ?? new List<DailyChallengeRewardDisplay>()).Select((DailyChallengeRewardDisplay reward) => ToReward(reward, applicationState)).ToList()
		};
		dailyChallengePageState.topRanking = (from entry in (ranking ?? Enumerable.Empty<DailyChallengeRankEntryDTO>()).Take(count)
			select ToRankingRow(entry, applicationState)).ToList();
		if (status != null && status.HasChallenged && status.BestEntry != null)
		{
			dailyChallengePageState.playerRanking = ToRankingRow(status.BestEntry, applicationState);
		}
		return dailyChallengePageState;
	}

	public static DailyChallengeSettlement ToSettlement(ChillyRoom.Services.ContainerOfSoul.DailyChallengeSettlement source, ApplicationStateInfo applicationState = null, DailyChallengeClientConfig clientConfig = null, DailyChallengeScoreContractPresentation fallbackContract = null)
	{
		if (source == null)
		{
			return null;
		}
		DailyChallengeSettlement dailyChallengeSettlement = new DailyChallengeSettlement
		{
			challengeId = source.ChallengeId,
			status = ParseStatus(source.Status),
			reasonCode = source.ReasonCode,
			statusText = null,
			challengeCompleted = source.ChallengeCompleted,
			hasChallenged = source.HasChallenged,
			scoreContractId = source.ScoreContractId,
			scoreContract = (clientConfig?.ResolveScoreContract(source.ScoreContractId)?.ToPresentation() ?? fallbackContract),
			scoreMetrics = ToScoreMetrics(source.ScoreMetrics),
			baseExperience = source.BaseExperience,
			competitionScore = source.CompetitionScore,
			preMultiplierScore = source.PreMultiplierScore,
			finalBossMultiplierNumerator = (int)Math.Round(source.BossMultiplier * 2.0),
			difficultyMultiplierTenths = (int)Math.Round(source.DifficultyMultiplier * 10.0),
			finalScore = source.FinalScore,
			completionRewardGranted = source.CompletionRewardGranted
		};
		if (applicationState == null || source.CompletionRewardInfos == null)
		{
			return dailyChallengeSettlement;
		}
		NetworkConvertContext context = new NetworkConvertContext(applicationState);
		foreach (ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo completionRewardInfo in source.CompletionRewardInfos)
		{
			GlobalRewardInfo globalRewardInfo = completionRewardInfo?.ToClient(context);
			if (globalRewardInfo != null)
			{
				dailyChallengeSettlement.completionRewardInfos.Add(globalRewardInfo);
			}
		}
		return dailyChallengeSettlement;
	}

	public static GameRecord ToRecordDetail(DailyChallengeRecordDetailDTO source, ApplicationStateInfo applicationState, DailyChallengeClientConfig clientConfig)
	{
		if (source?.Record == null)
		{
			return null;
		}
		GameRecord obj = ((applicationState == null) ? ToMinimalRecord(source.Record) : source.Record.ToClient(new NetworkConvertContext(applicationState)));
		DailyChallengeScoreContractPresentation dailyChallengeScoreContractPresentation = clientConfig?.ResolveScoreContract(source.Settlement?.ScoreContractId)?.ToPresentation();
		obj.dailyChallengeSettlement = ToSettlement(source.Settlement, applicationState, clientConfig, dailyChallengeScoreContractPresentation);
		obj.dailyChallengeContext = new DailyChallengeRunContext
		{
			challengeId = source.Settlement?.ChallengeId,
			isDailyChallenge = true,
			scoreCompetitionContract = dailyChallengeScoreContractPresentation
		};
		return obj;
	}

	private static DailyChallengeScoreMetrics ToScoreMetrics(ChillyRoom.Services.ContainerOfSoul.DailyChallengeScoreMetrics source)
	{
		if (source == null)
		{
			return new DailyChallengeScoreMetrics();
		}
		return new DailyChallengeScoreMetrics
		{
			durationSeconds = source.DurationSeconds,
			deckCount = source.DeckCount,
			blessingCount = source.BlessingCount,
			historicalRealHealthDamage = source.HistoricalRealHealthDamage,
			finalGold = source.FinalGold,
			finalSoulCrystal = source.FinalSoulCrystal,
			finalSanity = source.FinalSanity
		};
	}

	private static GameRecord ToMinimalRecord(ChillyRoom.Services.ContainerOfSoul.GameRecord source)
	{
		return new GameRecord
		{
			gameGuid = source.GameGuid,
			playerId = source.PlayerId.ToString(),
			playerName = source.Nickname,
			success = source.Success,
			progress = source.FinalProgress,
			stage = source.FinalStage,
			gameTime = source.Duration.Ticks,
			exp = source.ExpBonus,
			rankScore = source.RankScore,
			finishTime = source.FinishDateTime,
			deck = new List<GameRecord.ItemRecord>(),
			influences = new List<GameRecord.InfluenceRecord>(),
			skillInfos = new List<GameRecord.SkillRecord>(),
			expChangeRecords = new List<GameRecord.ExpChangeRecord>(),
			gameJournal = new GameJournal(),
			discoveredObjects = new List<ICTAsset>()
		};
	}

	private static DailyChallengeRankingRow ToRankingRow(DailyChallengeRankEntryDTO source, ApplicationStateInfo applicationState)
	{
		PlayerCharacterDef playerCharacterDef = ResolveAsset<PlayerCharacterDef>(applicationState, source.CharacterId);
		DifficultyLevel difficulty = (DifficultyLevel)source.Difficulty;
		return new DailyChallengeRankingRow
		{
			rank = source.Rank,
			playerId = source.PlayerId,
			recordId = source.BestGameRecordId,
			playerName = source.PlayerName,
			characterName = (playerCharacterDef?.characterName ?? source.CharacterId),
			difficultyName = (applicationState?.difficultyManager?.GetDifficultyInfo(difficulty)?.difficultyName ?? difficulty.ToString()),
			score = source.Score,
			avatar = playerCharacterDef?.headIcon,
			hasRecord = !string.IsNullOrEmpty(source.BestGameRecordId)
		};
	}

	private static DailyChallengeRewardPresentation ToReward(DailyChallengeRewardDisplay source, ApplicationStateInfo applicationState)
	{
		return new DailyChallengeRewardPresentation
		{
			type = source.Type,
			assetId = source.Id,
			count = Math.Max(0, source.Count),
			asset = ResolveAsset<ICTAsset>(applicationState, source.Id)
		};
	}

	private static T ResolveAsset<T>(ApplicationStateInfo applicationState, string id) where T : class, ICTAsset
	{
		if (!(applicationState?.assetCatalog == null) && !string.IsNullOrEmpty(id))
		{
			return applicationState.assetCatalog.FullIdToCTAsset<T>(id);
		}
		return null;
	}

	private static DailyChallengeSettlementStatus ParseStatus(string status)
	{
		if (!Enum.TryParse<DailyChallengeSettlementStatus>(status, ignoreCase: true, out var result))
		{
			return DailyChallengeSettlementStatus.NotCompleted;
		}
		return result;
	}

	private static DateTime NormalizeUtc(DateTime value)
	{
		return value.Kind switch
		{
			DateTimeKind.Utc => value, 
			DateTimeKind.Local => value.ToUniversalTime(), 
			_ => DateTime.SpecifyKind(value, DateTimeKind.Utc), 
		};
	}
}
