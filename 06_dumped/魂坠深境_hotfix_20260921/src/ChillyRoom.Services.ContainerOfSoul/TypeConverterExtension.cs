using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChillyRoom.Services.ContainerOfSoul;

public static class TypeConverterExtension
{
	public static global::MatterInfo ToClient(this MatterInfo matterInfo, NetworkConvertContext context)
	{
		MatterDef matterDef = context.FullIdToCTAsset<MatterDef>(matterInfo.MatterId);
		if (matterDef == null)
		{
			return null;
		}
		return new global::MatterInfo(matterDef, matterInfo.Amount);
	}

	public static global::DifficultyLevel ToClient(this DifficultyLevel difficulty)
	{
		return (global::DifficultyLevel)difficulty;
	}

	public static global::GameRecord.ExpChangeRecord.ExpChangeSource ToClient(this ExpChangeType type)
	{
		return (global::GameRecord.ExpChangeRecord.ExpChangeSource)type;
	}

	public static DifficultyLevel ToServer(this global::DifficultyLevel difficulty)
	{
		return (DifficultyLevel)difficulty;
	}

	public static LevelLine ToClient(this LevelLineInfo levelLineInfo, NetworkConvertContext context)
	{
		LevelLineDef levelLineDef = context.FullIdToCTAsset<LevelLineDef>(levelLineInfo.LevelLineID);
		if (levelLineDef == null)
		{
			return null;
		}
		LevelLine obj = new LevelLine(levelLineDef)
		{
			currentLevelIndex = levelLineInfo.LevelIndex,
			currentExp = levelLineInfo.Exp,
			dumpExtraLevelCount = levelLineInfo.DumpExtraLevelCount
		};
		obj.next = obj.levelLineDef.expDump?.rewardSetups.IndexOrNull(levelLineInfo.NextDumpRewardIndex);
		obj.baseRewardGotLevels = levelLineInfo.BaseRewardGotLevelIndex;
		obj.appendRewardGotLevels = levelLineInfo.AppendRewardGotLevelIndex;
		return obj;
	}

	public static Season ToClient(this SeasonInfo seasonInfo, NetworkConvertContext context)
	{
		SeasonDef def = context.FullIdToCTAsset<SeasonDef>(seasonInfo.SeasonID);
		return new Season(def)
		{
			def = def,
			seasonLeveLine = seasonInfo.SeasonLevelLineInfo.ToClient(context),
			seasonPassUnlocked = seasonInfo.SeasonPassUnlocked,
			highseasonPassUnlocked = seasonInfo.HighSeasonPassUnlocked
		};
	}

	public static StoryPointManager.StoryPointState ToClient(this StoryPointState state, NetworkConvertContext context)
	{
		StoryPoint storyPoint = context.FullIdToCTAsset<StoryPoint>(state.StoryPointID);
		if (storyPoint == null)
		{
			return null;
		}
		return new StoryPointManager.StoryPointState(storyPoint)
		{
			finished = state.Finished,
			observeCount = state.ObserveCount
		};
	}

	public static StoryPointManager.FortressNPCState ToClient(this FortressNPCState state, NetworkConvertContext context)
	{
		return new StoryPointManager.FortressNPCState
		{
			fortressNPCId = state.Id,
			finishedProgressStoryPoints = (state.FinishedProgress?.Select((string id) => context.FullIdToCTAsset<StoryPoint>(id)).WhereNotNull().ToList() ?? new List<StoryPoint>())
		};
	}

	public static global::CharacterClassState ToClient(this CharacterClassState classState, NetworkConvertContext context)
	{
		CharacterClassDef characterClassDef = context.FullIdToCTAsset<CharacterClassDef>(classState.CharacterClassID);
		if (characterClassDef == null)
		{
			return null;
		}
		global::CharacterClassState characterClassState = new global::CharacterClassState(characterClassDef);
		if (classState.ForgetShrineItems != null)
		{
			List<ItemDefinition> forgetShrineItems = classState.ForgetShrineItems.Select((string id) => context.FullIdToCTAsset<ItemDefinition>(id)).WhereNotNull().ToList();
			characterClassState.forgetShrineItems = forgetShrineItems;
		}
		return characterClassState;
	}

	public static global::IdolState ToClient(this IdolState idolState, NetworkConvertContext context)
	{
		IdolDef idolDef = context.FullIdToCTAsset<IdolDef>(idolState.IdolId);
		if (idolDef == null)
		{
			return null;
		}
		return new global::IdolState(idolDef)
		{
			currentLevel = idolState.CurrentLevel,
			currentDifficultyLevel = (global::DifficultyLevel)idolState.CurrentDifficultyLevel,
			mainlineDifficulty = (idolState.MainlineDifficulty.HasValue ? ((global::DifficultyLevel)idolState.MainlineDifficulty.Value) : global::DifficultyLevel.Not),
			seasonExperience = idolState.SeasonExperience,
			seasonSoul = idolState.SeasonSoul
		};
	}

	public static global::PlayerCharacterState ToClient(this PlayerCharacterState characterState, NetworkConvertContext context)
	{
		PlayerCharacterDef playerCharacterDef = context.FullIdToCTAsset<PlayerCharacterDef>(characterState.CharacterId);
		if (playerCharacterDef == null)
		{
			return null;
		}
		global::PlayerCharacterState character = new global::PlayerCharacterState(playerCharacterDef);
		character.locked = characterState.IsLocked;
		character.currentMastery = context.FullIdToCTAsset<CharacterMastery>(characterState.CurrentMasteryId);
		if (characterState.SkillPresets.IsNullOrEmpty())
		{
			character.skillPresets = new List<ProfileSkillPreset>();
			for (int i = 0; i < ScriptableObjectSingleton<FortressDataManager>.instance.characterSkillPresetCount; i++)
			{
				character.skillPresets.Add(new ProfileSkillPreset(i, character));
			}
		}
		else
		{
			character.skillPresets = characterState.SkillPresets.Select((CharacterSkillPreset s, int index) => s.ToClient(index, character, context)).ToList();
		}
		if (characterState.CurrentSkillPresetIndex < 0 || characterState.CurrentSkillPresetIndex >= character.skillPresets.Count)
		{
			characterState.CurrentSkillPresetIndex = 0;
		}
		else
		{
			character.skillPreset = character.skillPresets[characterState.CurrentSkillPresetIndex];
		}
		character.obsessionLevel = characterState.ObsessionLevel;
		character.unlockedTalents = characterState.UnlockedTalents.Select((string t) => context.FullIdToCTAsset<TalentDef>(t)).ToList();
		return character;
	}

	public static ProfileSkillPreset ToClient(this CharacterSkillPreset skillPreset, int index, global::PlayerCharacterState character, NetworkConvertContext context)
	{
		ProfileSkillPreset profileSkillPreset = new ProfileSkillPreset(index, character);
		for (int i = 0; i < skillPreset.ProfileSkillGuidList.Count; i++)
		{
			string skillGuid = skillPreset.ProfileSkillGuidList[i];
			global::ProfileSkill skill = context.applicationState.globalSkillManager.allSkills.FirstOrDefault((global::ProfileSkill s) => s.profileSkillGUID == skillGuid);
			profileSkillPreset.configSkillSlots[i].skill = skill;
		}
		return profileSkillPreset;
	}

	public static PlayerStatisticsInfo ToServer(this GameStatisticsManager manager)
	{
		return new PlayerStatisticsInfo
		{
			CastItemCount = manager.castItemCount,
			CoinGetCount = manager.coinGetCount,
			CrystalGetCount = manager.crystalGetCount,
			DamageCount = manager.damageCount,
			DefeatEnemyCount = manager.defeatEnemyCount,
			EncounterEventCount = manager.encounterEventCount,
			GameCount = manager.gameCount,
			WinGameCount = manager.winGameCount,
			HighestFinishedDifficulty = manager.highestFinishedDifficulty.ToServer(),
			CurrentWinStreak = manager.currentWinStreak,
			MaxWinStreak = manager.maxWinStreak,
			DifficultyStatistics = (manager.difficultyStatistics?.ToDictionary((KeyValuePair<int, GameStatisticsManager.GameModeStatisticsInfo> kv) => kv.Key, (KeyValuePair<int, GameStatisticsManager.GameModeStatisticsInfo> kv) => kv.Value.ToServer()) ?? new Dictionary<int, GameModeStatisticsInfo>()),
			ClassStatistics = (manager.classStatistics?.Where((KeyValuePair<string, GameStatisticsManager.PlayerClassStatisticsInfo> kv) => kv.Value != null).ToDictionary((KeyValuePair<string, GameStatisticsManager.PlayerClassStatisticsInfo> kv) => kv.Key, (KeyValuePair<string, GameStatisticsManager.PlayerClassStatisticsInfo> kv) => kv.Value.ToServer()) ?? new Dictionary<string, PlayerClassStatisticsInfo>())
		};
	}

	public static void ApplyToClient(this PlayerStatisticsInfo info, GameStatisticsManager manager)
	{
		manager.castItemCount = info.CastItemCount;
		manager.coinGetCount = info.CoinGetCount;
		manager.crystalGetCount = info.CrystalGetCount;
		manager.damageCount = info.DamageCount;
		manager.defeatEnemyCount = info.DefeatEnemyCount;
		manager.encounterEventCount = info.EncounterEventCount;
		manager.gameCount = info.GameCount;
		manager.winGameCount = info.WinGameCount;
		manager.highestFinishedDifficulty = info.HighestFinishedDifficulty.ToClient();
		manager.currentWinStreak = info.CurrentWinStreak;
		manager.maxWinStreak = info.MaxWinStreak;
		manager.difficultyStatistics = info.DifficultyStatistics?.ToDictionary((KeyValuePair<int, GameModeStatisticsInfo> kv) => kv.Key, (KeyValuePair<int, GameModeStatisticsInfo> kv) => kv.Value.ToClient()) ?? new Dictionary<int, GameStatisticsManager.GameModeStatisticsInfo>();
		manager.classStatistics = info.ClassStatistics?.Where((KeyValuePair<string, PlayerClassStatisticsInfo> kv) => kv.Value != null).ToDictionary((KeyValuePair<string, PlayerClassStatisticsInfo> kv) => kv.Key, (KeyValuePair<string, PlayerClassStatisticsInfo> kv) => kv.Value.ToClient()) ?? new Dictionary<string, GameStatisticsManager.PlayerClassStatisticsInfo>();
	}

	private static GameStatisticsManager.GameModeStatisticsInfo ToClient(this GameModeStatisticsInfo info)
	{
		if (info == null)
		{
			return new GameStatisticsManager.GameModeStatisticsInfo();
		}
		return new GameStatisticsManager.GameModeStatisticsInfo
		{
			gameCount = info.GameCount,
			winGameCount = info.WinGameCount
		};
	}

	private static GameModeStatisticsInfo ToServer(this GameStatisticsManager.GameModeStatisticsInfo info)
	{
		if (info == null)
		{
			return new GameModeStatisticsInfo();
		}
		return new GameModeStatisticsInfo
		{
			GameCount = info.gameCount,
			WinGameCount = info.winGameCount
		};
	}

	private static GameStatisticsManager.PlayerClassStatisticsInfo ToClient(this PlayerClassStatisticsInfo info)
	{
		if (info == null)
		{
			return new GameStatisticsManager.PlayerClassStatisticsInfo();
		}
		return new GameStatisticsManager.PlayerClassStatisticsInfo
		{
			castItemCount = info.CastItemCount,
			coinGetCount = info.CoinGetCount,
			crystalGetCount = info.CrystalGetCount,
			damageCount = info.DamageCount,
			defeatEnemyCount = info.DefeatEnemyCount,
			encounterEventCount = info.EncounterEventCount,
			gameCount = info.GameCount,
			winGameCount = info.WinGameCount,
			highestFinishedDifficulty = info.HighestFinishedDifficulty.ToClient(),
			currentWinStreak = info.CurrentWinStreak,
			maxWinStreak = info.MaxWinStreak,
			difficultyStatistics = (info.DifficultyStatistics?.ToDictionary((KeyValuePair<int, GameModeStatisticsInfo> kv) => kv.Key, (KeyValuePair<int, GameModeStatisticsInfo> kv) => kv.Value.ToClient()) ?? new Dictionary<int, GameStatisticsManager.GameModeStatisticsInfo>())
		};
	}

	private static PlayerClassStatisticsInfo ToServer(this GameStatisticsManager.PlayerClassStatisticsInfo info)
	{
		if (info == null)
		{
			return new PlayerClassStatisticsInfo();
		}
		return new PlayerClassStatisticsInfo
		{
			CastItemCount = info.castItemCount,
			CoinGetCount = info.coinGetCount,
			CrystalGetCount = info.crystalGetCount,
			DamageCount = info.damageCount,
			DefeatEnemyCount = info.defeatEnemyCount,
			EncounterEventCount = info.encounterEventCount,
			GameCount = info.gameCount,
			WinGameCount = info.winGameCount,
			HighestFinishedDifficulty = info.highestFinishedDifficulty.ToServer(),
			CurrentWinStreak = info.currentWinStreak,
			MaxWinStreak = info.maxWinStreak,
			DifficultyStatistics = (info.difficultyStatistics?.ToDictionary((KeyValuePair<int, GameStatisticsManager.GameModeStatisticsInfo> kv) => kv.Key, (KeyValuePair<int, GameStatisticsManager.GameModeStatisticsInfo> kv) => kv.Value.ToServer()) ?? new Dictionary<int, GameModeStatisticsInfo>())
		};
	}

	public static global::ProfileSkill ToClient(this ProfileSkill skill, NetworkConvertContext context)
	{
		SkillDef skillDef = context.FullIdToCTAsset<SkillDef>(skill.SkillDefId);
		if (skillDef == null)
		{
			return null;
		}
		global::ProfileSkill profileSkill = new global::ProfileSkill(skillDef);
		profileSkill.profileSkillGUID = skill.Guid;
		profileSkill.archetypeIndex = skill.ArchetypeIndex;
		if (skill.HexShape != null)
		{
			profileSkill.hexShape = new HexShape
			{
				points = skill.HexShape.Points.Select((Vector2IntSvr p) => new Vector2Int(p.X, p.Y)).ToList(),
				offset = new Vector2Int(skill.HexShape.Offset.X, skill.HexShape.Offset.Y)
			};
		}
		List<SkillGem> mountedSkillGems = skill.MountedSkillGems;
		if (mountedSkillGems != null && mountedSkillGems.Count > 0)
		{
			profileSkill.skillGemsMounted = (from g in skill.MountedSkillGems
				select g.ToClient(context) into g
				where g != null
				select g).ToList();
		}
		return profileSkill;
	}

	public static global::SkillGem ToClient(this SkillGem skillGem, NetworkConvertContext context)
	{
		SkillGemDef skillGemDef = context.FullIdToCTAsset<SkillGemDef>(skillGem.DefId);
		if (skillGemDef == null)
		{
			return null;
		}
		return new global::SkillGem(skillGemDef)
		{
			amount = skillGem.Amount,
			offset = new Vector2Int(skillGem.Offset.X, skillGem.Offset.Y),
			rotateIndex = skillGem.RotateIndex,
			tempRotateIndex = skillGem.RotateIndex,
			tempOffset = new Vector2Int(skillGem.Offset.X, skillGem.Offset.Y)
		};
	}

	public static SkillGem ToServer(this global::SkillGem skillGem)
	{
		return new SkillGem
		{
			DefId = skillGem.def.FullID,
			RotateIndex = skillGem.rotateIndex,
			Offset = skillGem.offset.ToServer(),
			Amount = skillGem.amount
		};
	}

	public static global::ProfileSkill FindInClient(this ProfileSkill skill, ApplicationStateInfo applicationStateInfo)
	{
		return applicationStateInfo.globalSkillManager.allSkills.FirstOrDefault((global::ProfileSkill s) => s.profileSkillGUID == skill.Guid);
	}

	public static global::SkillGem ToClient(this KeyValuePair<string, int> gem, NetworkConvertContext context)
	{
		SkillGemDef skillGemDef = context.FullIdToCTAsset<SkillGemDef>(gem.Key);
		if (skillGemDef == null)
		{
			return null;
		}
		return new global::SkillGem(skillGemDef)
		{
			amount = gem.Value
		};
	}

	public static void FetchGameOverResult(this global::GameRecord record, GameOverResult gameOverResult, NetworkConvertContext context)
	{
		if (record == null || gameOverResult == null || context == null)
		{
			return;
		}
		record.exp = gameOverResult.ExpSum;
		record.commonCurrency = gameOverResult.CommonCurrency;
		record.skillGemResults = new Dictionary<SkillGemDef, int>();
		foreach (var (id, value) in gameOverResult.Gems.OrEmptyIfNull())
		{
			SkillGemDef skillGemDef = context.FullIdToCTAsset<SkillGemDef>(id);
			if (skillGemDef != null)
			{
				record.skillGemResults.Add(skillGemDef, value);
			}
		}
		record.expChangeRecords = (from r in gameOverResult.ExpChangeRecords.OrEmptyIfNull()
			select r.ToClient(context) into r
			where r != null
			select r).ToList();
		record.gameLootResult = (from c in gameOverResult.InGameMatterRewards.OrEmptyIfNull()
			select c.ToClient(context) into c
			where c != null
			select c).ToList();
		record.rankScore = gameOverResult.RankScore;
		DailyChallengeRunContext dailyChallengeContext = record.dailyChallengeContext;
		if (dailyChallengeContext != null && dailyChallengeContext.isDailyChallenge)
		{
			record.dailyChallengeSettlement = DailyChallengeClientMapper.ToSettlement(gameOverResult.DailyChallengeSettlement, context.applicationState, null, record.dailyChallengeContext.scoreCompetitionContract);
			record.rankScore = 0;
		}
		List<LevelLine.ChangeExpResult> replaceList = (from r in gameOverResult.LevelLineChangeExpResults.OrEmptyIfNull()
			select r.ToClient(context) into r
			where r != null
			select r).ToList();
		ReplaceChangeExpResult(ref record.globalLineChangeExpResult, replaceList);
		record.contractFinishRewards = new List<global::GlobalRewardInfo>();
		GameOverContractRewardInfo gameOverContractRewardInfo = gameOverResult.GameOverContractRewardInfo;
		if (gameOverContractRewardInfo != null)
		{
			AddRewards(gameOverContractRewardInfo.ContractFinishRewardInfos);
			AddRewards(gameOverContractRewardInfo.ContractDifficultyRewardInfos);
			AddRewards(gameOverContractRewardInfo.ContractAccountDifficultyRewardInfos);
			AddRewards(gameOverContractRewardInfo.ContractClassDifficultyRewardInfos);
			AddRewards(gameOverContractRewardInfo.StarMapAllFinishRewardInfos);
		}
		void AddRewards(IEnumerable<GlobalRewardInfo> rewards)
		{
			record.contractFinishRewards.AddRange(from c in rewards.OrEmptyIfNull()
				select c.ToClient(context) into c
				where c != null
				select c);
		}
		static void ReplaceChangeExpResult(ref LevelLine.ChangeExpResult target, List<LevelLine.ChangeExpResult> list)
		{
			if (target != null && list != null && list.Count != 0)
			{
				LevelLineDef targetLevelLine = target.levelLineDef;
				LevelLine.ChangeExpResult changeExpResult = list.FirstOrDefault((LevelLine.ChangeExpResult r) => r.levelLineDef == targetLevelLine);
				if (changeExpResult != null)
				{
					target = changeExpResult;
				}
			}
		}
	}

	public static global::GameRecord.ExpChangeRecord ToClient(this ExpChangeRecord record, NetworkConvertContext context)
	{
		global::GameRecord.ExpChangeRecord expChangeRecord = new global::GameRecord.ExpChangeRecord();
		expChangeRecord.source = record.ChangeType.ToClient();
		expChangeRecord.baseExpBonus = record.BaseExpBonus;
		expChangeRecord.basePercentExpBonus = record.BasePercentExpBonus;
		expChangeRecord.fixAddExpBonus = record.FixAddExpBonus;
		expChangeRecord.sumPercentExpBonus = record.SumPercentExpBonus;
		expChangeRecord.progress = record.Progress;
		expChangeRecord.difficulty = context.applicationState.assetCatalog.difficultyCatalog.ValidAssets.Find((DifficultyInfo t) => t.difficultyLevel == record.DifficultyLevel.ToClient());
		if (!record.AchievementId.IsNullOrEmpty())
		{
			expChangeRecord.achievement = context.FullIdToCTAsset<AchievementDef>(record.AchievementId);
		}
		expChangeRecord.gameRange = new Vector2Int(record.DailyMinTimes, record.DailyMaxTimes);
		return expChangeRecord;
	}

	public static LevelLine.ChangeExpResult ToClient(this ChangeExpResult result, NetworkConvertContext context)
	{
		return new LevelLine.ChangeExpResult
		{
			request = result.Request.ToClient(context),
			levelLineDef = context.FullIdToCTAsset<LevelLineDef>(result.LevelLineId),
			initExp = result.InitExp,
			initLeveLIndex = result.InitLevelIndex,
			finalExp = result.FinalExp,
			finalLevelIndex = result.FinalLevelIndex,
			levelUps = result.LevelUpInfos.Select((LevelUpRewardInfo c) => c.ToClient(context)).ToList(),
			dumpRewards = result.DumpRewards.Select((GlobalRewardInfo r) => r.ToClient(context)).ToList(),
			fromQuest = result.FromQuest
		};
	}

	public static LevelLine.ChangeExpRequest ToClient(this ChangeExpRequest request, NetworkConvertContext context)
	{
		return new LevelLine.ChangeExpRequest
		{
			type = request.LevelLineType.ToClient(),
			deltaExp = request.DeltaExp
		};
	}

	public static LevelLine.ChangeExpResult.LevelRewardInfo ToClient(this LevelUpRewardInfo info, NetworkConvertContext context)
	{
		LevelLine.ChangeExpResult.LevelRewardInfo levelRewardInfo = new LevelLine.ChangeExpResult.LevelRewardInfo();
		levelRewardInfo.levelIndex = info.LevelIndex;
		if (info.BaseReward != null)
		{
			levelRewardInfo.baseReward = info.BaseReward.ToClient(context);
		}
		if (info.AppendReward != null)
		{
			levelRewardInfo.appendReward = info.AppendReward.ToClient(context);
		}
		return levelRewardInfo;
	}

	public static global::GlobalRewardInfo ToClient(this GlobalRewardInfo rewardInfo, NetworkConvertContext context)
	{
		global::GlobalRewardInfo globalRewardInfo = new global::GlobalRewardInfo();
		if (rewardInfo.MatterInfo != null)
		{
			globalRewardInfo.matter = rewardInfo.MatterInfo.ToClient(context);
		}
		if (rewardInfo.Skill != null)
		{
			globalRewardInfo.skill = rewardInfo.Skill.ToClient(context);
		}
		if (rewardInfo.SkillGem != null)
		{
			globalRewardInfo.skillGem = rewardInfo.SkillGem.ToClient(context);
		}
		if (rewardInfo.UnlockCardSkin != null)
		{
			globalRewardInfo.unlockItemSkin = context.FullIdToCTAsset<ItemSkinDef>(rewardInfo.UnlockCardSkin);
		}
		if (rewardInfo.GetStartGameBuff != null)
		{
			globalRewardInfo.getStartGameBuff = rewardInfo.GetStartGameBuff.ToClient(context);
		}
		if (rewardInfo.UnlockCTSOs != null)
		{
			globalRewardInfo.unlockCTSOs = rewardInfo.UnlockCTSOs.Select(context.FullIdToCTAsset<CTScriptableObject>).ToList();
		}
		if (rewardInfo.UnlockSeasonPassId != null)
		{
			globalRewardInfo.unlockSeasonPass = context.FullIdToCTAsset<SeasonDef>(rewardInfo.UnlockSeasonPassId);
		}
		if (rewardInfo.UnlockHighSeasonPassId != null)
		{
			globalRewardInfo.unlockHighSeasonPass = context.FullIdToCTAsset<SeasonDef>(rewardInfo.UnlockHighSeasonPassId);
		}
		if (rewardInfo.SetGlobalVariableFlag != null)
		{
			globalRewardInfo.setGlobalVariableFlag = rewardInfo.SetGlobalVariableFlag;
		}
		if (rewardInfo.UnlockPermantVIP)
		{
			globalRewardInfo.unlockPermanentVIP = rewardInfo.UnlockPermantVIP;
		}
		if (rewardInfo.UnlockMonthlyCard)
		{
			globalRewardInfo.unlockMonthlyCard = rewardInfo.UnlockMonthlyCard;
		}
		if (rewardInfo.UnlockPVPAvatar != null)
		{
			globalRewardInfo.unlockPVPEnemyAvatar = context.FullIdToCTAsset<EnemyInfoData>(rewardInfo.UnlockPVPAvatar);
		}
		if (rewardInfo.ChangeLevelLineExpResult != null)
		{
			globalRewardInfo.changeLevelLineExpResult = rewardInfo.ChangeLevelLineExpResult.ToClient(context);
		}
		if (rewardInfo.NestedRewards != null)
		{
			globalRewardInfo.nestedRewards = rewardInfo.NestedRewards.Select((GlobalRewardInfo r) => r.ToClient(context)).ToList();
		}
		globalRewardInfo.nestedType = (global::GlobalRewardInfo.RewardNestedType)rewardInfo.NestedType;
		globalRewardInfo.baseTokenCompensate = rewardInfo.BaseTokenCompensate;
		globalRewardInfo.giveAppendInfo = rewardInfo.GiveAppendInfo?.ToClient();
		globalRewardInfo.rewardSourceType = (GlobalRewardSourceType)rewardInfo.SourceType;
		globalRewardInfo.rewardSourceId = rewardInfo.SourceId;
		globalRewardInfo.forgetShinePoint = rewardInfo.ForgetShinePoint;
		return globalRewardInfo;
	}

	public static global::GlobalRewardGiveAppendInfo ToClient(this GlobalRewardGiveAppendInfo info)
	{
		return new global::GlobalRewardGiveAppendInfo
		{
			repeated = info.Repeated,
			repeatTokenCompensate = info.RepeatTokenCompensate,
			characterLevel = info.CharacterLevel,
			isNewGive = info.IsNewGive,
			soul = info.Soul,
			convertObsessionToUnlockCharacter = info.ConvertObsessionToUnlockCharacter,
			convertObsessionToDebris = info.ConvertObsessionToDebris
		};
	}

	public static LevelLineDef.LevelLineType ToClient(this LevelLineType type)
	{
		return (LevelLineDef.LevelLineType)type;
	}

	public static global::GameRecord ToClient(this GameRecord record, NetworkConvertContext context)
	{
		global::GameRecord gameRecord = new global::GameRecord();
		gameRecord.gameGuid = record.GameGuid;
		gameRecord.playerId = record.PlayerId.ToString();
		gameRecord.playerName = record.Nickname;
		gameRecord.character = context.FullIdToCTAsset<PlayerCharacterDef>(record.CharacterId);
		gameRecord.contract = new Contract();
		gameRecord.contract.contractDef = context.FullIdToCTAsset<ContractDef>(record.ContractId);
		gameRecord.contract.difficultyLevel = record.Difficulty.ToClient();
		gameRecord.success = record.Success;
		gameRecord.progress = record.FinalProgress;
		gameRecord.stage = record.FinalStage;
		gameRecord.gameTime = record.Duration.Ticks;
		gameRecord.exp = record.ExpBonus;
		gameRecord.expChangeRecords = (from c in record.ExpChangeRecords.OrEmptyIfNull()
			select c.ToClient(context)).ToList();
		gameRecord.gameAchievedAchievements = record.FinishAchievementIds.OrEmptyIfNull().Select(context.FullIdToCTAsset<AchievementDef>).ToList();
		gameRecord.rankScore = record.RankScore;
		gameRecord.finishTime = record.FinishDateTime;
		gameRecord.deck = (from c in record.Deck.OrEmptyIfNull()
			select c.ToClient(context)).ToList();
		gameRecord.influences = (from c in record.Influences.OrEmptyIfNull()
			select c.ToClient(context)).ToList();
		gameRecord.skillInfos = (from c in record.SkillRecords.OrEmptyIfNull()
			select c.ToClient(context)).ToList();
		gameRecord.gameJournal = new GameJournal();
		gameRecord.gameJournal.records = (from jc in record.Journals.OrEmptyIfNull()
			select jc.ToClient()).ToList();
		gameRecord.mostDamageEnemy = record.MostDamageEnemy?.ToClient(context);
		gameRecord.mostDamageObjects = (from c in record.MostDamageSources.OrEmptyIfNull()
			select c.ToClient(context)).ToList();
		return gameRecord;
	}

	public static GameRecord ToServer(this global::GameRecord record, NetworkConvertContext context)
	{
		GameRecord gameRecord = new GameRecord();
		gameRecord.GameGuid = record.gameGuid;
		gameRecord.PlayerId = Convert.ToInt64(record.playerId);
		gameRecord.Nickname = record.playerName;
		gameRecord.CharacterId = record.character.FullID;
		gameRecord.ContractId = record.contract.contractDef.FullID;
		gameRecord.Success = record.success;
		gameRecord.FinalProgress = record.progress;
		gameRecord.FinalStage = record.stage;
		gameRecord.Difficulty = record.contract.difficultyLevel.ToServer();
		gameRecord.Duration = TimeSpan.FromTicks(record.gameTime);
		gameRecord.ExpBonus = record.exp;
		gameRecord.ExpChangeRecords = record.expChangeRecords.Select((global::GameRecord.ExpChangeRecord c) => c.ToServer()).ToList();
		gameRecord.RankScore = record.rankScore;
		DailyChallengeRunContext dailyChallengeContext = record.dailyChallengeContext;
		if (dailyChallengeContext != null && dailyChallengeContext.isDailyChallenge)
		{
			gameRecord.RankScore = 0;
			gameRecord.DailyChallengeGameRecord = DailyChallengeRequestMapper.ToGameRecord(record);
		}
		gameRecord.InGameMatterRewards = record.inGameMatterRewards.Select((global::MatterInfo c) => c.ToServer()).ToList();
		gameRecord.EnteredNodePoint = record.enteredNodePoint;
		gameRecord.Deck = record.deck.Select((global::GameRecord.ItemRecord c) => c.ToServer()).ToList();
		gameRecord.Influences = record.influences.Select((global::GameRecord.InfluenceRecord c) => c.ToServer()).ToList();
		gameRecord.SkillRecords = record.skillInfos.Select((global::GameRecord.SkillRecord c) => c.ToServer(context)).ToList();
		gameRecord.Journals = record.gameJournal.records.Select((global::GameJournalRecord jc) => jc.ToServer()).ToList();
		gameRecord.DiscoveredDefIds = record.discoveredObjects.Select((ICTAsset c) => c.GetFullID()).ToList();
		gameRecord.FinishAchievementIds = record.gameAchievedAchievements.Select((AchievementDef c) => c.FullID).ToList();
		gameRecord.MostDamageEnemy = record.mostDamageEnemy?.ToServer();
		gameRecord.MostDamageSources = record.mostDamageObjects.Select((global::GameRecord.ObjectValueRecord c) => c.ToServer()).ToList();
		gameRecord.FinishDateTime = record.finishTime;
		gameRecord.PvpChanllengeGameRecord = record.pvpChallengeGameRecord?.ToServer();
		gameRecord.GemConfig = record.gemConfigs;
		gameRecord.ATCTamperCount = record.cheatTamperCount;
		gameRecord.ATCMismatchReadCount = record.cheatMismatchReadCount;
		if (record.BossBlessStatistics != null && record.BossBlessStatistics.Count > 0)
		{
			gameRecord.BossBlessStatistics = (from statistic in record.BossBlessStatistics
				where statistic != null
				select new BossBlessStatisticItem
				{
					BlessId = statistic.BlessId,
					ExposureCount = statistic.ExposureCount,
					PickCount = statistic.PickCount
				}).ToList();
		}
		return gameRecord;
	}

	public static MatterInfo ToServer(this global::MatterInfo matterInfo)
	{
		return new MatterInfo
		{
			MatterId = matterInfo.matterDef.FullID,
			Amount = matterInfo.count
		};
	}

	public static CardRecord ToServer(this global::GameRecord.ItemRecord itemRecord)
	{
		return new CardRecord
		{
			Id = itemRecord.itemDef.FullID,
			CardSkinId = itemRecord.skinDef?.FullID,
			Level = itemRecord.upgradeLevel,
			Counter = itemRecord.counter,
			MaxCounter = itemRecord.maxCounter,
			Runes = itemRecord.runes.Select((global::GameRecord.RuneRecord r) => r.ToServer()).ToList()
		};
	}

	public static ExpChangeRecord ToServer(this global::GameRecord.ExpChangeRecord record)
	{
		ExpChangeRecord expChangeRecord = new ExpChangeRecord();
		expChangeRecord.ChangeType = (ExpChangeType)record.source;
		expChangeRecord.BaseExpBonus = record.baseExpBonus;
		expChangeRecord.BasePercentExpBonus = record.basePercentExpBonus;
		expChangeRecord.FixAddExpBonus = record.fixAddExpBonus;
		expChangeRecord.SumPercentExpBonus = record.sumPercentExpBonus;
		expChangeRecord.Progress = record.progress;
		if (record.difficulty != null)
		{
			expChangeRecord.DifficultyLevel = (DifficultyLevel)record.difficulty.difficultyLevel;
		}
		if (record.achievement != null)
		{
			expChangeRecord.AchievementId = record.achievement.FullID;
		}
		expChangeRecord.DailyMinTimes = record.gameRange.x;
		expChangeRecord.DailyMaxTimes = record.gameRange.y;
		return expChangeRecord;
	}

	public static global::GameRecord.ItemRecord ToClient(this CardRecord cardRecord, NetworkConvertContext context)
	{
		return new global::GameRecord.ItemRecord
		{
			itemDef = context.FullIdToCTAsset<ItemDefinition>(cardRecord.Id),
			skinDef = context.FullIdToCTAsset<ItemSkinDef>(cardRecord.CardSkinId),
			upgradeLevel = cardRecord.Level,
			counter = cardRecord.Counter,
			maxCounter = cardRecord.MaxCounter,
			runes = cardRecord.Runes.Select((RuneRecord r) => r.ToClient(context)).ToList()
		};
	}

	public static global::GameRecord.RuneRecord ToClient(this RuneRecord runeRecord, NetworkConvertContext context)
	{
		return new global::GameRecord.RuneRecord
		{
			def = context.FullIdToCTAsset<RuneDef>(runeRecord.Id),
			value = runeRecord.Stack
		};
	}

	public static RuneRecord ToServer(this global::GameRecord.RuneRecord runeRecord)
	{
		return new RuneRecord
		{
			Id = runeRecord.def.FullID,
			Stack = runeRecord.value
		};
	}

	public static global::GameRecord.InfluenceRecord ToClient(this InfluenceRecord influenceRecord, NetworkConvertContext context)
	{
		return new global::GameRecord.InfluenceRecord
		{
			def = context.FullIdToCTAsset<InfluenceDef>(influenceRecord.Id),
			influenceValue = influenceRecord.Value
		};
	}

	public static InfluenceRecord ToServer(this global::GameRecord.InfluenceRecord influenceRecord)
	{
		return new InfluenceRecord
		{
			Id = influenceRecord.def.FullID,
			Value = influenceRecord.influenceValue
		};
	}

	public static global::GameRecord.SkillRecord ToClient(this SkillRecord skillRecord, NetworkConvertContext context)
	{
		return new global::GameRecord.SkillRecord
		{
			skill = context.FullIdToCTAsset<SkillDef>(skillRecord.Id),
			archetypeIndex = skillRecord.SkillArchetypeIndex,
			durability = skillRecord.Durability,
			activated = skillRecord.Activated,
			consumed = skillRecord.Consumed,
			hexShape = skillRecord.HexShape.ToClient(),
			skillGems = skillRecord.SkillGems.Select((SkillGem g) => g.ToClient(context)).ToList()
		};
	}

	public static SkillRecord ToServer(this global::GameRecord.SkillRecord skillRecord, NetworkConvertContext context)
	{
		return new SkillRecord
		{
			Id = skillRecord.skill.FullID,
			SkillArchetypeIndex = skillRecord.archetypeIndex,
			Durability = skillRecord.durability,
			Activated = skillRecord.activated,
			Consumed = skillRecord.consumed,
			HexShape = skillRecord.hexShape.ToServer(),
			SkillGems = skillRecord.skillGems.Select((global::SkillGem g) => g.ToServer()).ToList()
		};
	}

	public static HexShape ToClient(this HexShapeSvr hexShape)
	{
		if (hexShape == null)
		{
			return null;
		}
		return new HexShape
		{
			offset = hexShape.Offset.ToClient(),
			points = hexShape.Points.Select((Vector2IntSvr p) => p.ToClient()).ToList()
		};
	}

	public static HexShapeSvr ToServer(this HexShape hexShape)
	{
		if (hexShape == null)
		{
			return null;
		}
		return new HexShapeSvr
		{
			Offset = hexShape.offset.ToServer(),
			Points = hexShape.points.Select((Vector2Int p) => p.ToServer()).ToList()
		};
	}

	public static Vector2Int ToClient(this Vector2IntSvr vector2Int)
	{
		return new Vector2Int
		{
			x = vector2Int.X,
			y = vector2Int.Y
		};
	}

	public static Vector2IntSvr ToServer(this Vector2Int vector2Int)
	{
		return new Vector2IntSvr
		{
			X = vector2Int.x,
			Y = vector2Int.y
		};
	}

	public static GameJournalRecord ToServer(this global::GameJournalRecord journalRecord)
	{
		return new GameJournalRecord
		{
			RecordTime = TimeSpan.FromTicks(journalRecord.time),
			RecordType = journalRecord.GetType().FullName,
			RecordArg0 = journalRecord.NetworkSaveArg0,
			RecordArg1 = journalRecord.NetworkSaveArg1
		};
	}

	public static global::GameJournalRecord ToClient(this GameJournalRecord journalRecord)
	{
		Type type = Type.GetType(journalRecord.RecordType);
		if (type == null)
		{
			Debug.LogError("无法找到类型" + journalRecord.RecordType);
			return null;
		}
		global::GameJournalRecord obj = Activator.CreateInstance(type, nonPublic: true) as global::GameJournalRecord;
		obj.time = journalRecord.RecordTime.Ticks;
		obj.NetworkSaveArg0 = journalRecord.RecordArg0;
		obj.NetworkSaveArg1 = journalRecord.RecordArg1;
		return obj;
	}

	public static GlobalGameRecordManager.GameRecordPreviewInfo ToClient(this GameRecordBrief brief, NetworkConvertContext context)
	{
		GlobalGameRecordManager.GameRecordPreviewInfo obj = new GlobalGameRecordManager.GameRecordPreviewInfo
		{
			gameGuid = brief.GameRecordGuid,
			playerName = brief.PlayerNickName,
			character = context.FullIdToCTAsset<PlayerCharacterDef>(brief.CharacterId)
		};
		obj.avatar = obj.CharacterRecordIcon;
		obj.success = brief.Success;
		obj.contract = new Contract();
		obj.contract.contractDef = context.FullIdToCTAsset<ContractDef>(brief.ContractId);
		obj.contract.difficultyLevel = brief.DifficultyLevel.ToClient();
		obj.progress = brief.Progress;
		obj.gameTime = brief.Duration.Ticks;
		obj.rankingScore = brief.RankScore;
		obj.finishDateTime = brief.FinsihDateTime.Ticks;
		obj.pvpChanllengeRecord = brief.PvpChanllengeGameRecord?.ToClient(context);
		return obj;
	}

	public static RoutineQuest ToClient(this RoutineQuestState quest, NetworkConvertContext context)
	{
		RoutineQuestDef routineQuestDef = context.FullIdToCTAsset<RoutineQuestDef>(quest.QuestDefID);
		if (routineQuestDef == null)
		{
			return null;
		}
		RoutineQuest routineQuest = new RoutineQuest(routineQuestDef, context.applicationState);
		routineQuest.guid = quest.Guid;
		routineQuest.progress = quest.ProgressPerLayer;
		routineQuest.gotRewardLayer = quest.LayerFinished;
		if (!quest.QuestTargetDefID.IsNullOrEmpty())
		{
			CTScriptableObject cTScriptableObject = context.FullIdToCTAsset<CTScriptableObject>(quest.QuestTargetDefID);
			if (cTScriptableObject is SkillDef skill)
			{
				routineQuest.skill = skill;
			}
			else if (cTScriptableObject is PlayerCharacterDef character)
			{
				routineQuest.character = character;
			}
		}
		return routineQuest;
	}

	public static global::ContractState ToClient(this ContractState contractState, NetworkConvertContext context)
	{
		ContractDef contractDef = context.FullIdToCTAsset<ContractDef>(contractState.Id);
		if (contractDef == null)
		{
			return null;
		}
		return new global::ContractState(contractDef)
		{
			finishedThisCycle = contractState.FinishedThisCycle,
			finishedMaxDifficultyThisCycle = contractState.FinishedMaxDifficultyThisCycle.ToClient(),
			lastFinishedDataTimeTicks = (contractState.LastFinishTime?.Ticks ?? (-1)),
			lastFinishedSeason = context.FullIdToCTAsset<SeasonDef>(contractState.LastFinishSeason)
		};
	}

	public static Achievement ToClient(this AchievementState achievementState, NetworkConvertContext context)
	{
		AchievementDef achievementDef = context.FullIdToCTAsset<AchievementDef>(achievementState.Id);
		if (achievementDef == null)
		{
			return null;
		}
		return new Achievement(achievementDef)
		{
			achieved = achievementState.Finished,
			rewardReceived = achievementState.RewardGot,
			finishedLocalDate = achievementState.FinishedLocalDate
		};
	}

	public static DailyCheckInResult ToClient(this TriggerDailyCheckInResult checkInResult, NetworkConvertContext context)
	{
		return new DailyCheckInResult
		{
			festivalCheckInResult = checkInResult.FestivalCheckInResult?.ToClient(context),
			monthCheckInResult = checkInResult.MonthDailyCheckInRewards?.ToClient(context),
			monthCardCheckInResult = checkInResult.MonthlyCardCheckInResult?.ToClient(context),
			forerunnerShareRewardResult = checkInResult.ForerunnerLegacyRewardResult
		};
	}

	public static global::FestivalCheckInResult ToClient(this FestivalCheckInResult checkInResult, NetworkConvertContext context)
	{
		return new global::FestivalCheckInResult
		{
			festivalDef = context.FullIdToCTAsset<FestivalDef>(checkInResult.FestivalId),
			alreadyCheckInCount = checkInResult.CurrentCheckInCount,
			reward = checkInResult.FestivalCheckInReward.ToClient(context)
		};
	}

	public static global::MonthCheckInResult ToClient(this MonthCheckInResult checkInResult, NetworkConvertContext context)
	{
		return new global::MonthCheckInResult
		{
			checkInDaysThisMonth = checkInResult.CheckInDaysInThisMonth,
			reward = checkInResult.MonthDailyCheckInReward.ToClient(context)
		};
	}

	public static global::MonthCardCheckInResult ToClient(this MonthCardCheckInResult checkInResult, NetworkConvertContext context)
	{
		return new global::MonthCardCheckInResult
		{
			reward = checkInResult.MonthCardCheckInReward.ToClient(context),
			checkInDoubleReward = checkInResult.MonthCardCheckInDoubleReward.ToClient(context)
		};
	}

	public static ForerunnerLegacy ToServer(this global::ForerunnerLegacy legacy, NetworkConvertContext context)
	{
		return new ForerunnerLegacy
		{
			Guid = legacy.guid,
			PlayerId = legacy.sourcePlayerId,
			LegacyItem = legacy.legacyItem?.ToServer(),
			LegacySkill = legacy.legacySkill?.ToServer(context),
			LegacyMapID = legacy.sourceDraftMap?.CTAssetId,
			Stage = legacy.stage,
			LocationPivotIndex = legacy.locationPivotIndex,
			ExpireTime = legacy.expireTime,
			ShareEncounterCount = legacy.shaderEncounterCount,
			ShareGetCount = legacy.shareGetCount,
			ShareWinCount = legacy.shareWinCount,
			UpvoteCount = legacy.upvoteCount,
			DownvoteCount = legacy.downvoteCount,
			Note = legacy.note,
			PresetMessageId = legacy.presetMessageId,
			Signature = legacy.signature
		};
	}

	public static global::ForerunnerLegacy ToClient(this ForerunnerLegacy legacy, NetworkConvertContext context)
	{
		global::ForerunnerLegacy obj = new global::ForerunnerLegacy
		{
			guid = legacy.Guid,
			sourcePlayerId = legacy.PlayerId,
			legacyItem = legacy.LegacyItem?.ToClient(context),
			legacySkill = legacy.LegacySkill?.ToClient(context)
		};
		DraftMapInfoData draftMapInfoData = ((legacy.LegacyMapID == null) ? null : context.FullIdToCTAsset<DraftMapInfoData>(legacy.LegacyMapID));
		obj.sourceDraftMap = ((draftMapInfoData != null) ? new DraftMapRef(draftMapInfoData) : null);
		obj.stage = legacy.Stage;
		obj.locationPivotIndex = legacy.LocationPivotIndex;
		obj.leaveTime = legacy.LeaveTime;
		obj.expireTime = legacy.ExpireTime;
		obj.shaderEncounterCount = legacy.ShareEncounterCount;
		obj.shareGetCount = legacy.ShareGetCount;
		obj.shareWinCount = legacy.ShareWinCount;
		obj.upvoteCount = legacy.UpvoteCount;
		obj.downvoteCount = legacy.DownvoteCount;
		obj.note = legacy.Note;
		obj.presetMessageId = legacy.PresetMessageId;
		obj.signature = legacy.Signature;
		return obj;
	}

	public static AssetValueRecord ToServer(this global::GameRecord.ObjectValueRecord record)
	{
		return new AssetValueRecord
		{
			AssetId = record.target.GetFullID(),
			Value = record.value
		};
	}

	public static global::GameRecord.ObjectValueRecord ToClient(this AssetValueRecord record, NetworkConvertContext context)
	{
		return new global::GameRecord.ObjectValueRecord(context.FullIdToCTAsset<ICTAsset>(record.AssetId), record.Value);
	}

	public static global::PurchaseRecord ToClient(this PurchaseRecord record, NetworkConvertContext context)
	{
		return new global::PurchaseRecord
		{
			orderId = record.OrderId.ToString(),
			purchaseDateTime = record.PurchaseTime,
			quantity = record.Quantity,
			orderStatus = record.OrderStatus
		};
	}

	public static RankingManager.RankingData ToClient(this RankGameRecord rankGameRecord, NetworkConvertContext context)
	{
		return new RankingManager.RankingData
		{
			rankIndex = rankGameRecord.RankIndex,
			recordPreviewInfo = rankGameRecord.GameRecordBrief.ToClient(context)
		};
	}

	public static NewGameConfigData.GeneratePrepareConfig ToClient(this StartGameInfo startGameInfo, NetworkConvertContext context)
	{
		NewGameConfigData.GeneratePrepareConfig generatePrepareConfig = new NewGameConfigData.GeneratePrepareConfig();
		generatePrepareConfig.gameGuid = startGameInfo.GameGuid;
		generatePrepareConfig.gameRandomSeed = (uint)startGameInfo.GameRandomSeed;
		generatePrepareConfig.forerunnerLegacyThisGame = startGameInfo.ForeRunnerLegacyThisGame.Select((ForerunnerLegacy s) => s.ToClient(context)).ToList();
		generatePrepareConfig.pvpDefenderThisGame = startGameInfo.PvpDefenderThisGame?.Select((ModelPVPDefender s) => s.ToClient(context)).ToList();
		generatePrepareConfig.SetBossBlessWeightSnapshot((startGameInfo.BossBlessWeightSnapshot == null) ? null : new global::BossBlessWeightSnapshot
		{
			StatDate = startGameInfo.BossBlessWeightSnapshot.StatDate,
			Weights = ((startGameInfo.BossBlessWeightSnapshot.Weights == null) ? null : new Dictionary<string, double>(startGameInfo.BossBlessWeightSnapshot.Weights))
		});
		return generatePrepareConfig;
	}

	public static PVPDefender ToClient(this ModelPVPDefender defender, NetworkConvertContext context)
	{
		PVPDefender pVPDefender = new PVPDefender();
		pVPDefender.guid = defender.Id.ToString();
		pVPDefender.sourcePlayerId = defender.SourcePlayerId.ToString();
		pVPDefender.sourcePlayerName = defender.SourcePlayerNickName;
		pVPDefender.defenderName = defender.DefenderName;
		pVPDefender.remainGetMatterCount = defender.RemainGetBonusMatterCount;
		pVPDefender.creationTime = defender.CreationTime;
		EnemyInfoData enemyInfoData = context.FullIdToCTAsset<EnemyInfoData>(defender.AvatarId);
		if (enemyInfoData != null)
		{
			pVPDefender.enemyAvatarRef = new EnemyAssetRef(enemyInfoData);
		}
		pVPDefender.deckPreset = ((defender.DeckPreset != null) ? defender.DeckPreset.ToClient(context) : new DeckPreset());
		pVPDefender.battleRecords = ((defender.BattleRecords != null) ? defender.BattleRecords.Select((PVPDefenderBattleRecord r) => r.ToClient(context)).WhereNotNull().ToList() : new List<global::PVPDefenderBattleRecord>());
		return pVPDefender;
	}

	public static DeckPreset ToClient(this PVPDefenderDeckPreset deckPreset, NetworkConvertContext context)
	{
		DeckPreset deckPreset2 = new DeckPreset();
		if (deckPreset.Comps != null)
		{
			deckPreset2.comps = deckPreset.Comps.Select((PVPDefenderDeckComp c) => c.ToClient(context)).WhereNotNull().ToList();
		}
		return deckPreset2;
	}

	public static DeckPresetComp ToClient(this PVPDefenderDeckComp comp, NetworkConvertContext context)
	{
		if (comp.CompDefId.IsNullOrEmpty())
		{
			return null;
		}
		DeckPresetCompDef deckPresetCompDef = null;
		ItemDefinition itemDefinition = context.FullIdToCTAsset<ItemDefinition>(comp.CompDefId);
		if (itemDefinition != null)
		{
			deckPresetCompDef = new DeckPresetCompDef(itemDefinition);
		}
		else
		{
			RuneDef runeDef = context.FullIdToCTAsset<RuneDef>(comp.CompDefId);
			if (runeDef != null)
			{
				deckPresetCompDef = new DeckPresetCompDef(runeDef);
			}
			else
			{
				InfluenceDef influenceDef = context.FullIdToCTAsset<InfluenceDef>(comp.CompDefId);
				if (influenceDef != null)
				{
					deckPresetCompDef = new DeckPresetCompDef(influenceDef);
				}
			}
		}
		if (deckPresetCompDef == null)
		{
			return null;
		}
		DeckPresetComp deckPresetComp = new DeckPresetComp(deckPresetCompDef);
		if (comp.SubComps != null)
		{
			deckPresetComp.subComps = comp.SubComps.Select((PVPDefenderDeckComp sc) => sc.ToClient(context)).WhereNotNull().ToList();
		}
		return deckPresetComp;
	}

	public static global::PVPDefenderBattleRecord ToClient(this PVPDefenderBattleRecord record, NetworkConvertContext context)
	{
		return new global::PVPDefenderBattleRecord
		{
			defenderPresetGuid = record.PvpDefenderGuid,
			gameRecordId = record.GameRecordGuid,
			dealtDamageToChallenger = record.DealtDamageToChallenger,
			challengerFinishType = (FinishBattleEncounterAction.FinishType)record.ChallengerBattleFinishType,
			challengePlayerId = record.ChallengerPlayerId.ToString(),
			challengePlayerNickName = record.ChallengerPlayerNickName,
			defenderPlayerId = record.DefenderSourcePlayerId.ToString(),
			defenderName = record.DefenderName,
			defenderPlayerNickName = record.DefenderSourcePlayerNickName,
			defenderAvatarId = record.DefenderAvatarId,
			battleDateTime = new UnityDateTime(record.BattleEndTime)
		};
	}

	public static global::PVPChanllengeGameRecord ToClient(this PVPChanllengeGameRecord record, NetworkConvertContext context)
	{
		if (record == null)
		{
			return null;
		}
		global::PVPChanllengeGameRecord pVPChanllengeGameRecord = new global::PVPChanllengeGameRecord(record.GameGuid);
		if (record.DefenderBattleRecords != null)
		{
			pVPChanllengeGameRecord.defenderBattleRecords = record.DefenderBattleRecords.Select((PVPDefenderBattleRecord r) => r.ToClient(context)).ToList();
		}
		return pVPChanllengeGameRecord;
	}

	public static WeekCheckInState ToClient(this FestivalCheckInState festivalStateInfo, NetworkConvertContext context)
	{
		FestivalDef festivalDef = context.FullIdToCTAsset<FestivalDef>(festivalStateInfo.Id);
		if (festivalDef == null)
		{
			TraceLog.Warn("[DailyCheck] 服务端签到场次在本地无配置, FestivalId=" + festivalStateInfo.Id);
			return null;
		}
		return new WeekCheckInState(festivalDef)
		{
			checkInDays = festivalStateInfo.CheckInCount
		};
	}

	public static global::PVPDefenderRankData ToClient(this PVPDefenderRankData rankData, NetworkConvertContext context)
	{
		return new global::PVPDefenderRankData
		{
			index = rankData.Index,
			defenderGuid = rankData.DefenderGuid.ToString(),
			defenderNickName = rankData.DefenderNickName,
			defenderAvatar = context.FullIdToCTAsset<EnemyInfoData>(rankData.DefenderAvatarId),
			ownerPlayerId = rankData.OwnerPlayerId,
			ownerPlayerNickName = rankData.OwnerPlayerNickName,
			rankScore = rankData.RankScore
		};
	}

	public static StartGameBuff ToServer(this global::StartGameBuff buff)
	{
		return new StartGameBuff
		{
			StartGameBuffDefId = buff.def.FullID
		};
	}

	public static ModelPVPDefender ToServer(this PVPDefender defender)
	{
		ModelPVPDefender modelPVPDefender = new ModelPVPDefender();
		modelPVPDefender.Id = Guid.Parse(defender.guid);
		modelPVPDefender.DefenderName = defender.defenderName;
		modelPVPDefender.AvatarId = defender.enemyAvatarRef.CTAssetId;
		modelPVPDefender.RemainGetBonusMatterCount = defender.remainGetMatterCount;
		modelPVPDefender.DeckPreset = defender.deckPreset?.ToServer();
		modelPVPDefender.CreationTime = defender.creationTime;
		modelPVPDefender.SourcePlayerId = long.Parse(defender.sourcePlayerId);
		modelPVPDefender.SourcePlayerNickName = defender.sourcePlayerName;
		if (defender.battleRecords != null && defender.battleRecords.Count > 0)
		{
			modelPVPDefender.BattleRecords = defender.battleRecords.Select((global::PVPDefenderBattleRecord r) => r.ToServer()).ToList();
		}
		return modelPVPDefender;
	}

	public static PVPDefenderBattleRecord ToServer(this global::PVPDefenderBattleRecord record)
	{
		PVPDefenderBattleRecord obj = new PVPDefenderBattleRecord
		{
			PvpDefenderGuid = record.defenderPresetGuid,
			GameRecordGuid = record.gameRecordId,
			DealtDamageToChallenger = record.dealtDamageToChallenger,
			ChallengerBattleFinishType = (BattleFinishType)record.challengerFinishType
		};
		long result = 0L;
		if (!string.IsNullOrEmpty(record.challengePlayerId))
		{
			long.TryParse(record.challengePlayerId, out result);
		}
		obj.ChallengerPlayerId = result;
		obj.ChallengerPlayerNickName = record.challengePlayerNickName;
		obj.DefenderName = record.defenderName;
		obj.DefenderAvatarId = record.defenderAvatarId;
		long result2 = 0L;
		if (!string.IsNullOrEmpty(record.defenderPlayerId))
		{
			long.TryParse(record.defenderPlayerId, out result2);
		}
		obj.DefenderSourcePlayerId = result2;
		obj.DefenderSourcePlayerNickName = record.defenderPlayerNickName;
		obj.BattleEndTime = record.battleDateTime.UtcDateTime;
		return obj;
	}

	public static PVPChanllengeGameRecord ToServer(this global::PVPChanllengeGameRecord record)
	{
		if (record == null)
		{
			return null;
		}
		return new PVPChanllengeGameRecord
		{
			GameGuid = record.gameGuid,
			DefenderBattleRecords = record.defenderBattleRecords?.Select((global::PVPDefenderBattleRecord r) => r.ToServer()).ToList()
		};
	}

	public static PVPDefenderDeckPreset ToServer(this DeckPreset deckPreset)
	{
		PVPDefenderDeckPreset pVPDefenderDeckPreset = new PVPDefenderDeckPreset();
		if (deckPreset.comps != null)
		{
			pVPDefenderDeckPreset.Comps = deckPreset.comps.Select((DeckPresetComp c) => c.ToServer()).WhereNotNull().ToList();
		}
		return pVPDefenderDeckPreset;
	}

	public static PVPDefenderDeckComp ToServer(this DeckPresetComp comp)
	{
		if (comp?.def == null)
		{
			return null;
		}
		PVPDefenderDeckComp pVPDefenderDeckComp = new PVPDefenderDeckComp();
		pVPDefenderDeckComp.CompDefId = comp.def.TargetAssetID;
		if (comp.subComps != null && comp.subComps.Count > 0)
		{
			pVPDefenderDeckComp.SubComps = comp.subComps.Select((DeckPresetComp sc) => sc.ToServer()).WhereNotNull().ToList();
		}
		return pVPDefenderDeckComp;
	}

	public static global::StartGameBuff ToClient(this StartGameBuff buff, NetworkConvertContext context)
	{
		return new global::StartGameBuff(context.FullIdToCTAsset<StartGameBuffDef>(buff.StartGameBuffDefId))
		{
			expireDayPeriod = buff.ExpireDayPeriod
		};
	}

	public static global::GachaPoolState ToClient(this GachaPoolState state, NetworkConvertContext context)
	{
		return new global::GachaPoolState(context.FullIdToCTAsset<GachaPoolDef>(state.GachaPoolId))
		{
			gachaDrawHistoryInfos = (state.DrawRewardHistory?.Select((GachaDrawHistory h) => h.ToClient(context)).WhereNotNull().ToList() ?? new List<GachaHistoryInfo>()),
			wishCharacter = context.FullIdToCTAsset<PlayerCharacterDef>(state.WishTargetId)
		};
	}

	public static GachaHistoryInfo ToClient(this GachaDrawHistory history, NetworkConvertContext context)
	{
		GachaHistoryInfo gachaHistoryInfo = new GachaHistoryInfo();
		gachaHistoryInfo.gachaPoolDef = context.FullIdToCTAsset<GachaPoolDef>(history.GachaPoolId);
		gachaHistoryInfo.contentType = (GachaPoolDef.GachaContentType)history.ContentType;
		gachaHistoryInfo.content = context.FullIdToCTAsset<CTScriptableObject>(history.ContentId);
		gachaHistoryInfo.qualityLevel = (global::QualityLevel)history.ContentQualityLevel;
		gachaHistoryInfo.reward = history.GachaReward.ToClient(context);
		gachaHistoryInfo.reward.SetupGachaDisplayContent(gachaHistoryInfo.contentType, gachaHistoryInfo.content, gachaHistoryInfo.qualityLevel);
		gachaHistoryInfo.wish = history.IsWish;
		gachaHistoryInfo.date = history.DrawDateTime;
		return gachaHistoryInfo;
	}

	public static void ApplyProfileData(this ApplicationStateInfo applicationState, PlayerProfileData profileData)
	{
		LimitedCharacterDeploymentService.LocalOneWayProgress localProgress = LimitedCharacterDeploymentService.CaptureLocalOneWayProgress(applicationState);
		NetworkConvertContext context = new NetworkConvertContext(applicationState);
		applicationState.playerInfoManager.nickName = profileData.NickName;
		applicationState.playerInfoManager.vip = profileData.Vip;
		applicationState.playerInfoManager.dailyObsessionDebrisRecycleCount = profileData.DailyObsessionDebrisRecycleCount;
		applicationState.playerInfoManager.cheatScore = profileData.CheatScore;
		applicationState.playerInfoManager.monthlyCardExpireDayPeriod = profileData.MonthlyCardExpireDayPeriod;
		applicationState.matterManager.matterInfos = profileData.MatterInfos.Select((MatterInfo m) => m.ToClient(context)).WhereNotNull().ToList();
		applicationState.globalSkillManager.allSkills = profileData.SkillPool.Select((ProfileSkill s) => s.ToClient(context)).WhereNotNull().ToList();
		profileData.FavoriteSkillIds.ForEach(delegate(string fid)
		{
			global::ProfileSkill profileSkill = applicationState.globalSkillManager.allSkills.FirstOrDefault((global::ProfileSkill s) => s.profileSkillGUID == fid);
			if (profileSkill != null)
			{
				profileSkill.isFavorite = true;
			}
		});
		applicationState.globalSkillManager.pickUpgradableSkillWords = profileData.UnlockedSkillWords.Select(context.FullIdToCTAsset<SkillWordDef>).WhereNotNull().ToList();
		applicationState.skillGemManager.allSkillGems = profileData.SkillGems.Select((KeyValuePair<string, int> gem) => gem.ToClient(context)).WhereNotNull().ToList();
		applicationState.itemSkinManager.unlockedItemSkinLevels = (from kv in profileData.CardSkinUnlockLevels
			select new ItemSkinManager.ItemSkinState
			{
				skinDef = context.FullIdToCTAsset<ItemSkinDef>(kv.Key),
				level = kv.Value
			} into state
			where state.skinDef != null
			select state).ToList();
		applicationState.itemSkinManager.RebuildCache();
		applicationState.globalUnlockManager.unlockedResource = profileData.UnLockedEntries.Select(context.FullIdToCTAsset<ICTAsset>).WhereNotNull().ToHashSet();
		applicationState.globalUnlockManager.gotObject = profileData.GotObject.Select(context.FullIdToCTAsset<ICTAsset>).WhereNotNull().ToList();
		applicationState.globalVariableManager.globalVariables = profileData.GlobalVariables.Select((KeyValuePair<string, int> kv) => new GlobalVariable
		{
			id = kv.Key,
			value = kv.Value
		}).WhereNotNull().ToList();
		LimitedCharacterDeploymentService.RestoreLocalOneWayProgress(applicationState, localProgress);
		applicationState.storyPointManager.storyPointStates = profileData.StoryPointFinishStates.Select((KeyValuePair<string, StoryPointState> ss) => ss.Value.ToClient(context)).WhereNotNull().ToList();
		applicationState.storyPointManager.fortressNPCStates = profileData.FortressNpcStates.Select((FortressNPCState fs) => fs.ToClient(context)).WhereNotNull().ToList();
		applicationState.difficultyManager.finishedMaxDifficulty = profileData.FinishedMaxDiffuclty.ToClient();
		applicationState.globalUnlockManager.globalLevelLine = profileData.GlobalLevelLineInfo.ToClient(context);
		applicationState.characterManager.characterClassStates = profileData.CharacterClassStates.Select((CharacterClassState cs) => cs.ToClient(context)).WhereNotNull().ToList();
		applicationState.characterManager.characterStates = profileData.CharacterStates.Select((PlayerCharacterState cs) => cs.ToClient(context)).WhereNotNull().ToList();
		applicationState.characterTrialManager.SetCompletedCharacterIds(profileData.CompletedCharacterTrialIds);
		applicationState.idolManager.ApplyPlayerData(profileData, context);
		applicationState.newbieFestivalManager.ApplyInfo(profileData.NewbieFestivalInfo);
		Dictionary<GachaPoolDef, global::GachaPoolState> dictionary = applicationState.gachaManager.gachaPoolStates?.Where((global::GachaPoolState state) => state?.def != null).ToDictionary((global::GachaPoolState state) => state.def, (global::GachaPoolState state) => state) ?? new Dictionary<GachaPoolDef, global::GachaPoolState>();
		List<global::GachaPoolState> list = profileData.GachaPoolStates?.Select((GachaPoolState ps) => ps.ToClient(context)).WhereNotNull().ToList() ?? new List<global::GachaPoolState>();
		foreach (global::GachaPoolState item in list)
		{
			if (item.def != null && dictionary.TryGetValue(item.def, out var value))
			{
				item.viewed = value.viewed;
				if (item.gachaDrawHistoryInfos.IsNullOrEmpty())
				{
					item.gachaDrawHistoryInfos = value.gachaDrawHistoryInfos ?? new List<GachaHistoryInfo>();
				}
			}
			item.gachaHistoryReady = false;
		}
		foreach (GachaPoolDef gachaPoolDef in applicationState.assetCatalog.gachaPoolCatalog.ValidAssets)
		{
			if (list.All((global::GachaPoolState state) => state.def != gachaPoolDef))
			{
				global::GachaPoolState gachaPoolState = new global::GachaPoolState(gachaPoolDef);
				if (dictionary.TryGetValue(gachaPoolDef, out var value2))
				{
					gachaPoolState.viewed = value2.viewed;
					gachaPoolState.gachaDrawHistoryInfos = value2.gachaDrawHistoryInfos ?? new List<GachaHistoryInfo>();
				}
				gachaPoolState.gachaHistoryReady = false;
				list.Add(gachaPoolState);
			}
		}
		applicationState.gachaManager.gachaPoolStates = list;
		applicationState.seasonManager.currentSeason = profileData.SeasonInfo.ToClient(context);
		applicationState.seasonManager.gotQuestExpThisWeek = profileData.GotQuestExpThisWeek;
		applicationState.guideManager.finishedGuides = profileData.GuideInfo.FinishedGuideModules;
		applicationState.guideManager.introFinished = profileData.GuideInfo.IntroFinished;
		applicationState.guideManager.ReapplySkipGuideAfterProfileFetch();
		List<(ClavisState, ClavisDef)> source = profileData.ClavisStates.Select((ClavisState state) => (state: state, clavis: context.FullIdToCTAsset<ClavisDef>(state.Id))).ToList();
		List<string> list2 = (from t in source
			where t.clavis == null && !string.IsNullOrEmpty(t.state?.Id)
			select t.state.Id).Distinct().ToList();
		if (list2.Count > 0)
		{
			TraceLog.Warn($"读取存档时发现缺失的钥匙配置，相关状态将被忽略: {list2}");
		}
		applicationState.clavisManager.activetedClavis = (from t in source
			where t.state.Activated
			select t.clavis).WhereNotNull().ToList();
		applicationState.clavisManager.unlockedClavis = (from t in source
			where t.state.Got
			select t.clavis).WhereNotNull().ToList();
		applicationState.routineQuestManager.currentWeekQuests = profileData.WeekRoutineQuestStates.Select((RoutineQuestState q) => q.ToClient(context)).WhereNotNull().ToList();
		applicationState.routineQuestManager.currentDailyQuests = profileData.DailyRoutineQuestStates.Select((RoutineQuestState q) => q.ToClient(context)).WhereNotNull().ToList();
		applicationState.routineQuestManager.currentSeasonQuests = profileData.SeasonRoutineQeustStates.Select((RoutineQuestState q) => q.ToClient(context)).WhereNotNull().ToList();
		applicationState.routineQuestManager.questReplaceCountToday = profileData.ReplaceDailyQuestCountToday;
		applicationState.contractManager.contractStates = profileData.ContractStates.Select((ContractState c) => c.ToClient(context)).WhereNotNull().ToList();
		applicationState.contractManager.activeWeekContractIDs = profileData.CurrentWeekShuffleContracts.ToList();
		applicationState.achievementManager.ApplyProfileAchievements(profileData.AchievementStates.Select((AchievementState a) => a.ToClient(context)).WhereNotNull(), applicationState.savedGameState != null);
		applicationState.forerunnerManager.localForerunnerLegacy = profileData.LocalForerunnerLegacy?.ToClient(context);
		applicationState.forerunnerManager.shareForerunnerLegacy = profileData.SharedForerunnerLegacy?.ToClient(context);
		applicationState.purchaseManager.purchaseGroups = profileData.ActivePurchaseGroups.Select(context.FullIdToCTAsset<PurchaseGroupDef>).WhereNotNull().ToList();
		applicationState.purchaseManager.purchasedOrderHistory = profileData.PurchaseRecords.ToDictionary((KeyValuePair<string, List<PurchaseRecord>> record) => record.Key, (KeyValuePair<string, List<PurchaseRecord>> record) => record.Value.Select((PurchaseRecord p) => p.ToClient(context)).WhereNotNull().ToList());
		applicationState.purchaseManager.limitedTimeBundleInfos = profileData.LimitedTimeBundleInfos?.ToList() ?? new List<LimitedTimeBundleInfo>();
		applicationState.startGameBuffManager.currentStartGameBuffs = profileData.CurrentStartGameBuffs.Select((StartGameBuff s) => s.ToClient(context)).WhereNotNull().ToList();
		applicationState.dailyCheckManager.weekCheckState = profileData.FestivalCheckInState?.ToClient(context);
		applicationState.openTestManager.QuestionnaireBonusGet = profileData.TestQuestionnaireBonusGet;
		applicationState.pvpModeManager.currentDefender = profileData.PvpModeInfo.CurrentDefedner?.ToClient(context);
		if (applicationState.savedGameState != null && (profileData.LastStartGameInfo == null || profileData.LastStartGameInfo.GameGuid != applicationState.savedGameState.GameGUID))
		{
			applicationState.savedGameState = null;
		}
		profileData.StatisticsInfo.ApplyToClient(applicationState.gameStatisticsManager);
	}
}
