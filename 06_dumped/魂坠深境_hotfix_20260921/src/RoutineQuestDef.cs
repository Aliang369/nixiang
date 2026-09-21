using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoutineQuestDef : CTScriptableObject
{
	public enum QuestType
	{
		DailyQuest,
		WeeklyQuest,
		SeasonQuest,
		NewbieFestivalQuest
	}

	public enum QuestExtraInfoType
	{
		None,
		RandomOwnedCharacter,
		RandomOwnedSkill
	}

	public abstract class RoutineQuestNode
	{
		public enum JumpPanelType
		{
			None,
			StartGame,
			SkillUpgrade,
			StarMap,
			GachaDraw,
			CombineGem,
			CharacterInfo,
			SkillGachaDraw
		}

		public virtual JumpPanelType JumpToPanelType => JumpPanelType.None;

		protected virtual bool IsNeedCache => false;

		public virtual QuestExtraInfoType ExtraInfoType => QuestExtraInfoType.None;

		public abstract void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState);

		public virtual string QuestTargetText(RoutineQuest quest)
		{
			return quest.character?.characterName ?? quest.skill?.skillName ?? ScriptableObjectSingleton<TextDataPreset>.instance.term_unknown_question_mark;
		}

		public void AddProgress(int progress, RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			if (!applicationState.routineQuestManager.currentDailyQuests.Contains(quest) && !applicationState.routineQuestManager.currentWeekQuests.Contains(quest) && !applicationState.routineQuestManager.currentSeasonQuests.Contains(quest))
			{
				List<RoutineQuest> currentDailyTasks = applicationState.newbieFestivalManager.currentDailyTasks;
				if (currentDailyTasks == null || !currentDailyTasks.Contains(quest))
				{
					return;
				}
			}
			if (!quest.AllLayerFinished)
			{
				if (quest.def.questType == QuestType.NewbieFestivalQuest)
				{
					applicationState.newbieFestivalManager.ApplyTaskUpdate(new RoutineQuestManager.RoutineQuestUpdate
					{
						questGuid = quest.guid,
						addProgress = progress
					});
				}
				else if (IsNeedCache)
				{
					applicationState.routineQuestManager.AddCachedRoutineQuestUpdate(quest, progress);
				}
				else
				{
					applicationState.routineQuestManager.ApplyRoutineQuestUpdate(new RoutineQuestManager.RoutineQuestUpdate
					{
						questGuid = quest.guid,
						addProgress = progress
					});
				}
			}
		}
	}

	public class DailyCheckIn : RoutineQuestNode
	{
		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.dailyCheckInEvent.AddListener(delegate(DailyCheckInResult result)
			{
				if (result.monthCheckInResult != null || IsFixedSingleStepDailyLoginQuest(quest, applicationState))
				{
					AddProgress(1, quest, applicationState);
				}
			});
		}

		private static bool IsFixedSingleStepDailyLoginQuest(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			if (quest?.def != null && quest.def.questType == QuestType.DailyQuest && quest.def.AllLayerSumProgress == 1)
			{
				List<RoutineQuest> currentDailyQuests = applicationState.routineQuestManager.currentDailyQuests;
				if (currentDailyQuests != null && currentDailyQuests.Contains(quest))
				{
					return ScriptableObjectSingleton<FortressDataManager>.instance.fixedDailyQuests?.Contains(quest.def) ?? false;
				}
			}
			return false;
		}
	}

	public class GameOverGetExp : RoutineQuestNode
	{
		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.afterGameOverEvent.AddListener(delegate(GameRecord r)
			{
				AddProgress(r.exp, quest, applicationState);
			});
		}
	}

	public class AddSkillGemSlot : RoutineQuestNode
	{
		public override JumpPanelType JumpToPanelType => JumpPanelType.SkillUpgrade;

		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.addSkillGemSlotEvent.AddListener(delegate
			{
				AddProgress(1, quest, applicationState);
			});
		}
	}

	public class BreakThroughSkill : RoutineQuestNode
	{
		public override JumpPanelType JumpToPanelType => JumpPanelType.SkillUpgrade;

		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.breakThroughSkillEvent.AddListener(delegate
			{
				AddProgress(1, quest, applicationState);
			});
		}
	}

	public class CombineSkillGem : RoutineQuestNode
	{
		public override JumpPanelType JumpToPanelType => JumpPanelType.CombineGem;

		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.combineGemEvent.AddListener(delegate
			{
				AddProgress(1, quest, applicationState);
			});
		}
	}

	public class ConsumeMatterCount : RoutineQuestNode
	{
		public MatterDef matterDef;

		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.matterChangeEvent.AddListener(delegate(MatterInfo info)
			{
				if (info.matterDef == matterDef && info.count < 0)
				{
					AddProgress(Mathf.Abs(info.count), quest, applicationState);
				}
			});
		}

		public override string QuestTargetText(RoutineQuest quest)
		{
			return matterDef.matterName;
		}
	}

	public class FinishWeeklyChallengeContract : RoutineQuestNode
	{
		public override JumpPanelType JumpToPanelType => JumpPanelType.StarMap;

		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.afterGameOverEvent.AddListener(delegate(GameRecord r)
			{
				if (r.IsDailyChallenge)
				{
					DailyChallengeSettlement dailyChallengeSettlement = r.dailyChallengeSettlement;
					if (dailyChallengeSettlement != null && dailyChallengeSettlement.challengeCompleted)
					{
						AddProgress(1, quest, applicationState);
					}
				}
			});
		}
	}

	public class UpgradeCharacterObsessionLevel : RoutineQuestNode
	{
		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.upgradeCharacterObsessionLevelEvent.AddListener(delegate
			{
				AddProgress(1, quest, applicationState);
			});
		}
	}

	public class GachaDrawCount : RoutineQuestNode
	{
		public List<GachaPoolDef.GachaPoolType> poolTypeConstraint;

		public override JumpPanelType JumpToPanelType => JumpPanelType.GachaDraw;

		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.gachaDrawEvent.AddListener(delegate((GachaPoolDef, int) data)
			{
				if (poolTypeConstraint == null || poolTypeConstraint.Contains(data.Item1.poolType))
				{
					AddProgress(data.Item2, quest, applicationState);
				}
			});
		}
	}

	public class PayPurchaseFen : RoutineQuestNode
	{
		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.purchasePayEvent.AddListener(delegate(int fen)
			{
				AddProgress(fen, quest, applicationState);
			});
		}
	}

	public class DealDamageX : RoutineQuestNode
	{
		protected override bool IsNeedCache => true;

		public override JumpPanelType JumpToPanelType => JumpPanelType.StartGame;

		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterDamage, delegate(GameEvent.Type get, GameEvent gameEvent)
			{
				BattleFieldTransform sourceOwnerBft = gameEvent.damage.SourceOwnerBft;
				if (sourceOwnerBft != null && sourceOwnerBft.isPlayer)
				{
					AddProgress(gameEvent.damage.subCounter, quest, applicationState);
				}
			});
		}
	}

	public class OneTurnKill : RoutineQuestNode
	{
		protected override bool IsNeedCache => true;

		public override JumpPanelType JumpToPanelType => JumpPanelType.StartGame;

		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			bool isBeforeFirstPlayerTurnEnd = false;
			applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BattleStart, delegate
			{
				isBeforeFirstPlayerTurnEnd = true;
			});
			applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.PlayerTurnEnd, delegate
			{
				isBeforeFirstPlayerTurnEnd = false;
			});
			applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeBattleEnd, delegate(GameEvent.Type type, GameEvent gameEvent)
			{
				if (isBeforeFirstPlayerTurnEnd && gameEvent.battleFinishType == FinishBattleEncounterAction.FinishType.Win)
				{
					AddProgress(1, quest, applicationState);
				}
				isBeforeFirstPlayerTurnEnd = false;
			});
		}
	}

	public class StartGameWithRandomCharacter : RoutineQuestNode
	{
		public override QuestExtraInfoType ExtraInfoType => QuestExtraInfoType.RandomOwnedCharacter;

		public override JumpPanelType JumpToPanelType => JumpPanelType.StartGame;

		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.BeforeGameStart, delegate(GameEvent.Type get, GameEvent ge)
			{
				if (ge.gameState.playerState.CharacterDef == quest.character)
				{
					AddProgress(1, quest, applicationState);
				}
			});
		}
	}

	public class FinishConstellation : RoutineQuestNode
	{
		public Constellation constellation;

		public override JumpPanelType JumpToPanelType => JumpPanelType.StarMap;

		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.enterFortressPanelEvent.AddListener(delegate
			{
				if ((from s in constellation.ChildrenStars
					select s.presetContractDef into c
					where c != null
					select c).ToList().TrueForAll((ContractDef c) => applicationState.contractManager.IsContractFinished(c)))
				{
					AddProgress(1, quest, applicationState);
				}
			});
		}

		public override string QuestTargetText(RoutineQuest quest)
		{
			return constellation.constellationName;
		}
	}

	public class TalkToFortressNPC : RoutineQuestNode
	{
		public FortressNPC targetNpc;

		public override JumpPanelType JumpToPanelType => JumpPanelType.None;

		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			applicationState.eventManager.talkToFortressNPCEvent.AddListener(delegate(FortressNPC npc)
			{
				if (npc.FullId == targetNpc.FullId)
				{
					AddProgress(1, quest, applicationState);
				}
			});
		}

		public override string QuestTargetText(RoutineQuest quest)
		{
			return targetNpc.npcName;
		}
	}

	public class GlobalObserveQuestNode : RoutineQuestNode
	{
		public GlobalTriggerObserver observer;

		public bool needCache;

		public JumpPanelType jumpPanelType;

		public override JumpPanelType JumpToPanelType => jumpPanelType;

		protected override bool IsNeedCache => needCache;

		public override void InitObserve(RoutineQuest quest, ApplicationStateInfo applicationState)
		{
			observer.Init(applicationState, delegate
			{
				AddProgress(1, quest, applicationState);
			});
		}
	}

	[LocalizeText("任务名称")]
	[CTSOAssetName]
	public string questName;

	[LocalizeText("任务描述")]
	public string description;

	public QuestType questType;

	public List<GlobalRewardSetup> rewards;

	public int maxProgress;

	public int maxLayer = 1;

	public RoutineQuestNode node;

	public List<StoryPoint> generateRequiredStoryPoints;

	public List<ClavisDef> generateRequiredClavis;

	public const string ProgressDesciptionTag = "<Progress>";

	public const string MaxProgressDesciptionTag = "<MaxProgress>";

	public const string AllLayerMaxProgressDesciptionTag = "<AllLayerMaxProgress>";

	public const string TargetDesciptionTag = "<Target>";

	public override string CTAssetIDPrepend => "QUEST";

	public string DescriptionTagHint => "---任务描述文本标记---\n<Progress> 任务当前进度\n<MaxProgress> 任务当前层级最大进度\n<AllLayerMaxProgress> 任务所有层级累积最大进度\n<Target> 任务目标（随机角色，随机护符，素材名称等）\n";

	public int AllLayerSumProgress => maxProgress * maxLayer;

	public bool GenerateAble(ApplicationStateInfo applicationState)
	{
		if (generateRequiredStoryPoints != null && generateRequiredStoryPoints.Any((StoryPoint storyPoint) => storyPoint == null || !storyPoint.IsFinished(applicationState)))
		{
			return false;
		}
		if (generateRequiredClavis != null && generateRequiredClavis.Any((ClavisDef clavis) => clavis == null || applicationState.clavisManager == null || !applicationState.clavisManager.IsUnlocked(clavis)))
		{
			return false;
		}
		if (node.ExtraInfoType == QuestExtraInfoType.RandomOwnedCharacter && applicationState.globalSkillManager.allSkills.Count == 0)
		{
			return false;
		}
		if (node.ExtraInfoType == QuestExtraInfoType.RandomOwnedSkill && applicationState.characterManager.characterStates.Count((PlayerCharacterState c) => !c.locked) == 0)
		{
			return false;
		}
		return true;
	}
}
