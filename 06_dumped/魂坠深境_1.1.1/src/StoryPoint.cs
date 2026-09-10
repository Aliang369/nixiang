using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using UnityEngine;

public class StoryPoint : CTScriptableObject
{
	public abstract class StoryPointNode : IEffectNode, ITextDisplay, IChildren<IEffectNode>
	{
		public abstract IEnumerator ExecuteCor(ApplicationStateInfo applicationState);

		public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
		{
			return null;
		}

		public virtual List<IEffectNode> GetChildren()
		{
			return null;
		}
	}

	public class SequenceNode : StoryPointNode
	{
		public List<StoryPointNode> nodes;

		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			foreach (StoryPointNode node in nodes)
			{
				yield return node.ExecuteCor(applicationState);
			}
		}

		public override List<IEffectNode> GetChildren()
		{
			return nodes.ConvertAll((Converter<StoryPointNode, IEffectNode>)((StoryPointNode n) => n));
		}
	}

	public class ExecuteAvatarSpeak : StoryPointNode
	{
		public AvatarDialogueSection section;

		public AvatarTalkData avatarTalkData;

		public bool lockAction;

		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			AvatarDialogueSection avatarDialogueSection = avatarTalkData?.section ?? section;
			if (applicationState.runningGameState != null)
			{
				if (lockAction)
				{
					applicationState.runningGameState.actionManager.AcquireLock(ActionLock.Type.StoryAvatarSpeak);
				}
				yield return applicationState.runningGameState.UIManager.avatarDialoguePanel.PlayCurrentDialogueSectionCor(avatarDialogueSection);
				if (lockAction)
				{
					applicationState.runningGameState.actionManager.ReleaseLock(ActionLock.Type.StoryAvatarSpeak);
				}
			}
			else if (applicationState.fortressState != null)
			{
				yield return applicationState.fortressState.uiManager.avatarDialoguePanel.PlayCurrentDialogueSectionCor(avatarDialogueSection);
			}
		}
	}

	public class PlayConfigSkillGuide : StoryPointNode
	{
		public AvatarTalkData normalTalkData;

		public AvatarTalkData configuredTalkData;

		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			bool hasConfiguredSkill = HasConfiguredSkill(applicationState);
			AvatarDialogueSection avatarDialogueSection = (hasConfiguredSkill ? configuredTalkData : normalTalkData)?.section;
			AvatarDialoguePanel avatarDialoguePanel = applicationState.runningGameState?.UIManager?.avatarDialoguePanel ?? applicationState.fortressState?.uiManager?.avatarDialoguePanel;
			if (avatarDialogueSection != null && avatarDialoguePanel != null)
			{
				yield return avatarDialoguePanel.PlayCurrentDialogueSectionCor(avatarDialogueSection);
			}
			if (!hasConfiguredSkill)
			{
				PlayGuideSequence playGuideSequence = new PlayGuideSequence
				{
					maskGuideType = PlayGuideSequence.MaskGuideType.ConfigSkillGuide
				};
				yield return playGuideSequence.ExecuteCor(applicationState);
			}
		}

		private static bool HasConfiguredSkill(ApplicationStateInfo applicationState)
		{
			PlayerCharacterState obj = (applicationState.newGameConfigManager?.currentNewGameConfig)?.GetSkillConfigCharacterState(applicationState);
			if (obj == null)
			{
				return false;
			}
			return obj.skillPreset?.configSkillSlots?.Any((PresetSkillSlot slot) => slot?.skill != null) == true;
		}
	}

	public class SpawnRoomObjectDisplay : StoryPointNode
	{
		public enum SpawnType
		{
			Major,
			Secondary
		}

		public SpawnType type;

		public RoomObjectDisplay eventDisplayPrefab;

		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			if (applicationState.runningGameState?.currentEncounter != null)
			{
				switch (type)
				{
				case SpawnType.Major:
					applicationState.runningGameState.currentEncounter.SpawnRoomObjectDisplay(eventDisplayPrefab, major: true);
					break;
				case SpawnType.Secondary:
					applicationState.runningGameState.currentEncounter.SpawnRoomObjectDisplay(eventDisplayPrefab, major: false);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			yield break;
		}
	}

	public class ShowClavisHint : StoryPointNode
	{
		public ClavisDef clavisDef;

		public ClavisHint.HintType hintType;

		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			if (applicationState.runningGameState != null)
			{
				applicationState.runningGameState.UIManager.clavisHint.EnqueueShowClavisHint(clavisDef, hintType);
				yield return new WaitUntil(() => !applicationState.runningGameState.UIManager.clavisHint.WithClavisToHint);
			}
			if (applicationState.fortressState != null)
			{
				applicationState.fortressState.uiManager.clavisHint.EnqueueShowClavisHint(clavisDef, hintType);
				yield return new WaitUntil(() => !applicationState.fortressState.uiManager.clavisHint.WithClavisToHint);
			}
		}
	}

	public class OverlayWorldEvent : StoryPointNode
	{
		public List<NodeBasedWorldEvent.WorldEventChoiceSetup> choiceSetups;

		public RoomObjectDisplay spawnObject;

		[LocalizeText("自定义事件名称")]
		public string worldEventName;

		[LocalizeText("自定义事件描述")]
		[TextArea]
		public string worldEventDescription;

		public bool overWrite;

		public bool overlayBeforeEnterLocation;

		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			GameStateInfo gameState = applicationState.runningGameState;
			if (gameState == null)
			{
				yield break;
			}
			EncounterSetupInfo encounterSetupInfo = ((!overlayBeforeEnterLocation) ? gameState.currentEncounter.setupInfo : (gameState.dungeonMapManager.currentLocation?.attachEncounterSetupInfo ?? gameState.currentEncounter.setupInfo));
			if (encounterSetupInfo == null)
			{
				yield break;
			}
			WorldEvent originWorldEvent = encounterSetupInfo.fixedEventTemplate;
			bool num = gameState.currentEncounter?.triggerEvent != null;
			if (gameState.battleState.isInBattle)
			{
				gameState.actionManager.EnqueueAction(new StopBattleAction(FinishBattleEncounterAction.FinishType.Break));
			}
			NodeBasedWorldEvent we = ScriptableObject.CreateInstance<NodeBasedWorldEvent>();
			we.EventName = worldEventName;
			we.EventDescription = worldEventDescription;
			we.setups = choiceSetups;
			we.eventDisplayPrefab = originWorldEvent.eventDisplayPrefab;
			we.secondaryEventDisplayPrefab = spawnObject;
			we.stayRoomAfterFinish = !overWrite;
			encounterSetupInfo.fixedEventTemplate = we;
			if (gameState.dungeonMapManager.currentLocation != null)
			{
				gameState.dungeonMapManager.currentLocation.attachEncounterSetupInfo = encounterSetupInfo;
			}
			if (num)
			{
				gameState.actionManager.EnqueueAction(new OverlayWorldEventAction(we));
			}
			yield return new WaitUntil(() => gameState.actionManager.isEmpty && gameState.currentEncounter?.triggerEvent?.FullID == we.FullID);
			yield return new WaitUntil(() => gameState.currentEncounter.triggerEvent.finished || gameState.currentEncounter.triggerEvent.FullID != we.FullID);
			if (!overWrite)
			{
				gameState.actionManager.EnqueueAction(new OverlayWorldEventAction(originWorldEvent));
				yield return new WaitUntil(() => gameState.actionManager.isEmpty && gameState.currentEncounter.triggerEvent.FullID == originWorldEvent.FullID);
			}
		}

		public override List<IEffectNode> GetChildren()
		{
			List<IEffectNode> list = new List<IEffectNode>();
			foreach (NodeBasedWorldEvent.WorldEventChoiceSetup item in choiceSetups.OrEmptyIfNull())
			{
				list.Add(item);
			}
			return list;
		}
	}

	public class ExecuteStoryNote : StoryPointNode
	{
		[TextArea]
		public string note;

		public string signature;

		public NewItemGenerator itemGenerator;

		public SkillGenerator skillGenerator;

		public ClavisDef clavisDef;

		public bool haltAction;

		public bool gatherGet;

		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			GameEvent gameEvent = new GameEvent(GameEvent.Type.None, GameEvent.TriggerType.Real);
			gameEvent.gameState = applicationState.runningGameState;
			StoryNoteInfo noteInfo = StoryNoteInfo.PlainNote(note, signature, itemGenerator?.GenerateNewItems(gameEvent).FirstOrDefault(), skillGenerator?.GenerateSkills(gameEvent).FirstOrDefault(), clavisDef, gatherGet);
			if (applicationState.runningGameState != null)
			{
				applicationState.runningGameState.UIManager.storyNotePanel.Setup(noteInfo);
				if (haltAction)
				{
					applicationState.runningGameState.actionManager.EnqueueAction(new WaitUntilAction(() => !applicationState.runningGameState.UIManager.storyNotePanel.gameObject.activeInHierarchy));
				}
				yield return new WaitUntil(() => !applicationState.runningGameState.UIManager.storyNotePanel.gameObject.activeSelf);
			}
			if (applicationState.fortressState != null)
			{
				applicationState.fortressState.uiManager.storyNotePanel.Setup(noteInfo);
				yield return new WaitUntil(() => !applicationState.fortressState.uiManager.storyNotePanel.gameObject.activeSelf);
			}
		}
	}

	public class ExecuteEffect : StoryPointNode
	{
		public Effect effect;

		public bool waitForActionClean;

		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			if (applicationState.runningGameState == null)
			{
				throw new Exception("无法在游戏外运行");
			}
			GameEvent eve = new GameEvent(GameEvent.Type.None, GameEvent.TriggerType.Real);
			eve.gameState = applicationState.runningGameState;
			effect.Cast(eve);
			if (waitForActionClean)
			{
				yield return new WaitUntil(() => eve.gameState.actionManager.isEmpty);
			}
		}

		public override List<IEffectNode> GetChildren()
		{
			return new List<IEffectNode> { effect };
		}
	}

	public class StartNewGame : StoryPointNode
	{
		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			applicationState.newGameConfigManager.StartGame(applicationState.newGameConfigManager.currentNewGameConfig);
			yield break;
		}
	}

	public class BubbleDialogue : StoryPointNode
	{
		public DialogueSequence sequence;

		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			if (!(applicationState.runningGameState?.currentEncounter?.majorRoomObjectDisplay == null))
			{
				DialoguePlayer componentInChildren = applicationState.runningGameState.currentEncounter.majorRoomObjectDisplay.dialoguePivot.GetComponentInChildren<DialoguePlayer>();
				if (componentInChildren == null)
				{
					componentInChildren = applicationState.runningGameState.currentEncounter.majorRoomObjectDisplay.dialoguePivot.GetComponentInChildren<DialoguePlayer>();
				}
				applicationState.runningGameState.actionManager.EnqueueAction(new TriggerDialogueAction(sequence, componentInChildren));
			}
			yield break;
		}
	}

	public class PlayCutscene : StoryPointNode
	{
		public CutsceneRef cutSceneRef;

		public bool skipFadeIn;

		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			CutsceneInfoData cutSceneInfo = cutSceneRef.GetAssetPreData(applicationState.assetCatalog);
			if (cutSceneInfo != null)
			{
				if (applicationState.runningGameState != null)
				{
					applicationState.runningGameState.actionManager.AcquireLock(ActionLock.Type.Cutscene);
					PlayCutSceneState playCutSceneState = new PlayCutSceneState(cutSceneInfo, applicationState.runningGameState, stayCutscene: false, skipFadeIn);
					yield return playCutSceneState.PlayCutsceneCor();
					applicationState.runningGameState.actionManager.ReleaseLock(ActionLock.Type.Cutscene);
				}
				if (applicationState.fortressState != null)
				{
					PlayCutSceneState playCutSceneState2 = new PlayCutSceneState(cutSceneInfo, applicationState.fortressState, stayCutscene: false, skipFadeIn);
					yield return playCutSceneState2.PlayCutsceneCor();
				}
			}
		}
	}

	public class PlayGuideSequence : StoryPointNode
	{
		public enum MaskGuideType
		{
			TalentTreeGuide = 1,
			ConfigSkillGuide,
			DifficultyGuide,
			GachaGuide,
			SkillSpellCastGuide,
			ConfigMasteryGuide,
			ActivateSkillGuide,
			UpgradeRoomSetSkinGuide,
			SkillBreakthroughCraftGuide,
			SkillBreakthroughCompleteGuide,
			SkillBreakthroughReturnGuide,
			IdolFirstWorshipGuide,
			IdolFirstWorshipEnterRoomGuide
		}

		public MaskGuideType maskGuideType;

		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			FortressGuidePanel fortressGuidePanel = applicationState.fortressState?.uiManager?.fortressGuidePanel;
			if (fortressGuidePanel != null)
			{
				if (maskGuideType == MaskGuideType.ConfigMasteryGuide)
				{
					yield return fortressGuidePanel.PlayFirstMasteryTalentGuideCor();
					yield break;
				}
				if (maskGuideType == MaskGuideType.IdolFirstWorshipEnterRoomGuide)
				{
					yield return fortressGuidePanel.PlayIdolFirstWorshipEnterRoomGuideCor();
					yield break;
				}
				if (maskGuideType == MaskGuideType.IdolFirstWorshipGuide)
				{
					yield return fortressGuidePanel.PlayIdolFirstWorshipGuideCor();
					yield break;
				}
				if (maskGuideType == MaskGuideType.DifficultyGuide)
				{
					yield return fortressGuidePanel.AutoCompleteDifficultyGuideCor();
					yield break;
				}
			}
			MaskGuideSequence maskGuideSequence = null;
			if (fortressGuidePanel != null)
			{
				maskGuideSequence = fortressGuidePanel.maskGuideSeqSetups.FirstOrDefault((MaskGuideSeqSetup t) => t.type == maskGuideType)?.guideSeq;
			}
			GuideUIPanel guideUIPanel = applicationState.runningGameState?.UIManager?.guideUIPanel;
			if (guideUIPanel != null)
			{
				maskGuideSequence = guideUIPanel.maskGuideSeqSetups.FirstOrDefault((MaskGuideSeqSetup t) => t.type == maskGuideType)?.guideSeq;
			}
			if (maskGuideSequence == null && fortressGuidePanel != null)
			{
				if (maskGuideType == MaskGuideType.SkillBreakthroughCraftGuide)
				{
					yield return fortressGuidePanel.PlaySkillBreakthroughCraftGuideCor();
					yield break;
				}
				if (maskGuideType == MaskGuideType.SkillBreakthroughCompleteGuide)
				{
					yield return fortressGuidePanel.PlaySkillBreakthroughCompleteGuideCor();
					yield break;
				}
				if (maskGuideType == MaskGuideType.SkillBreakthroughReturnGuide)
				{
					yield return fortressGuidePanel.PlaySkillBreakthroughReturnGuideCor();
					yield break;
				}
			}
			if (maskGuideSequence == null)
			{
				yield break;
			}
			if (maskGuideType == MaskGuideType.UpgradeRoomSetSkinGuide)
			{
				yield return PlayUpgradeRoomSetSkinGuideCor(applicationState, maskGuideSequence);
				yield break;
			}
			yield return maskGuideSequence.GuideCor();
			if (maskGuideType == MaskGuideType.DifficultyGuide && fortressGuidePanel != null)
			{
				fortressGuidePanel.SelectHardDifficultyAfterDifficultyGuide();
			}
		}

		private static IEnumerator PlayUpgradeRoomSetSkinGuideCor(ApplicationStateInfo applicationState, MaskGuideSequence seq)
		{
			seq.gameObject.SetActive(value: true);
			GuideMask[] guideMasks = seq.GuideMasks;
			for (int i = 0; i < guideMasks.Length; i++)
			{
				guideMasks[i].SetActiveAndDirty(active: false);
			}
			GuideMask[] guideMasks2 = seq.GuideMasks;
			foreach (GuideMask guide in guideMasks2)
			{
				GuideMask.GuideTargetType targetType = guide.targetType;
				RectTransform presetTarget = guide.presetTarget;
				ConfigureUpgradeRoomSetSkinGuideStep(applicationState, guide);
				guide.StartGuide(seq);
				yield return new WaitUntil(() => guide.finished);
				guide.targetType = targetType;
				guide.presetTarget = presetTarget;
			}
			seq.gameObject.SetActive(value: false);
		}

		private static void ConfigureUpgradeRoomSetSkinGuideStep(ApplicationStateInfo applicationState, GuideMask guide)
		{
			if (guide.targetType == GuideMask.GuideTargetType.MaskToFirstSkinUpgradeItem)
			{
				ItemDisplayBase skinUpgradeGuideTargetDisplay = applicationState.runningGameState.UIManager.worldEventPanel_UpgradeRoom.upgradePanel.GetSkinUpgradeGuideTargetDisplay();
				if (!(skinUpgradeGuideTargetDisplay == null))
				{
					guide.presetTarget = skinUpgradeGuideTargetDisplay.GetComponent<RectTransform>();
				}
			}
		}
	}

	public class ShowIdolGuideHint : StoryPointNode
	{
		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			GuideHintPanelSet guideHint = applicationState.fortressState?.uiManager?.fortressGuidePanel?.idolGuideHint;
			if (!(guideHint == null))
			{
				bool confirmed = false;
				guideHint.ShowAndMarkAutoPopuped(delegate
				{
					confirmed = true;
				});
				yield return new WaitUntil(() => confirmed || guideHint == null || !guideHint.gameObject.activeInHierarchy);
			}
		}
	}

	public class PlayDifficultyUnlockStageDisplay : StoryPointNode
	{
		public DifficultyLevel difficultyLevel;

		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			DifficultyUnlockStage difficultyUnlockStage = applicationState.fortressState?.uiManager?.difficultyUnlockStage;
			if (!(difficultyUnlockStage == null))
			{
				yield return difficultyUnlockStage.TriggerDifficultyAnim(difficultyLevel);
			}
		}
	}

	public class SetGameDifficultyToEasyIfFirstGameNotWin : StoryPointNode
	{
		public override IEnumerator ExecuteCor(ApplicationStateInfo applicationState)
		{
			if (applicationState.gameStatisticsManager.winGameCount <= 0)
			{
				applicationState.newGameConfigManager.currentNewGameConfig.contract.difficultyLevel = DifficultyLevel.Normal;
			}
			yield break;
		}
	}

	[CTSOAssetName]
	public string pointName;

	public List<StoryPoint> parentPoints;

	public int observeCountRequirement;

	public GlobalTriggerObserver triggerObserver;

	public int storyVersion_openTest;

	public StoryPointNode node;

	public bool hideRewardHint;

	public List<GlobalRewardSetup> finishRewards;

	public List<ClavisDef> finishUnlockClavis;

	public override string CTAssetIDPrepend => "STP";

	public bool NeedObserve => observeCountRequirement > 0;

	public void Init(ApplicationStateInfo applicationState)
	{
		if (triggerObserver != null && !IsFinished(applicationState))
		{
			triggerObserver.Init(applicationState, delegate
			{
				_OnObserve(applicationState);
			});
		}
	}

	public void _OnObserve(ApplicationStateInfo applicationState)
	{
		if (!IsFinished(applicationState))
		{
			StoryPointManager.StoryPointState storyState = applicationState.storyPointManager.GetStoryState(this);
			if (parentPoints == null || parentPoints.TrueForAll((StoryPoint s) => s.IsFinished(applicationState)))
			{
				TriggerStoryPoint(storyState, applicationState);
			}
		}
	}

	public void TriggerStoryPoint(StoryPointManager.StoryPointState state, ApplicationStateInfo applicationState)
	{
		int num = Mathf.Max(1, observeCountRequirement);
		if (state.observeCount < num)
		{
			AddStoryPointObserveCount(state, applicationState);
		}
		if (state.observeCount >= num && !applicationState.storyPointManager.storyPointStateQueue.Contains(this))
		{
			Execute(applicationState);
		}
	}

	private void AddStoryPointObserveCount(StoryPointManager.StoryPointState state, ApplicationStateInfo applicationState)
	{
		state.observeCount++;
		if (GlobalConfigManager.instance.enableNetwork)
		{
			new GeneralGameAsyncOperationRequestOnly<UpdateStoryPointRequest>(new UpdateStoryPointRequest(state), applicationState.ChillyRoomSDK.Progress.UpdateStoryPointStateAsync).HideFaultPanel().EnqueueRun(GameAsyncOperationLane.Foreground, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_syncing_story_progress);
		}
	}

	public void Execute(ApplicationStateInfo applicationState, FinishStoryPointResult finishResult = null, bool skip = false)
	{
		Debug.Log("叙事点触发【" + pointName + "】");
		applicationState.StartCoroutine(() => ExecuteCor(applicationState, finishResult, skip));
	}

	public IEnumerator ExecuteCor(ApplicationStateInfo applicationState, FinishStoryPointResult finishResult, bool skip)
	{
		Queue<StoryPoint> storyPointQueue = applicationState.storyPointManager.storyPointStateQueue;
		if (storyPointQueue.Contains(this))
		{
			yield break;
		}
		storyPointQueue.Enqueue(this);
		while (applicationState.storyPointManager.RunningStoryPoint != this)
		{
			yield return null;
		}
		bool finishRequestOver = finishResult != null;
		if (GlobalConfigManager.instance.enableNetwork)
		{
			FinishStoryPointRequest request = new FinishStoryPointRequest(this);
			GeneralGameAsyncOperation<FinishStoryPointRequest, FinishStoryPointResult> op = new GeneralGameAsyncOperation<FinishStoryPointRequest, FinishStoryPointResult>(request, applicationState.ChillyRoomSDK.Progress.FinishStoryPointAsync);
			op.OnComplete(delegate
			{
				finishResult = op.OpResult;
				finishRequestOver = true;
			}).OnFault(delegate
			{
				finishRequestOver = true;
			}).ManualRetry()
				.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_syncing_story_progress);
		}
		if (!skip)
		{
			yield return node.ExecuteCor(applicationState);
		}
		if (GlobalConfigManager.instance.enableNetwork)
		{
			yield return new WaitUntil(() => finishRequestOver);
			if (finishResult == null)
			{
				storyPointQueue.Dequeue();
				yield break;
			}
		}
		bool flag = skip;
		Finish(applicationState, flag, finishResult);
		if (!flag)
		{
			while (!ClavisAndRewardDisplayFinished(applicationState))
			{
				yield return null;
			}
		}
		storyPointQueue.Dequeue();
	}

	private void Finish(ApplicationStateInfo applicationState, bool silent, FinishStoryPointResult finishResult = null)
	{
		if (finishResult != null)
		{
			FinishImpl(finishResult, applicationState, silent);
		}
		else
		{
			FinishImpl(applicationState, silent);
		}
	}

	private void FinishImpl(ApplicationStateInfo applicationState, bool silent)
	{
		applicationState.storyPointManager.GetStoryState(this).finished = true;
		if (finishRewards != null)
		{
			GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(finishRewards.ConvertAll((GlobalRewardSetup r) => r.GenerateReward(applicationState, GlobalRewardSourceType.Story, this)));
			globalRewardPackage.GiveAll(applicationState);
			if (applicationState.fortressState != null && !silent && !hideRewardHint)
			{
				applicationState.fortressState.uiManager.globalRewardPackageDisplay.Setup(globalRewardPackage);
			}
		}
		if (finishUnlockClavis != null)
		{
			foreach (ClavisDef finishUnlockClavi in finishUnlockClavis)
			{
				applicationState.clavisManager.UnlockClavis(finishUnlockClavi);
			}
		}
		ThinkingAnalyticsExtends.TrackFinishStoryPoint(applicationState, this);
		applicationState.eventManager.storyPointFinished.TriggerEvent(this);
		if (applicationState.fortressState?.uiManager != null)
		{
			applicationState.fortressState.uiManager.SetDirty();
		}
		applicationState.savingManager.SetProfileDirtyToSave();
	}

	private void FinishImpl(FinishStoryPointResult result, ApplicationStateInfo applicationState, bool silent)
	{
		applicationState.storyPointManager.GetStoryState(this).finished = true;
		NetworkConvertContext context = new NetworkConvertContext(applicationState);
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		foreach (ChillyRoom.Services.ContainerOfSoul.GlobalRewardInfo item in result.Rewards.OrEmptyIfNull())
		{
			list.Add(item.ToClient(context));
		}
		if (list.Count > 0)
		{
			GlobalRewardPackage globalRewardPackage = new GlobalRewardPackage(list);
			globalRewardPackage.GiveAll(applicationState);
			if (applicationState.fortressState != null && !silent && !hideRewardHint)
			{
				applicationState.fortressState.uiManager.globalRewardPackageDisplay.Setup(globalRewardPackage);
			}
		}
		foreach (UnlockClavisResult item2 in result.AcquireClavisResults.OrEmptyIfNull())
		{
			applicationState.clavisManager.UnlockClavisImpl(item2, applicationState, silent);
		}
		ThinkingAnalyticsExtends.TrackFinishStoryPoint(applicationState, this);
		applicationState.eventManager.storyPointFinished.TriggerEvent(this);
		if (applicationState.fortressState?.uiManager != null)
		{
			applicationState.fortressState.uiManager.SetDirty();
		}
		applicationState.savingManager.SetProfileDirtyToSave();
	}

	private bool ClavisAndRewardDisplayFinished(ApplicationStateInfo applicationState)
	{
		if (applicationState.runningGameState != null && applicationState.runningGameState.UIManager.clavisHint.WithClavisToHint)
		{
			return false;
		}
		if (applicationState.fortressState != null)
		{
			if (applicationState.fortressState.uiManager.clavisHint.WithClavisToHint)
			{
				return false;
			}
			if (applicationState.fortressState.uiManager.globalRewardPackageDisplay.isActiveAndEnabled)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsFinished(ApplicationStateInfo applicationState)
	{
		try
		{
			if (applicationState.storyPointManager == null)
			{
				return false;
			}
			return applicationState.storyPointManager.GetStoryState(this).finished;
		}
		catch (Exception arg)
		{
			TraceLog.Error($"[StoryPoint] IsFinished() - [{pointName}] [{applicationState.storyPointManager == null}] {arg}");
			return false;
		}
	}
}
