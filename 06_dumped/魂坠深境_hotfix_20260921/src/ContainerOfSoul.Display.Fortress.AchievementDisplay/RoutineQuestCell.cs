using System;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.AchievementDisplay;

public class RoutineQuestCell : CellUI<RoutineQuest>
{
	public TextMeshProUGUI questDescriptionText;

	public TextMeshProUGUI questProgressText;

	public Slider questProgress;

	public GameButton interactButton;

	public GameButton gotoButton;

	public TextMeshProUGUI interactButtonText;

	public GameObject gottenDarken;

	public GlobalItemDisplay rewardCellPrefab;

	public RectTransform rewardCellParent;

	private RoutineQuest quest;

	public HintRedDot redDot;

	[NonSerialized]
	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> rewardCellPool;

	[NonSerialized]
	private SeasonDetailPanel seasonDetailParentPanel;

	protected override void Init()
	{
		seasonDetailParentPanel = GetUIInParents<SeasonDetailPanel>();
		rewardCellPool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(rewardCellPrefab, rewardCellParent);
		interactButton.onClick.AddListener(_OnInteractButtonClicked);
		gotoButton.onClick.AddListener(delegate
		{
			if (quest.JumpPanelType != RoutineQuestDef.RoutineQuestNode.JumpPanelType.None)
			{
				JumpToPanel();
			}
		});
	}

	protected override void OnSetup(RoutineQuest t)
	{
		quest = t;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (quest == null)
		{
			return;
		}
		questDescriptionText.text = quest.GetDescription(CellUI<RoutineQuest>.applicationState);
		questProgressText.text = $"{quest.progress}/{quest.CurrentMaxProgress}";
		questProgress.value = (float)quest.progress / (float)quest.CurrentMaxProgress;
		rewardCellPool.Sync(quest.def.rewards.ConvertAll((Converter<GlobalRewardSetup, IGlobalItemDisplaySetup>)((GlobalRewardSetup s) => s)));
		foreach (GlobalItemDisplay activeElement in rewardCellPool.GetActiveElements())
		{
			InfoModule module = activeElement.GetModule<InfoModule>();
			if (module != null)
			{
				module.infoPanel = ((activeElement.setupData is AddCurrentSeasonLevelExp) ? null : CellUI<RoutineQuest>.fortressState.uiManager.universalCommonInfoPanel);
			}
		}
		interactButton.gameObject.SetActive(value: true);
		interactButton.interactable = false;
		gotoButton.gameObject.SetActive(value: false);
		redDot.gameObject.SetActive(value: false);
		if (quest.AllLayerFinished)
		{
			interactButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_finished;
		}
		else if (quest.GetRewardAble)
		{
			interactButton.interactable = true;
			interactButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_quest_get_bonus;
			redDot.gameObject.SetActive(value: true);
		}
		else if (quest.JumpPanelType != RoutineQuestDef.RoutineQuestNode.JumpPanelType.None)
		{
			interactButton.gameObject.SetActive(value: false);
			gotoButton.gameObject.SetActive(value: true);
			interactButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_not_finished;
		}
		else
		{
			interactButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_not_finished;
		}
		gottenDarken.gameObject.SetActive(quest.AllLayerFinished);
	}

	private void _OnInteractButtonClicked()
	{
		if (quest.GetRewardAble && GlobalConfigManager.instance.enableNetwork)
		{
			GetRoutineQuestBonusRequest request = new GetRoutineQuestBonusRequest(quest);
			GeneralGameAsyncOperation<GetRoutineQuestBonusRequest, GetRoutineQuestBonusResult> op = new GeneralGameAsyncOperation<GetRoutineQuestBonusRequest, GetRoutineQuestBonusResult>(request, CellUI<RoutineQuest>.applicationState.ChillyRoomSDK.Routine.GetRoutineQuestBonusAsync);
			op.OnComplete(delegate
			{
				GlobalRewardPackage rewardPackage = op.OpResult.GiveRoutineQuestRewards(CellUI<RoutineQuest>.applicationState);
				AfterGetReward(rewardPackage);
			}).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_claiming_quest_reward);
		}
		void AfterGetReward(GlobalRewardPackage rewardPackage)
		{
			CellUI<RoutineQuest>.applicationState.StartCoroutine(() => CellUI<RoutineQuest>.applicationState.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(rewardPackage));
			ThinkingAnalyticsExtends.TrackRoutineQuestFinish(CellUI<RoutineQuest>.applicationState, quest);
			if (quest.sumExp != 0)
			{
				CellUI<RoutineQuest>.applicationState.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_get_exp.FormatWith(quest.sumExp));
			}
			manager?.SetDirty();
		}
	}

	public void JumpToPanel()
	{
		RoutineQuestNavigation.TryJumpToPanel(quest, CellUI<RoutineQuest>.applicationState, delegate
		{
			seasonDetailParentPanel?.SetActiveAndDirty(active: false);
		});
	}
}
