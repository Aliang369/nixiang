using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using ContainerOfSoul.Display.Fortress;
using GameRandom;
using UnityEngine;

public class FortressNPC : CTUIBaseAsset, IFrameUpdate
{
	[Serializable]
	public class ProgressNode
	{
		[Serializable]
		public class WeightedAvatarTalkData
		{
			public AvatarTalkData avatarTalkData;

			public float weight = 1f;
		}

		public enum ProgressType
		{
			Persistence,
			OneTime,
			End
		}

		public StoryPoint activateStoryPoint;

		public ProgressType type;

		public int priority;

		public List<WeightedAvatarTalkData> interactiveAvatarTalks;

		public AvatarTalkData avatarTalkData;

		[LocalizeText("立绘对话文本")]
		[TextArea]
		public string scribeIndexBookLockedFirstText;

		[LocalizeText("立绘对话文本")]
		[TextArea]
		public string scribeIndexBookLockedSecondText;

		public StoryPoint talkStoryPoint;

		public DialogueSequence idleDialogueSequence;

		public GameObject displaySubObject;

		[NonSerialized]
		private AvatarTalkData lastPlayedAvatarTalkData;

		public bool IsTalkStoryPointNode => talkStoryPoint != null;

		private List<WeightedAvatarTalkData> GetValidInteractiveAvatarTalks()
		{
			if (interactiveAvatarTalks == null || interactiveAvatarTalks.Count == 0)
			{
				return null;
			}
			return interactiveAvatarTalks.Where((WeightedAvatarTalkData t) => t != null && t.avatarTalkData != null).ToList();
		}

		public AvatarTalkData SelectInteractiveAvatarTalkData()
		{
			List<WeightedAvatarTalkData> validInteractiveAvatarTalks = GetValidInteractiveAvatarTalks();
			if (validInteractiveAvatarTalks == null || validInteractiveAvatarTalks.Count == 0)
			{
				return avatarTalkData;
			}
			if (validInteractiveAvatarTalks.Count == 1)
			{
				lastPlayedAvatarTalkData = validInteractiveAvatarTalks[0].avatarTalkData;
				return lastPlayedAvatarTalkData;
			}
			List<WeightedAvatarTalkData> list = validInteractiveAvatarTalks.Where((WeightedAvatarTalkData t) => t.avatarTalkData != lastPlayedAvatarTalkData).ToList();
			if (list.Count == 0)
			{
				list = validInteractiveAvatarTalks;
			}
			lastPlayedAvatarTalkData = list.SelectRandomWeight((WeightedAvatarTalkData t) => (!(t.weight > 0f)) ? 1f : t.weight)?.avatarTalkData;
			return lastPlayedAvatarTalkData;
		}

		private bool ShouldUseScribeIndexBookLockedTalk(ApplicationStateInfo applicationState)
		{
			if (scribeIndexBookLockedFirstText.IsNullOrEmpty() || scribeIndexBookLockedSecondText.IsNullOrEmpty())
			{
				return false;
			}
			if (avatarTalkData == null || avatarTalkData.FullID != "ATD#1ef1eb5b6cfb8274daa4accf6b868fab")
			{
				return false;
			}
			if (applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressStoreHousePanel))
			{
				return false;
			}
			if (applicationState.gameStatisticsManager.gameCount >= 1)
			{
				return applicationState.gameStatisticsManager.gameCount < 2;
			}
			return false;
		}

		private AvatarDialogueSection BuildScribeIndexBookLockedSection()
		{
			return new AvatarDialogueSection
			{
				steps = new List<AvatarDialogueStep>
				{
					new AvatarSpeakText
					{
						side = AvatarDialogueStep.AvatarSide.Right,
						text = scribeIndexBookLockedFirstText,
						newAvatar = avatarTalkData.section.steps.OfType<AvatarSpeakText>().FirstOrDefault()?.newAvatar
					},
					new AvatarSpeakText
					{
						side = AvatarDialogueStep.AvatarSide.Right,
						text = scribeIndexBookLockedSecondText
					}
				}
			};
		}

		private void PlayAvatarDialogueSection(ApplicationStateInfo applicationState, AvatarDialogueSection section)
		{
			if (section != null)
			{
				applicationState.fortressState.StartCoroutine(() => applicationState.fortressState.uiManager.avatarDialoguePanel.PlayCurrentDialogueSectionCor(section));
			}
		}

		private bool TryPlayInteractiveAvatarTalk(ApplicationStateInfo applicationState)
		{
			if (ShouldUseScribeIndexBookLockedTalk(applicationState))
			{
				PlayAvatarDialogueSection(applicationState, BuildScribeIndexBookLockedSection());
				return true;
			}
			AvatarTalkData avatarTalkData = SelectInteractiveAvatarTalkData();
			if (avatarTalkData == null)
			{
				return false;
			}
			PlayAvatarDialogueSection(applicationState, avatarTalkData.section);
			return true;
		}

		public bool HasInteractiveAvatarTalkData()
		{
			if (avatarTalkData != null)
			{
				return true;
			}
			List<WeightedAvatarTalkData> validInteractiveAvatarTalks = GetValidInteractiveAvatarTalks();
			if (validInteractiveAvatarTalks != null)
			{
				return validInteractiveAvatarTalks.Count > 0;
			}
			return false;
		}

		public void TriggerTalk(FortressNPC npc, ApplicationStateInfo applicationState)
		{
			ThinkingAnalyticsExtends.TrackTalkToFortressNPC(applicationState, npc);
			if (GlobalConfigManager.instance.enableNetwork)
			{
				if (!IsFinished(npc, applicationState))
				{
					TalkToFortressNPCRequest request = new TalkToFortressNPCRequest(npc, applicationState);
					GeneralGameAsyncOperation<TalkToFortressNPCRequest, TalkToFortressNPCResult> op = new GeneralGameAsyncOperation<TalkToFortressNPCRequest, TalkToFortressNPCResult>(request, applicationState.ChillyRoomSDK.Progress.TalkFortressNPCAsync);
					op.OnComplete(delegate
					{
						if (!TryPlayInteractiveAvatarTalk(applicationState) && op.OpResult.FinishStoryPointResult != null)
						{
							talkStoryPoint.Execute(applicationState, op.OpResult.FinishStoryPointResult);
						}
						if (!IsFinished(npc, applicationState))
						{
							Finish(npc, applicationState);
						}
					}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_syncing_story_progress);
				}
				else
				{
					TryPlayInteractiveAvatarTalk(applicationState);
				}
			}
			applicationState.eventManager.talkToFortressNPCEvent.TriggerEvent(npc);
		}

		public bool IsTalkable(FortressNPC npc, ApplicationStateInfo applicationState)
		{
			if (!HasInteractiveAvatarTalkData())
			{
				if (talkStoryPoint != null)
				{
					return !IsFinished(npc, applicationState);
				}
				return false;
			}
			return true;
		}

		public bool IsActivated(ApplicationStateInfo applicationState)
		{
			return applicationState.storyPointManager.GetStoryState(activateStoryPoint).finished;
		}

		public bool IsFinished(FortressNPC npc, ApplicationStateInfo applicationState)
		{
			return applicationState.storyPointManager.fortressNPCStates.Find((StoryPointManager.FortressNPCState x) => x.fortressNPCId == npc.FullId).finishedProgressStoryPoints.Contains(activateStoryPoint);
		}

		public void Finish(FortressNPC npc, ApplicationStateInfo applicationState)
		{
			applicationState.storyPointManager.fortressNPCStates.Find((StoryPointManager.FortressNPCState x) => x.fortressNPCId == npc.FullId).finishedProgressStoryPoints.Add(activateStoryPoint);
			applicationState.savingManager.SetProfileDirtyToSave();
		}

		public bool IsShowAble(FortressNPC npc, ApplicationStateInfo applicationState)
		{
			if (IsActivated(applicationState))
			{
				if (type == ProgressType.OneTime)
				{
					return !IsFinished(npc, applicationState);
				}
				return true;
			}
			return false;
		}
	}

	private const string ScribeDefaultTalkDataId = "ATD#1ef1eb5b6cfb8274daa4accf6b868fab";

	private const int ScribeIndexBookLockedTalkMinFinishedGameCount = 1;

	private const int ScribeIndexBookLockedTalkMaxFinishedGameCount = 2;

	[LocalizeText("NPC名称")]
	public string npcName;

	public List<ProgressNode> progressNodes;

	public HintRedDot talkHint;

	public GameButton talkTriggerButton;

	public FortressMainUIPanel.MainPanelLayer layerType;

	public DialoguePlayer dialoguePlayer;

	[NonSerialized]
	public ProgressNode currentProgressNode;

	[NonSerialized]
	private bool roomSwitchDialogueSuppressed;

	public override string CTAssetIDPrepend => "FORTRESSNPC";

	private string FortressNPCRuleHint => "\n+++ 载入外部系统时，会按照以下流程判断当前 NPC 显示进度节点\n1. 如果终结节点已经激活，则 NPC 直接隐藏\n2. 在所有满足以下条件的节点中，选择优先级最高的节点显示\n+ 已激活但未完成的一次性节点\n+ 已激活的持久节点\n3. 如果上述条件都不符合，则不显示\n\n+++ NPC 的进度节点中不能有两个进度节点使用相同的激活叙事点\n+++ “进度节点是否完成”取决于玩家是否点击过 NPC 对话，点击过则该进度“完成”\n+++ 进度节点“完成”不意味着这个进度就不会显示，显示规则参照上面\n+++ 如果 NPC 要变换进度节点，则玩家必须重新载入主界面\n编辑器中可以使用“编辑器小工具-立即刷新所有要塞 NPC 进度”来立刻刷新进度节点\n";

	protected override void Init()
	{
		talkTriggerButton.onClick.AddListener(_OnTalkTriggerButtonClicked);
		currentProgressNode = GetCurrentDisplayProgressNode(UIBase.applicationState);
	}

	protected override void Dispose()
	{
		talkTriggerButton.onClick.RemoveListener(_OnTalkTriggerButtonClicked);
	}

	protected override void UpdateDisplay()
	{
		UpdateProgressNode();
	}

	public void UpdateProgressNode()
	{
		currentProgressNode = GetCurrentDisplayProgressNode(UIBase.applicationState);
	}

	public void FrameUpdate()
	{
		if (currentProgressNode == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!roomSwitchDialogueSuppressed && !dialoguePlayer.IsPlaying(UIBase.fortressState) && currentProgressNode.idleDialogueSequence != null && currentProgressNode.idleDialogueSequence.dialogueSequence.Count > 0)
		{
			UIBase.applicationState.fortressState.uiManager.DialoguePanel.TriggerDialogue(currentProgressNode.idleDialogueSequence, dialoguePlayer, null);
		}
		talkHint.gameObject.SetActive(currentProgressNode.IsTalkable(this, UIBase.applicationState) && !currentProgressNode.IsFinished(this, UIBase.applicationState));
		foreach (ProgressNode progressNode in progressNodes)
		{
			if (!(progressNode.displaySubObject == null))
			{
				progressNode.displaySubObject.SetActive(progressNode.displaySubObject == currentProgressNode.displaySubObject);
			}
		}
	}

	public void SetRoomSwitchDialogueSuppressed(bool suppressed)
	{
		if (roomSwitchDialogueSuppressed != suppressed)
		{
			roomSwitchDialogueSuppressed = suppressed;
			if (suppressed && dialoguePlayer != null && UIBase.fortressState?.uiManager?.DialoguePanel != null)
			{
				UIBase.fortressState.uiManager.DialoguePanel.StopAllDialogueOfPlayer(dialoguePlayer);
			}
		}
	}

	public ProgressNode GetCurrentDisplayProgressNode(ApplicationStateInfo applicationState)
	{
		if (progressNodes.Find((ProgressNode t) => t.type == ProgressNode.ProgressType.End && t.IsActivated(applicationState)) != null)
		{
			return null;
		}
		List<ProgressNode> list = progressNodes.FindAll((ProgressNode t) => t.IsShowAble(this, applicationState));
		if (list.Count == 0)
		{
			return null;
		}
		list = list.OrderByDescending((ProgressNode a) => a.priority).ToList();
		return list[0];
	}

	private void _OnTalkTriggerButtonClicked()
	{
		if (currentProgressNode != null && currentProgressNode.IsTalkable(this, UIBase.applicationState))
		{
			currentProgressNode.TriggerTalk(this, UIBase.applicationState);
		}
	}
}
