using System;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Fortress.DotManagers;
using TMPro;
using UnityEngine;

public class SeasonDetailPanel : UIBase, IShowAsync, IHideAsync
{
	public class LevelLineModule : RedDotModule
	{
		protected override bool? CheckImpl()
		{
			return RedDotModule.applicationState.seasonManager.currentSeason.seasonLeveLine.WithAnyUngotReward(RedDotModule.applicationState);
		}
	}

	public SeasonProgressBriefPanel progressBriefPanel;

	public SeasonPanel seasonPanel;

	public SeasonQuestPanel seasonQuestPanel;

	public BuyLevelOfLineLevelPanel buyLevelOfLineLevelPanel;

	public GameButton jumpToSeasonStoreAreaButton;

	public GameButton jumpToSeasonStoreButton;

	public TextMeshProUGUI jumpToSeasonStoreButtonText;

	public GameObject jumpToSeasonStoreCompleteMask;

	public GameButton backButton;

	public SwitchPanel switchPanel;

	public SwitchTag levelLineTag;

	public SwitchTag questTag;

	public HintRedDot questHintDot;

	public HintRedDot levelLineHintDot;

	protected override void Init()
	{
		InitUI(progressBriefPanel);
		InitUI(seasonPanel);
		InitUI(seasonQuestPanel);
		InitUI(switchPanel);
		InitUI(buyLevelOfLineLevelPanel);
		InitUI(questHintDot);
		InitUI(levelLineHintDot);
		UIBase.applicationState.eventManager.ReceiveRewardEvent.AddListener(_OnReward);
		seasonPanel.Setup(UIBase.applicationState.seasonManager.currentSeason);
		switchPanel.Select(levelLineTag);
		SwitchPanel obj = switchPanel;
		obj.onTagSwitched = (Action)Delegate.Combine(obj.onTagSwitched, new Action(_OnTagSwitched));
		backButton.onClick.AddListener(delegate
		{
			switchPanel.Select(levelLineTag);
			this.HideUI();
		});
		jumpToSeasonStoreAreaButton.onClick.AddListener(_OnJumpToStore);
		jumpToSeasonStoreButton.onClick.AddListener(_OnJumpToStore);
		questHintDot.childrenRedDots.Add(seasonQuestPanel.seasonQuestRedDot);
		questHintDot.childrenRedDots.Add(seasonQuestPanel.weeklyQuestRedDot);
		levelLineHintDot.Setup(new LevelLineModule());
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.ReceiveRewardEvent.RemoveListener(_OnReward);
		SwitchPanel obj = switchPanel;
		obj.onTagSwitched = (Action)Delegate.Remove(obj.onTagSwitched, new Action(_OnTagSwitched));
	}

	protected override void UpdateDisplay()
	{
		if (UIBase.applicationState.seasonManager.currentSeason.highseasonPassUnlocked)
		{
			jumpToSeasonStoreButton.gameObject.SetActive(value: false);
			jumpToSeasonStoreCompleteMask.SetActive(value: true);
		}
		else if (UIBase.applicationState.seasonManager.currentSeason.seasonPassUnlocked)
		{
			jumpToSeasonStoreButton.gameObject.SetActive(value: true);
			jumpToSeasonStoreButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_to_unlock_high_season_pass_hint;
			jumpToSeasonStoreCompleteMask.SetActive(value: false);
		}
		else
		{
			jumpToSeasonStoreButton.gameObject.SetActive(value: true);
			jumpToSeasonStoreButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_to_unlock_season_pass_hint;
			jumpToSeasonStoreCompleteMask.SetActive(value: false);
		}
	}

	void IShowAsync.OnShowStart()
	{
		SetActiveAndDirtyWithFadeAnim(active: true);
		switchPanel.targetShowTag = levelLineTag;
		seasonQuestPanel.gameObject.SetActive(value: false);
		manager.ShowUI(seasonPanel);
		SetDirty();
	}

	void IHideAsync.OnHideStart()
	{
		SetActiveAndDirtyWithFadeAnim(active: false);
	}

	private void _OnTagSwitched()
	{
		if (switchPanel.targetShowTag == levelLineTag)
		{
			seasonQuestPanel.SetActiveAndDirty(active: false);
			manager.ShowUI(seasonPanel);
		}
		else if (switchPanel.targetShowTag == questTag)
		{
			seasonQuestPanel.SetActiveAndDirty(active: true);
			seasonPanel.SetActiveAndDirty(active: false);
		}
	}

	public bool ToShowRedDot()
	{
		if (UIBase.applicationState.seasonManager.currentSeason?.seasonLeveLine == null)
		{
			return false;
		}
		if (!UIBase.applicationState.seasonManager.currentSeason.seasonLeveLine.WithAnyUngotReward(UIBase.applicationState) && !UIBase.applicationState.routineQuestManager.currentSeasonQuests.Any((RoutineQuest q) => q.GetRewardAble))
		{
			return UIBase.applicationState.routineQuestManager.currentWeekQuests.Any((RoutineQuest q) => q.GetRewardAble);
		}
		return true;
	}

	private void _OnJumpToStore()
	{
		if (UIBase.fortressState.uiManager.purchaseUIPanel.TryShowPurchaseGroup(PurchaseGroupType.SeasonPass))
		{
			UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
		}
	}

	private void _OnReward(GlobalRewardInfo reward)
	{
		SetDirty();
	}
}
