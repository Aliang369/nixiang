using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonProgressBriefPanel : UIBase
{
	private struct ProgressState
	{
		public int level;

		public int exp;
	}

	public TextMeshProUGUI currentLevelText;

	public TextMeshProUGUI seasonRemainTimeText;

	public TextMeshProUGUI currentLevelExpProgressText;

	public TextMeshProUGUI weeklyExpLimitText;

	public Scrollbar currentLevelExpProgressBar;

	public GameButton buyLevelButton;

	private SeasonDetailPanel parentPanel;

	private const float progressAddPercentPerSecond = 1f;

	[NonSerialized]
	private ProgressState currentProgressState;

	[NonSerialized]
	private ProgressState targetProgressState;

	private Season CurrentSeason => UIBase.applicationState.seasonManager.currentSeason;

	private LevelLine CurrentSeasonLevelLine => CurrentSeason.seasonLeveLine;

	protected override void Init()
	{
		parentPanel = GetUIInParents<SeasonDetailPanel>();
		UIBase.applicationState.eventManager.changeExpResultExecuteEvent.AddListener(_OnChangeExpRequest);
		buyLevelButton.onClick.AddListener(_OnBuyLevelButtonClicked);
		SyncDisplayProgress();
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.changeExpResultExecuteEvent.RemoveListener(_OnChangeExpRequest);
	}

	private void OnEnable()
	{
		SyncDisplayProgress();
	}

	private void OnDisable()
	{
		SyncDisplayProgress();
	}

	private void SyncDisplayProgress()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		targetProgressState = new ProgressState
		{
			level = CurrentSeasonLevelLine.currentLevelIndex,
			exp = CurrentSeasonLevelLine.currentExp
		};
		currentProgressState = targetProgressState;
		Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_dream_credit);
	}

	protected override void UpdateDisplay()
	{
		TimeSpan span = CurrentSeason.def.endTime.UtcDateTime - DateTime.UtcNow;
		seasonRemainTimeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_remain_time_arg0.FormatWith(TextDisplayHelper.GetTimeSpanText(span));
		if (!CurrentSeasonLevelLine.IsMaxLevel)
		{
			targetProgressState = new ProgressState
			{
				level = CurrentSeasonLevelLine.currentLevelIndex,
				exp = CurrentSeasonLevelLine.currentExp
			};
		}
		else
		{
			targetProgressState = new ProgressState
			{
				level = CurrentSeasonLevelLine.MaxLevelIndex,
				exp = CurrentSeasonLevelLine.LastLevel.expCost
			};
		}
		weeklyExpLimitText.gameObject.SetActive(parentPanel.seasonQuestPanel.questTypeSwitchPanel.targetShowTag == parentPanel.seasonQuestPanel.weeklyQuestTag);
		weeklyExpLimitText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_this_week_season_exp_limit_arg0_per_arg1.FormatWith(UIBase.applicationState.seasonManager.gotQuestExpThisWeek, UIBase.applicationState.seasonManager.FinalWeeklyQuestSeasonExpLimit);
		buyLevelButton.interactable = CurrentSeasonLevelLine.MaxLevelIndex > CurrentSeasonLevelLine.currentLevelIndex;
	}

	private void Update()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		int num = CurrentSeasonLevelLine.GetLineLevel(currentProgressState.level)?.expCost ?? CurrentSeasonLevelLine.LastLevel.expCost;
		bool num2 = currentProgressState.level == CurrentSeasonLevelLine.MaxLevelIndex;
		if (num2 || (targetProgressState.level <= currentProgressState.level && targetProgressState.exp <= currentProgressState.exp))
		{
			Bootstrap.instance.audioManager.StopSoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_dream_credit);
			buyLevelButton.interactable = CurrentSeasonLevelLine.MaxLevelIndex > CurrentSeasonLevelLine.currentLevelIndex;
		}
		else
		{
			if (!Bootstrap.instance.audioManager.IsSustainedEventPlaying(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_dream_credit))
			{
				Bootstrap.instance.audioManager.PlaySoleSustained(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_dream_credit);
			}
			int num3 = currentProgressState.exp + Mathf.CeilToInt((float)num * 1f * Time.deltaTime);
			if (num3 >= num)
			{
				currentProgressState.level++;
				currentProgressState.exp = 0;
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_dream_target);
			}
			else
			{
				if (currentProgressState.level == targetProgressState.level)
				{
					num3 = Math.Min(num3, targetProgressState.exp);
				}
				currentProgressState.exp = num3;
			}
			buyLevelButton.interactable = false;
		}
		currentLevelText.text = currentProgressState.level.ToString();
		if (num2)
		{
			currentLevelExpProgressText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(0, 0);
			currentLevelExpProgressBar.size = 1f;
		}
		else
		{
			currentLevelExpProgressText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(currentProgressState.exp, num);
			currentLevelExpProgressBar.size = (float)currentProgressState.exp / (float)num;
		}
	}

	private void _OnChangeExpRequest(LevelLine.ChangeExpResult result)
	{
		SetDirty();
	}

	private void _OnBuyLevelButtonClicked()
	{
		if (CurrentSeasonLevelLine.MaxLevelIndex > CurrentSeasonLevelLine.currentLevelIndex)
		{
			parentPanel.buyLevelOfLineLevelPanel.Setup(CurrentSeasonLevelLine);
			parentPanel.buyLevelOfLineLevelPanel.SetActiveAndDirty(active: true);
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-战令界面-购买等级");
		}
	}
}
