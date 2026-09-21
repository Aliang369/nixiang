using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public class GameRecord
{
	public class BossBlessStatistic
	{
		[SaveField(null, null)]
		public string BlessId;

		[SaveField(null, null)]
		public int ExposureCount;

		[SaveField(null, null)]
		public int PickCount;

		public BossBlessStatistic()
		{
		}

		public BossBlessStatistic(string blessId)
		{
			BlessId = blessId;
		}

		public BossBlessStatistic Clone()
		{
			return new BossBlessStatistic
			{
				BlessId = BlessId,
				ExposureCount = ExposureCount,
				PickCount = PickCount
			};
		}
	}

	public class ExpChangeRecord
	{
		public enum ExpChangeSource
		{
			Progress = 0,
			Difficulty = 1,
			Achievement = 2,
			DailyWinGame = 3,
			DailyFinishGame = 4,
			VipBonus = 100,
			SettlementModifier = 200
		}

		[SaveField(null, null)]
		public ExpChangeSource source;

		[SaveField(null, null)]
		public int baseExpBonus;

		[SaveField(null, null)]
		public int fixAddExpBonus;

		[SaveField(null, null)]
		public int basePercentExpBonus;

		[SaveField(null, null)]
		public int sumPercentExpBonus;

		[SaveField(null, null)]
		public int progress;

		[SaveField(null, null)]
		public DifficultyInfo difficulty;

		[SaveField(null, null)]
		public AchievementDef achievement;

		[SaveField(null, null)]
		public Vector2Int gameRange;

		public static Color BaseExpBonusColor => Color.cyan;

		public static Color FixAddExpBonusColor => Color.white;

		public static Color BasePercentExpBonusColor => Color.magenta;

		public static Color SumPercentExpBonusColor => Color.red;

		public static ExpChangeRecord ProgressChange(DifficultyInfo difficulty, int progress)
		{
			return new ExpChangeRecord
			{
				source = ExpChangeSource.Progress,
				progress = progress,
				baseExpBonus = progress * difficulty.expPerProgress
			};
		}

		public static ExpChangeRecord FinishAchievement(AchievementDef achievement)
		{
			return new ExpChangeRecord
			{
				source = ExpChangeSource.Achievement,
				achievement = achievement,
				fixAddExpBonus = achievement.expBonus
			};
		}

		public static ExpChangeRecord FinishDifficulty(DifficultyInfo difficulty)
		{
			return new ExpChangeRecord
			{
				source = ExpChangeSource.Difficulty,
				difficulty = difficulty,
				fixAddExpBonus = difficulty.bonusExtraExp
			};
		}

		public static ExpChangeRecord DailyFinishGame(FortressDataManager.DailyGameExpBonusSetup setup)
		{
			if (setup.type == FortressDataManager.DailyGameExpBonusSetup.BonusType.FinishGame)
			{
				return new ExpChangeRecord
				{
					source = ExpChangeSource.DailyFinishGame,
					gameRange = setup.range,
					fixAddExpBonus = setup.addFixExp,
					basePercentExpBonus = setup.addPercentExp
				};
			}
			if (setup.type == FortressDataManager.DailyGameExpBonusSetup.BonusType.WinGame)
			{
				return new ExpChangeRecord
				{
					source = ExpChangeSource.DailyWinGame,
					gameRange = setup.range,
					fixAddExpBonus = setup.addFixExp,
					basePercentExpBonus = setup.addPercentExp
				};
			}
			return null;
		}

		public static ExpChangeRecord VipExtraExp()
		{
			return new ExpChangeRecord
			{
				source = ExpChangeSource.VipBonus,
				sumPercentExpBonus = ScriptableObjectSingleton<FortressDataManager>.instance.vipExpBonusPercent
			};
		}

		public string GetDescription(ApplicationStateInfo asi)
		{
			if (source == ExpChangeSource.Progress)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_exp_change_record_progress_finish_arg0.FormatWith(progress);
			}
			if (source == ExpChangeSource.Difficulty)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_exp_change_record_finish_arg0_difficulty.FormatWith(difficulty.difficultyName);
			}
			TextDataGenerator generator;
			if (source == ExpChangeSource.Achievement)
			{
				return achievement.GetDescription(asi, out generator);
			}
			if (source == ExpChangeSource.DailyWinGame)
			{
				string text = ((gameRange.x == gameRange.y) ? gameRange.x.ToString() : ScriptableObjectSingleton<TextDataPreset>.instance.term_range_arg0_arg1.FormatWith(gameRange.x, gameRange.y));
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_exp_change_record_win_arg0_game_bonus.FormatWith(text);
			}
			if (source == ExpChangeSource.DailyFinishGame)
			{
				string text2 = ((gameRange.x == gameRange.y) ? gameRange.x.ToString() : ScriptableObjectSingleton<TextDataPreset>.instance.term_range_arg0_arg1.FormatWith(gameRange.x, gameRange.y));
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_exp_change_record_finish_arg0_game_bonus.FormatWith(text2);
			}
			if (source == ExpChangeSource.VipBonus)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_exp_change_record_vip_exp_extra_bonus;
			}
			if (source == ExpChangeSource.SettlementModifier)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_exp_change_record_settlement_modifier_bonus;
			}
			return "";
		}

		public string GetFullDescription(ApplicationStateInfo asi)
		{
			string text = GetDescription(asi);
			if (baseExpBonus > 0)
			{
				text += $" {baseExpBonus:+0;-#} ".ColorText(BaseExpBonusColor);
			}
			if (basePercentExpBonus > 0)
			{
				text += $" {basePercentExpBonus:+0;-#}% ".ColorText(BasePercentExpBonusColor);
			}
			if (fixAddExpBonus > 0)
			{
				text += $" {fixAddExpBonus:+0;-#} ".ColorText(FixAddExpBonusColor);
			}
			if (sumPercentExpBonus > 0)
			{
				text += $" {sumPercentExpBonus:+0;-#}% ".ColorText(SumPercentExpBonusColor);
			}
			return text;
		}

		public static int GetExpSum(List<ExpChangeRecord> records)
		{
			List<ExpChangeRecord> list = records.Where((ExpChangeRecord r) => r.source == ExpChangeSource.SettlementModifier).ToList();
			List<ExpChangeRecord> list2 = records.Where((ExpChangeRecord r) => r.source != ExpChangeSource.SettlementModifier).ToList();
			int num = list2.Sum((ExpChangeRecord r) => r.baseExpBonus);
			int num2 = list2.Sum((ExpChangeRecord r) => r.fixAddExpBonus);
			float num3 = (100f + (float)list2.Sum((ExpChangeRecord r) => r.basePercentExpBonus)) / 100f;
			float num4 = (100f + (float)list2.Sum((ExpChangeRecord r) => r.sumPercentExpBonus)) / 100f;
			float num5 = (100f + (float)list.Sum((ExpChangeRecord r) => r.sumPercentExpBonus)) / 100f;
			return (int)(((float)num * num3 + (float)num2) * num4 * num5);
		}
	}

	public class DifficultyTrialProgressInfo
	{
		[SaveField(null, null)]
		public CharacterClassType classType;

		[SaveField(null, null)]
		public DifficultyLevel oldDifficultyLevel;

		[SaveField(null, null)]
		public DifficultyLevel newDifficultyLevel;

		[SaveField(null, null)]
		public GlobalRewardInfo reward;
	}

	public class ObjectValueRecord
	{
		[SaveField(null, null)]
		public ICTAsset target;

		[SaveField(null, null)]
		public int value;

		private ObjectValueRecord()
		{
		}

		public ObjectValueRecord(ICTAsset target, int value)
		{
			this.target = target;
			this.value = value;
		}

		public string GetTargetDisplayName()
		{
			ICTAsset iCTAsset = target;
			if (!(iCTAsset is ItemDefinition { itemName: var itemName }))
			{
				if (!(iCTAsset is SkillDef { skillName: var skillName }))
				{
					if (!(iCTAsset is InfluenceDef { InfluenceName: var influenceName }))
					{
						if (!(iCTAsset is EnchantDef { Name: var name }))
						{
							if (!(iCTAsset is RuneDef { Name: var name2 }))
							{
								if (!(iCTAsset is EnemyInfoData { name: var name3 }))
								{
									return "";
								}
								return name3;
							}
							return name2;
						}
						return name;
					}
					return influenceName;
				}
				return skillName;
			}
			return itemName;
		}
	}

	public class RuneRecord
	{
		[SaveField(null, null)]
		public RuneDef def;

		[SaveField(null, null)]
		public int value;

		public Rune DisplayRune => new Rune(def, value);

		public RuneRecord()
		{
		}

		public RuneRecord(Rune rune)
		{
			def = rune.def;
			value = rune.value;
		}
	}

	public class ItemRecord
	{
		[SaveField(null, null)]
		public ItemDefinition itemDef;

		[SaveField(null, null)]
		public int upgradeLevel;

		[SaveField(null, null)]
		public int counter;

		[SaveField(null, null)]
		public int maxCounter;

		[SaveField(null, null)]
		public List<RuneRecord> runes;

		[SaveField(null, null)]
		public ItemSkinDef skinDef;

		public Item DisplayItem
		{
			get
			{
				Item it = new Item(itemDef);
				if (skinDef != null)
				{
					it.WithSkin(skinDef);
				}
				for (int i = 0; i < upgradeLevel; i++)
				{
					it.Upgrade();
				}
				it.counter = counter;
				it.maxCounter = maxCounter;
				runes.ForEach(delegate(RuneRecord r)
				{
					it.AddRune(r.DisplayRune);
				});
				return it;
			}
		}

		public ItemRecord()
		{
		}

		public ItemRecord(Item item)
		{
			itemDef = item.def;
			upgradeLevel = item.upgradeLevel;
			counter = item.counter;
			maxCounter = item.maxCounter;
			skinDef = item.skin;
			runes = item.Runes.Select((Rune r) => new RuneRecord(r)).ToList();
		}
	}

	public class InfluenceRecord
	{
		[SaveField(null, null)]
		public InfluenceDef def;

		[SaveField(null, null)]
		public int influenceValue;

		public Influence DisplayInfluence => new Influence(def)
		{
			influenceValue = influenceValue
		};

		public InfluenceRecord()
		{
		}

		public InfluenceRecord(Influence inf)
		{
			def = inf.def;
			influenceValue = inf.influenceValue;
		}
	}

	public class SkillRecord
	{
		[SaveField(null, null)]
		public SkillDef skill;

		[SaveField(null, null)]
		public int archetypeIndex;

		[SaveField(null, null)]
		public bool activated;

		[SaveField(null, null)]
		public bool consumed;

		[SaveField(null, null)]
		public int durability;

		[SaveField(null, null)]
		public HexShape hexShape;

		[SaveField(null, null)]
		public List<SkillGem> skillGems;

		public GameSkill DisplaySkill => new GameSkill(new ProfileSkill(skill, archetypeIndex)
		{
			hexShape = hexShape,
			skillGemsMounted = skillGems
		})
		{
			durability = durability
		};

		public SkillRecord()
		{
		}

		public SkillRecord(GameSkill skill, bool activated, bool consumed)
		{
			this.skill = skill.skillDef;
			archetypeIndex = skill.archeTypeIndex;
			durability = skill.durability;
			this.activated = activated;
			this.consumed = consumed;
			hexShape = skill.profileSkill?.hexShape;
			skillGems = skill.skillGems;
		}
	}

	[SaveField(null, null)]
	public string playerId;

	[SaveField(null, null)]
	public string playerName;

	[SaveField(null, null)]
	public string gameGuid;

	[SaveField(null, null)]
	public List<AchievementDef> gameAchievedAchievements;

	[SaveField(null, null)]
	public PlayerCharacterDef character;

	[SaveField(null, null)]
	public CharacterUsageMode characterUsageMode;

	[SaveField(null, null)]
	public Contract contract;

	[SaveField(null, null)]
	public DailyChallengeRunContext dailyChallengeContext;

	[SaveField(null, null)]
	public DailyChallengeSettlement dailyChallengeSettlement;

	[SaveField(null, null)]
	public bool success;

	[SaveField(null, null)]
	public List<ItemRecord> deck;

	[SaveField(null, null)]
	public int exp;

	[SaveField(null, null)]
	public bool finished;

	[SaveField(null, null)]
	public bool skipUnfinishedNewbieClear;

	[SaveField(null, null)]
	public GameJournal gameJournal;

	[SaveField(null, null)]
	public long gameTime;

	[SaveField(null, null)]
	public DailyChallengeSettlementSnapshot dailyChallengeSettlementSnapshot;

	[SaveField(null, null)]
	public List<InfluenceRecord> influences;

	[SaveField(null, null)]
	public List<SkillRecord> skillInfos;

	[SaveField(null, null)]
	public ObjectValueRecord mostDamageEnemy;

	[SaveField(null, null)]
	public List<ObjectValueRecord> mostDamageObjects;

	[SaveField(null, null)]
	public int progress;

	[SaveField(null, null)]
	public int stage;

	[SaveField(null, null)]
	public List<GlobalRewardInfo> contractFinishRewards;

	[SaveField(null, null)]
	public List<ExpChangeRecord> expChangeRecords;

	[SaveField(null, null)]
	public int rankScore;

	[SaveField(null, null)]
	public List<ICTAsset> discoveredObjects;

	[SaveField(null, null)]
	public DateTime finishTime;

	[SaveField(null, null)]
	public Dictionary<int, int> gemConfigs;

	[SaveField(null, null)]
	public int commonCurrency;

	[SaveField(null, null)]
	public int enteredNodePoint;

	[SaveField(null, null)]
	public List<MatterInfo> inGameMatterRewards;

	[SaveField(null, null)]
	public PVPChanllengeGameRecord pvpChallengeGameRecord;

	[SaveField(null, null)]
	public List<GlobalRewardInfo> soulSacrificeRewards;

	[SaveField(null, null)]
	public GlobalRewardInfo difficultyTrialReward;

	[SaveField(null, null)]
	public DifficultyTrialProgressInfo difficultyTrialProgressInfo;

	[SaveField(null, null)]
	public DifficultyTrialProgressInfo mainlineDifficultyProgressInfo;

	[SaveField(null, null)]
	public IdolManager.SoulExpInfo soulExpInfo;

	[SaveField(null, null)]
	public List<BossBlessStatistic> bossBlessStatistics;

	[SaveField(null, null)]
	public int cheatTamperCount;

	[SaveField(null, null)]
	public int cheatMismatchReadCount;

	public bool gameoverResolveFinished;

	public List<MatterInfo> gameLootResult;

	public Dictionary<SkillGemDef, int> skillGemResults;

	public LevelLine.ChangeExpResult globalLineChangeExpResult;

	public List<RuneRecord> runes => deck.SelectMany((ItemRecord i) => i.runes).ToList();

	public List<BossBlessStatistic> BossBlessStatistics
	{
		get
		{
			return bossBlessStatistics;
		}
		set
		{
			bossBlessStatistics = value;
		}
	}

	public bool IsDailyChallenge => dailyChallengeContext?.isDailyChallenge ?? false;

	public bool AllowsDifficultyProgression => characterUsageMode != CharacterUsageMode.LimitedDeployment;

	public bool FinalLevelSuccess
	{
		get
		{
			if (success)
			{
				return contract.difficultyLevel >= DifficultyLevel.Nightmare;
			}
			return false;
		}
	}

	public ApplicationStateInfo applicationStateInfo => Bootstrap.applicationState;

	public string GameOverTitle
	{
		get
		{
			string result = (success ? ScriptableObjectSingleton<TextDataPreset>.instance.term_gameOver_win : ScriptableObjectSingleton<TextDataPreset>.instance.term_gameOver_lose);
			if (FinalLevelSuccess)
			{
				result = ScriptableObjectSingleton<TextDataPreset>.instance.term_gameOver_win_fake_ending;
			}
			return result;
		}
	}

	public int GetSumRankingScore()
	{
		return GenerateRecordRankScorePoints().Sum((RankingScorePoint s) => s.score);
	}

	public List<RankingScorePoint> GenerateRecordRankScorePoints()
	{
		return new List<RankingScorePoint>
		{
			new RankingScorePoint(RankingScorePoint.ScroeType.WinScore, this),
			new RankingScorePoint(RankingScorePoint.ScroeType.DifficultyScore, this),
			new RankingScorePoint(RankingScorePoint.ScroeType.TimeScore, this)
		};
	}

	public void TriggerResolve(ApplicationStateInfo applicationState, Action afterResoveCallback)
	{
		GenerateExpReward(applicationState);
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			return;
		}
		GameOverRequest request = new GameOverRequest(this, applicationState);
		GeneralGameAsyncOperation<GameOverRequest, GameOverResult> op = new GeneralGameAsyncOperation<GameOverRequest, GameOverResult>(request, applicationState.ChillyRoomSDK.Gameplay.GameOverAsync);
		op.AutoRetry().ManualRetry();
		op.OnComplete(delegate
		{
			GameOverResult opResult = op.OpResult;
			if (opResult != null && opResult.AlreadySettled)
			{
				Bootstrap.instance.Load(BootStrapLoadData.LoadToFortress(Bootstrap.instance.CurrentAccountID));
			}
			else
			{
				this.FetchGameOverResult(op.OpResult, new NetworkConvertContext(applicationState));
				AfterResolve();
			}
		}).OnFault(delegate
		{
			Bootstrap.instance.Load(BootStrapLoadData.LoadToFortress(Bootstrap.instance.CurrentAccountID));
		}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_settling_game);
		void AfterResolve()
		{
			ApplyServerSettlementRewards(delegate(GlobalRewardInfo reward)
			{
				reward.Give(applicationState);
			});
			foreach (MatterInfo item in gameLootResult.OrEmptyIfNull())
			{
				GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo();
				globalRewardInfo.matter = item;
				globalRewardInfo.rewardSourceType = GlobalRewardSourceType.GameOverLootMatter;
				globalRewardInfo.Give(applicationState);
			}
			foreach (GlobalRewardInfo gemReward in GetGemRewards())
			{
				gemReward.Give(applicationState);
			}
			GetSeasonTrialSoul();
			applicationState.matterManager.AddMatter(applicationState.matterManager.CommonCurrencyMatter.matterDef, commonCurrency, "结算固定奖励");
			ChangeExpResolve(applicationState);
			DailyChallengeRunContext dailyChallengeRunContext = dailyChallengeContext;
			if (dailyChallengeRunContext != null && dailyChallengeRunContext.isDailyChallenge)
			{
				rankScore = 0;
			}
			else
			{
				rankScore = GetSumRankingScore();
			}
			if (success)
			{
				if (!skipUnfinishedNewbieClear && ShouldApplySharedContractCompletion())
				{
					CharacterClassType? characterClassType = character?.attachClass?.mClass;
					DifficultyLevel oldDifficultyLevel = (characterClassType.HasValue ? applicationState.idolManager.GetMainlineDifficulty(characterClassType.Value) : DifficultyLevel.Not);
					contract.FinishContract(applicationState, character, AllowsDifficultyProgression);
					if (characterClassType.HasValue && AllowsDifficultyProgression)
					{
						Contract obj = contract;
						if (obj != null && obj.contractDef?.contractType == ContractDef.ContractType.BaseGame)
						{
							mainlineDifficultyProgressInfo = new DifficultyTrialProgressInfo
							{
								classType = characterClassType.Value,
								oldDifficultyLevel = oldDifficultyLevel,
								newDifficultyLevel = applicationState.idolManager.GetMainlineDifficulty(characterClassType.Value)
							};
						}
					}
				}
				applicationState.forerunnerManager.localForerunnerLegacy = null;
			}
			GetDifficultyTrialSoul();
			gameoverResolveFinished = true;
			afterResoveCallback?.Invoke();
		}
	}

	public void ApplyServerSettlementRewards(Action<GlobalRewardInfo> applyReward)
	{
		if (applyReward == null)
		{
			throw new ArgumentNullException("applyReward");
		}
		foreach (GlobalRewardInfo item in contractFinishRewards.OrEmptyIfNull())
		{
			applyReward(item);
		}
	}

	public bool ShouldApplySharedContractCompletion()
	{
		return !IsDailyChallenge;
	}

	private MatterInfo GenerateVipDebrisDropLocal(ApplicationStateInfo applicationState)
	{
		if (!success || !applicationState.playerInfoManager.vip)
		{
			return null;
		}
		if (character == null || character.obsessionDebris == null)
		{
			return null;
		}
		if (character.FullID == "#CHAD#000007#" || character.FullID == "#CHAD#000027#")
		{
			return null;
		}
		FortressDataManager instance = ScriptableObjectSingleton<FortressDataManager>.instance;
		Dictionary<DifficultyLevel, FortressDataManager.VipDebrisDropRange> dictionary;
		if (stage >= ScriptableObjectSingleton<DungeonGenerationManager>.instance.finalFinishGameStage)
		{
			dictionary = instance.vipDebrisDropFinalBoss;
		}
		else
		{
			if (stage < ScriptableObjectSingleton<DungeonGenerationManager>.instance.baseFinishGameStage)
			{
				return null;
			}
			dictionary = instance.vipDebrisDropNormalBoss;
		}
		if (dictionary == null || !dictionary.TryGetValue(contract.difficultyLevel, out var value))
		{
			return null;
		}
		int num = Math.Min(value.min, value.max);
		int num2 = Math.Max(value.min, value.max);
		if (num2 <= 0)
		{
			return null;
		}
		int num3 = ((num >= num2) ? num2 : UnityEngine.Random.Range(num, num2 + 1));
		if (num3 <= 0)
		{
			return null;
		}
		return new MatterInfo(character.obsessionDebris, num3);
	}

	private void GenerateExpReward(ApplicationStateInfo applicationState)
	{
		int variableValue = applicationState.runningGameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.GameOverExpMultiplier);
		if (variableValue > 1)
		{
			exp *= variableValue;
		}
		globalLineChangeExpResult = LevelLine.ChangeExpRequest.ChangeGlobalExp(exp).Generate(applicationState, "全局等级线奖励");
		skillGemResults = GenerateGems(applicationState);
		gameLootResult = new List<MatterInfo>();
		if (!inGameMatterRewards.IsNullOrEmpty())
		{
			gameLootResult.AddRange(inGameMatterRewards);
		}
		MatterInfo matterInfo = applicationState.runningGameState.classMatterManager.GenerateGameOverMatters(enteredNodePoint);
		if (matterInfo != null)
		{
			gameLootResult.Add(matterInfo);
		}
		MatterInfo matterInfo2 = GenerateVipDebrisDropLocal(applicationState);
		if (matterInfo2 != null)
		{
			gameLootResult.Add(matterInfo2);
		}
	}

	private Dictionary<SkillGemDef, int> GenerateGems(ApplicationStateInfo applicationState)
	{
		Dictionary<SkillGemDef, int> dictionary = new Dictionary<SkillGemDef, int>();
		if (gemConfigs.IsNullOrEmpty())
		{
			return dictionary;
		}
		foreach (var (level, num3) in gemConfigs)
		{
			for (int i = 0; i < num3; i++)
			{
				SkillGemDef randomGem = applicationState.skillGemManager.GetRandomGem(level);
				if (randomGem != null)
				{
					dictionary.Change(randomGem, 1);
				}
			}
		}
		return dictionary;
	}

	private void ChangeExpResolve(ApplicationStateInfo applicationState)
	{
		globalLineChangeExpResult?.Excute(applicationState);
	}

	private void GetDifficultyTrialSoul()
	{
		if (!success)
		{
			return;
		}
		Contract obj = contract;
		if ((obj == null || obj.contractDef?.IsDifficultyIsolated != true) && !skipUnfinishedNewbieClear && AllowsDifficultyProgression)
		{
			DifficultyTrialProgressInfo difficultyTrialProgressInfo = mainlineDifficultyProgressInfo;
			if (HasActualMainlineDifficultyProgress(difficultyTrialProgressInfo))
			{
				CharacterClassType classType = difficultyTrialProgressInfo.classType;
				IdolState idolState = applicationStateInfo.idolManager.GetIdolState(classType);
				DifficultyLevel difficultyLevel = idolState?.currentDifficultyLevel ?? DifficultyLevel.Easy;
				difficultyTrialReward = applicationStateInfo.idolManager.OnDifficultyClear(classType, (int)difficultyTrialProgressInfo.newDifficultyLevel);
				DifficultyLevel newDifficultyLevel = idolState?.currentDifficultyLevel ?? difficultyLevel;
				this.difficultyTrialProgressInfo = new DifficultyTrialProgressInfo
				{
					classType = classType,
					oldDifficultyLevel = difficultyLevel,
					newDifficultyLevel = newDifficultyLevel,
					reward = difficultyTrialReward
				};
			}
		}
	}

	private static bool HasActualMainlineDifficultyProgress(DifficultyTrialProgressInfo progress)
	{
		if (progress != null)
		{
			return progress.newDifficultyLevel > progress.oldDifficultyLevel;
		}
		return false;
	}

	private void GetSeasonTrialSoul()
	{
		soulExpInfo = applicationStateInfo.idolManager.OnExperienceGain(character.attachClass.mClass, exp);
	}

	public List<GlobalRewardInfo> GetAllRewardInfos()
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		list.AddRange(contractFinishRewards.OrEmptyIfNull());
		list.AddRange(from m in gameLootResult.OrEmptyIfNull()
			select new GlobalRewardInfo
			{
				matter = m
			});
		list.AddRange((globalLineChangeExpResult?.GetAllUnFoldRewards()).OrEmptyIfNull());
		return list;
	}

	public string GenerateDetailText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(ScriptableObjectSingleton<TextDataPreset>.instance.term_journal_content_title_damage_output);
		if (!mostDamageObjects.IsNullOrEmpty())
		{
			for (int i = 0; i < mostDamageObjects.Count; i++)
			{
				stringBuilder.AppendLine(mostDamageObjects[i].GetTargetDisplayName() + "  " + mostDamageObjects[i].value);
			}
		}
		stringBuilder.AppendLine(ScriptableObjectSingleton<TextDataPreset>.instance.term_journal_content_title_enemy_threat);
		if (mostDamageEnemy != null)
		{
			stringBuilder.AppendLine(mostDamageEnemy.GetTargetDisplayName() + "  " + mostDamageEnemy.value);
		}
		return stringBuilder.ToString();
	}

	public List<ExpChangeRecord> GetExpChangeRecords(ApplicationStateInfo applicationState, bool isAfterIntroFirstGame)
	{
		List<ExpChangeRecord> list = new List<ExpChangeRecord>();
		list.Add(ExpChangeRecord.ProgressChange(contract.GetDifficultyInfo(applicationState), progress));
		foreach (AchievementDef gameAchievedAchievement in gameAchievedAchievements)
		{
			list.Add(ExpChangeRecord.FinishAchievement(gameAchievedAchievement));
		}
		if (!isAfterIntroFirstGame)
		{
			foreach (FortressDataManager.DailyGameExpBonusSetup dailyGameExpBonusSetup in ScriptableObjectSingleton<FortressDataManager>.instance.dailyGameExpBonusSetups)
			{
				if (dailyGameExpBonusSetup.IsValid(success, applicationState))
				{
					list.Add(ExpChangeRecord.DailyFinishGame(dailyGameExpBonusSetup));
				}
			}
		}
		if (success)
		{
			Contract obj = contract;
			if (obj == null || obj.contractDef?.IsDifficultyIsolated != true)
			{
				list.Add(ExpChangeRecord.FinishDifficulty(applicationState.difficultyManager.GetDifficultyInfo(contract.difficultyLevel)));
			}
		}
		if (applicationState.playerInfoManager.vip)
		{
			list.Add(ExpChangeRecord.VipExtraExp());
		}
		return list;
	}

	public List<GlobalRewardInfo> GetGemRewards()
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		if (skillGemResults.IsNullOrEmpty())
		{
			return list;
		}
		foreach (var (def, amount) in skillGemResults)
		{
			list.Add(new GlobalRewardInfo
			{
				skillGem = new SkillGem(def, amount),
				rewardSourceType = GlobalRewardSourceType.GameOverGem
			});
		}
		return list;
	}
}
