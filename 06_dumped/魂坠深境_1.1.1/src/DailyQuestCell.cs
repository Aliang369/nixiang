using System;
using Common.UI;
using TMPro;
using UnityEngine;

public class DailyQuestCell : CellUI<RoutineQuest>
{
	private static readonly Color ClaimableActionButtonColor = new Color32(241, 223, 173, byte.MaxValue);

	public GlobalItemDisplay rewardCellPrefab;

	public RectTransform rewardCellParent;

	public GameObject getReewardAbleHint;

	public GameButton replaceQuestButton;

	public GameButton actionButton;

	public TextMeshProUGUI actionButtonText;

	public TextMeshProUGUI questDescriptionText;

	public TextMeshProUGUI questProgressText;

	public MatterCell cost;

	[NonSerialized]
	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> rewardCellPool;

	[NonSerialized]
	private DailyQuestPanel parentPanel;

	private RoutineQuest quest;

	protected override void Init()
	{
		InitUI(cost);
		parentPanel = GetUIInParents<DailyQuestPanel>();
		rewardCellPool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(rewardCellPrefab, rewardCellParent);
		actionButton.onClick.AddListener(_OnActionButtonClicked);
		replaceQuestButton.onClick.AddListener(_OnReplaceQuestButtonClicked);
	}

	protected override void OnSetup(RoutineQuest t)
	{
		quest = t;
		cost.Setup(ScriptableObjectSingleton<FortressDataManager>.instance.refreshDailyQuestMatter);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		if (quest != null)
		{
			questDescriptionText.text = quest.GetDescription(CellUI<RoutineQuest>.applicationState);
			questProgressText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(quest.progress, quest.CurrentMaxProgress);
			rewardCellPool.Sync(quest.def.rewards.ConvertAll((Converter<GlobalRewardSetup, IGlobalItemDisplaySetup>)((GlobalRewardSetup s) => s)));
			bool allLayerFinished = quest.AllLayerFinished;
			bool getRewardAble = quest.GetRewardAble;
			bool flag = !allLayerFinished && quest.JumpPanelType != RoutineQuestDef.RoutineQuestNode.JumpPanelType.None;
			getReewardAbleHint.SetActive(getRewardAble);
			actionButton.interactable = getRewardAble | flag;
			actionButtonText.text = (allLayerFinished ? ScriptableObjectSingleton<TextDataPreset>.instance.term_finished : (getRewardAble ? ScriptableObjectSingleton<TextDataPreset>.instance.term_quest_get_bonus : (flag ? ScriptableObjectSingleton<TextDataPreset>.instance.term_quest_jump_to_Panel : ScriptableObjectSingleton<TextDataPreset>.instance.term_not_finished)));
			if (actionButton.targetGraphic != null)
			{
				actionButton.targetGraphic.color = (getRewardAble ? ClaimableActionButtonColor : Color.white);
			}
			replaceQuestButton.gameObject.SetActive(!allLayerFinished && !getRewardAble);
			replaceQuestButton.interactable = !allLayerFinished && !getRewardAble && CellUI<RoutineQuest>.applicationState.routineQuestManager.CanRefresh();
		}
	}

	private void _OnActionButtonClicked()
	{
		if (quest == null)
		{
			return;
		}
		if (quest.GetRewardAble)
		{
			_GetReward();
			return;
		}
		RoutineQuestNavigation.TryJumpToPanel(quest, CellUI<RoutineQuest>.applicationState, delegate
		{
			parentPanel?.HideUI();
		});
	}

	private void _GetReward()
	{
		CellUI<RoutineQuest>.applicationState.routineQuestManager.GetReward(quest, delegate(GlobalRewardPackage rewardPackage)
		{
			CellUI<RoutineQuest>.applicationState.StartCoroutine(() => CellUI<RoutineQuest>.applicationState.fortressState.uiManager.globalRewardPackageDisplay.ShowNestedRewardPackSeperateCor(rewardPackage));
			ThinkingAnalyticsExtends.TrackRoutineQuestFinish(CellUI<RoutineQuest>.applicationState, quest);
			parentPanel?.SetDirty();
		});
	}

	private void _OnReplaceQuestButtonClicked()
	{
		if (quest != null && !quest.AllLayerFinished && !quest.GetRewardAble)
		{
			CellUI<RoutineQuest>.applicationState.routineQuestManager.ReplaceQuest(quest, delegate(RoutineQuest newQuest)
			{
				Setup(newQuest);
				parentPanel?.SetDirty();
			});
		}
	}
}
