using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public sealed class DailyChallengeMockDataProvider : IDailyChallengeAsyncDataProvider, IDailyChallengeDataProvider
{
	private static readonly string[] MockPlayerNames = new string[120]
	{
		"暮雨", "长风", "青禾", "星野", "白露", "云归", "南枝", "北辰", "朝雾", "晚舟",
		"清川", "远山", "流萤", "秋水", "春信", "夏木", "冬青", "月白", "松间", "竹影",
		"溪云", "海棠", "落霞", "晴岚", "微光", "夜航", "拾星", "寻梦", "听潮", "望舒",
		"飞鸿", "归雁", "苍梧", "霁月", "烟萝", "逐风", "栖霞", "锦书", "墨池", "玄羽",
		"山岚", "江雪", "临渊", "观澜", "怀瑾", "握瑜", "若木", "扶桑", "青梧", "丹枫",
		"银沙", "金穗", "玉衡", "天璇", "摇光", "开阳", "新月", "满月", "破晓", "黄昏",
		"云雀", "夜莺", "青鸾", "白鹭", "赤羽", "玄鹤", "雪狐", "山猫", "林鹿", "海燕",
		"浪花", "潮声", "岩松", "原野", "风铃", "雨燕", "纸鸢", "木槿", "芙蕖", "兰舟",
		"枫桥", "柳岸", "桃源", "杏雨", "梨云", "荷风", "桂影", "梅雪", "茶烟", "琴心",
		"剑意", "书灯", "棋声", "画境", "诗酒", "清欢", "半夏", "千帆", "一苇", "三秋",
		"九歌", "百川", "苍穹", "大地", "星火", "霜华", "雷鸣", "岚影", "烬余", "晨曦",
		"暮云", "归途", "初心", "知行", "安然", "从容", "行歌", "静水", "无尘", "有光"
	};

	private readonly ApplicationStateInfo applicationState;

	private readonly DailyChallengeClientConfig config;

	private readonly DailyChallengeMockScenario scenario;

	private readonly IDailyChallengeClock clock;

	private readonly Dictionary<string, DailyChallengeSettlementMockScenario> settlementScenarios = new Dictionary<string, DailyChallengeSettlementMockScenario>();

	private ScoreCompetitionContractDef currentMockScoreContract;

	private DailyChallengePageState current;

	public DailyChallengeMockDataProvider(ApplicationStateInfo applicationState, DailyChallengeClientConfig config, DailyChallengeMockScenario scenario, IDailyChallengeClock clock = null)
	{
		this.applicationState = applicationState;
		if (!(config != null))
		{
			throw new ArgumentNullException("config");
		}
		this.config = config;
		this.scenario = scenario;
		this.clock = clock ?? new SystemDailyChallengeClock();
		Refresh();
	}

	public DailyChallengePageState GetCurrentState()
	{
		return current;
	}

	public Task<DailyChallengePageState> RefreshAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		Refresh();
		return Task.FromResult(current);
	}

	public Task<GameRecord> GetRecordDetailAsync(long playerId, string recordId, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.FromResult(GetRecordDetail(recordId));
	}

	public void Refresh()
	{
		settlementScenarios.Clear();
		DateTime utcNow = clock.UtcNow;
		DateTime dateTime = DailyChallengeClock.GetNextDailyResetUtc(utcNow, config.utcOffsetHours, config.dailyResetHour);
		if (scenario == DailyChallengeMockScenario.BeforeDailyReset)
		{
			dateTime = utcNow.AddMinutes(5.0);
		}
		else if (scenario == DailyChallengeMockScenario.AfterDailyReset)
		{
			dateTime = utcNow.AddMinutes(-5.0);
		}
		currentMockScoreContract = config.GetMockScoreContract();
		ContractDef mockGameplayContract = config.GetMockGameplayContract();
		current = new DailyChallengePageState
		{
			challengeId = config.mockChallengeId,
			gameplayContract = mockGameplayContract,
			scoreCompetitionContract = currentMockScoreContract?.ToPresentation(),
			rotationResetUtc = dateTime,
			settlementUtc = dateTime,
			weeklyResetUtc = DailyChallengeClock.GetNextWeeklyResetUtc(utcNow, config.utcOffsetHours, config.dailyResetHour, config.weeklyResetDay),
			rewardClaimedThisWeek = ((scenario == DailyChallengeMockScenario.NoRewardAttempts) ? config.rewardLimitPerWeek : ((scenario == DailyChallengeMockScenario.ShortRanking) ? Mathf.Max(0, config.rewardLimitPerWeek - 1) : 0)),
			rewardLimitPerWeek = config.rewardLimitPerWeek,
			claimedToday = (scenario == DailyChallengeMockScenario.NoRewardAttempts),
			leaderboardRewardText = config.GetLeaderboardRewardText(),
			completionRewards = ((config.completionRewardMatter == null) ? new List<DailyChallengeRewardPresentation>() : new List<DailyChallengeRewardPresentation>
			{
				new DailyChallengeRewardPresentation
				{
					type = "matter",
					assetId = config.completionRewardMatter.FullID,
					count = config.completionRewardAmount,
					asset = config.completionRewardMatter
				}
			})
		};
		if (scenario == DailyChallengeMockScenario.SettlementShowcase)
		{
			BuildSettlementShowcase(currentMockScoreContract);
		}
		else
		{
			BuildStandardRanking();
		}
		if (scenario != DailyChallengeMockScenario.NoPlayerRecord)
		{
			if (scenario == DailyChallengeMockScenario.SettlementShowcase)
			{
				DailyChallengeSettlementMockScenario value = DailyChallengeSettlementMockScenario.ClearedStage6ThenDiedStage7;
				DailyChallengeSettlementMockCase dailyChallengeSettlementMockCase = DailyChallengeSettlementMockFactory.Create(value, currentMockScoreContract.rule, config.defaultDifficulty);
				DailyChallengeMockScoreResult dailyChallengeMockScoreResult = DailyChallengeMockScoreCalculator.Calculate(currentMockScoreContract.rule, dailyChallengeSettlementMockCase.snapshot, dailyChallengeSettlementMockCase.difficulty, dailyChallengeSettlementMockCase.challengeCompleted);
				settlementScenarios["mock-settlement-player"] = value;
				current.playerRanking = new DailyChallengeRankingRow
				{
					rank = 37,
					recordId = "mock-settlement-player",
					playerName = (applicationState?.playerInfoManager?.DisplayName ?? string.Empty),
					characterName = "小骑士",
					difficultyName = GetDifficultyName(dailyChallengeSettlementMockCase.difficulty),
					score = dailyChallengeMockScoreResult.finalScore,
					avatar = FindCharacter("小骑士")?.headIcon,
					hasRecord = true
				};
			}
			else
			{
				current.playerRanking = ((scenario == DailyChallengeMockScenario.PlayerInTopTen) ? current.topRanking[0] : new DailyChallengeRankingRow
				{
					rank = 37,
					recordId = "mock-record-player",
					playerName = (applicationState?.playerInfoManager?.DisplayName ?? string.Empty),
					characterName = "小骑士",
					difficultyName = "普通",
					score = 12480,
					avatar = FindCharacter("小骑士")?.headIcon,
					hasRecord = true
				});
			}
		}
	}

	public GameRecord GetRecordDetail(string recordId)
	{
		if (string.IsNullOrEmpty(recordId) || applicationState == null)
		{
			return null;
		}
		DailyChallengeRankingRow dailyChallengeRankingRow = current.topRanking.Find((DailyChallengeRankingRow row) => row.recordId == recordId);
		if (dailyChallengeRankingRow == null && current.playerRanking?.recordId == recordId)
		{
			dailyChallengeRankingRow = current.playerRanking;
		}
		if (dailyChallengeRankingRow == null || currentMockScoreContract == null)
		{
			return null;
		}
		DailyChallengeSettlementMockScenario dailyChallengeSettlementMockScenario = (settlementScenarios.TryGetValue(recordId, out var value) ? value : DailyChallengeSettlementMockScenario.DefeatedStage7FinalBoss);
		DailyChallengeSettlementMockCase dailyChallengeSettlementMockCase = DailyChallengeSettlementMockFactory.Create(dailyChallengeSettlementMockScenario, currentMockScoreContract.rule, config.defaultDifficulty);
		GameRecord gameRecord = new GameRecord
		{
			playerId = "mock-player-id",
			playerName = dailyChallengeRankingRow.playerName,
			gameGuid = recordId,
			character = ((dailyChallengeRankingRow.characterName == "和音奏者") ? FindCharacter("和音奏者") : FindCharacter("小骑士")),
			contract = current.gameplayContract?.GenerateContract(applicationState.assetCatalog, dailyChallengeSettlementMockCase.difficulty),
			gameTime = TimeSpan.FromSeconds(dailyChallengeSettlementMockCase.snapshot.completionSeconds).Ticks,
			finishTime = clock.UtcNow,
			deck = new List<GameRecord.ItemRecord>(),
			influences = new List<GameRecord.InfluenceRecord>(),
			skillInfos = new List<GameRecord.SkillRecord>(),
			expChangeRecords = new List<GameRecord.ExpChangeRecord>(),
			gameJournal = new GameJournal(),
			discoveredObjects = new List<ICTAsset>(),
			dailyChallengeSettlementSnapshot = dailyChallengeSettlementMockCase.snapshot
		};
		gameRecord.dailyChallengeContext = new DailyChallengeRunContext
		{
			challengeId = current.challengeId,
			isDailyChallenge = true,
			scoreCompetitionContract = current.scoreCompetitionContract,
			rotationResetUtc = current.rotationResetUtc,
			settlementUtc = current.settlementUtc
		};
		DailyChallengeSettlementMockFactory.ApplyToRecord(gameRecord, dailyChallengeSettlementMockScenario, currentMockScoreContract, dailyChallengeSettlementMockCase.difficulty);
		if (dailyChallengeRankingRow.rank == 1)
		{
			IEnumerable<GameRecord.InfluenceRecord> collection = from influence in applicationState.assetCatalog.influenceCatalog.ValidAssets.Where((InfluenceDef influence) => influence != null && influence.type == InfluenceDef.InfluenceType.Positive && influence.randomGenerate && !influence.developingAssetFlag && !influence.hideInIndex).Take(20)
				select new GameRecord.InfluenceRecord
				{
					def = influence,
					influenceValue = influence.initValue
				};
			gameRecord.influences.AddRange(collection);
		}
		return gameRecord;
	}

	private void BuildStandardRanking()
	{
		int num = Mathf.Min(config.leaderboardRowLimit, MockPlayerNames.Length);
		int num2 = ((scenario == DailyChallengeMockScenario.ShortRanking) ? Mathf.Min(4, num) : num);
		int num3 = (int)(current.rotationResetUtc.Date.Ticks / 864000000000L % MockPlayerNames.Length);
		for (int i = 0; i < num2; i++)
		{
			string text = ((i % 2 == 0) ? "小骑士" : "和音奏者");
			current.topRanking.Add(new DailyChallengeRankingRow
			{
				rank = i + 1,
				recordId = $"mock-record-{i + 1}",
				playerName = MockPlayerNames[(num3 + i) % MockPlayerNames.Length],
				characterName = text,
				difficultyName = ((i % 3 == 0) ? "困难" : "普通"),
				score = 16789 - i * 421,
				avatar = FindCharacter(text)?.headIcon,
				hasRecord = true
			});
		}
	}

	private void BuildSettlementShowcase(ScoreCompetitionContractDef scoreContract)
	{
		if (scoreContract == null)
		{
			return;
		}
		int num = 0;
		foreach (DailyChallengeSettlementMockScenario item in DailyChallengeSettlementMockFactory.ShowcaseScenarios.Take(config.leaderboardRowLimit))
		{
			DailyChallengeSettlementMockCase dailyChallengeSettlementMockCase = DailyChallengeSettlementMockFactory.Create(item, scoreContract.rule, config.defaultDifficulty);
			DailyChallengeMockScoreResult dailyChallengeMockScoreResult = DailyChallengeMockScoreCalculator.Calculate(scoreContract.rule, dailyChallengeSettlementMockCase.snapshot, dailyChallengeSettlementMockCase.difficulty, dailyChallengeSettlementMockCase.challengeCompleted);
			string recordId = DailyChallengeSettlementMockFactory.GetRecordId(item);
			string text = ((num % 2 == 0) ? "小骑士" : "和音奏者");
			settlementScenarios[recordId] = item;
			current.topRanking.Add(new DailyChallengeRankingRow
			{
				recordId = recordId,
				playerName = dailyChallengeSettlementMockCase.displayName,
				characterName = text,
				difficultyName = GetDifficultyName(dailyChallengeSettlementMockCase.difficulty),
				score = dailyChallengeMockScoreResult.finalScore,
				avatar = FindCharacter(text)?.headIcon,
				hasRecord = true
			});
			num++;
		}
		current.topRanking = (from row in current.topRanking
			orderby row.score descending, row.playerName
			select row).ToList();
		for (int num2 = 0; num2 < current.topRanking.Count; num2++)
		{
			current.topRanking[num2].rank = num2 + 1;
		}
	}

	private string GetDifficultyName(DifficultyLevel difficulty)
	{
		return applicationState?.difficultyManager?.GetDifficultyInfo(difficulty)?.difficultyName ?? difficulty.ToString();
	}

	private PlayerCharacterDef FindCharacter(string name)
	{
		object obj = applicationState?.assetCatalog?.characterDefCatalog?.ValidAssets.Find((PlayerCharacterDef c) => c.characterName == name);
		if (obj == null)
		{
			FortressDataManager instance = ScriptableObjectSingleton<FortressDataManager>.instance;
			if ((object)instance == null)
			{
				return null;
			}
			obj = instance.defaultCharacter;
		}
		return (PlayerCharacterDef)obj;
	}
}
