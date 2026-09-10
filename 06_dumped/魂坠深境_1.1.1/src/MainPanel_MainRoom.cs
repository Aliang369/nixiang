using Common.UI;
using TMPro;
using UnityEngine;

public class MainPanel_MainRoom : UIBase, IFrameUpdate
{
	private RectTransform commonTopArea;

	[SerializeField]
	private GameButton toBaseGameButton;

	[SerializeField]
	private GameButton toGachaDrawButton;

	[SerializeField]
	private GameButton toCrewButton;

	[SerializeField]
	private GameButton toSkillButton;

	[SerializeField]
	private GameButton toSkinButton;

	[SerializeField]
	private GameObject toSkinLockMark;

	[SerializeField]
	private GameButton storehouseButton;

	[SerializeField]
	private GameButton toShopButton;

	[SerializeField]
	public GameButton toSeasonDetailButton;

	[SerializeField]
	private GameButton toDailyQuestButton;

	[SerializeField]
	public GameButton toAchievementButton;

	[SerializeField]
	private GameButton toNewbieFestivalButton;

	[SerializeField]
	private TextMeshProUGUI newbieFestivalButtonText;

	[SerializeField]
	private GameObject newbieFestivalRedDot;

	private string newbieFestivalButtonTextCache = string.Empty;

	[SerializeField]
	private GameButton toYuanQiActivityButton;

	[SerializeField]
	private TextMeshProUGUI yuanQiActivityButtonText;

	[SerializeField]
	private GameObject yuanQiActivityRedDot;

	[SerializeField]
	private GameButton toIntermissionPlayButton;

	[SerializeField]
	private TextMeshProUGUI intermissionPlayCountdownText;

	[SerializeField]
	private GameObject intermissionPlayRedDot;

	private bool intermissionPlayEntranceExposed;

	public RectTransform ToCrewButtonRect
	{
		get
		{
			if (!(toCrewButton != null))
			{
				return null;
			}
			return toCrewButton.transform as RectTransform;
		}
	}

	public RectTransform ToSkillButtonRect
	{
		get
		{
			if (!(toSkillButton != null))
			{
				return null;
			}
			return toSkillButton.transform as RectTransform;
		}
	}

	protected override void Init()
	{
		toBaseGameButton.onClick.AddListener(_OnToBaseGameButtonClicked);
		toCrewButton.onClick.AddListener(_OnToCrewButtonClicked);
		toGachaDrawButton.onClick.AddListener(_OnToGachaButtonClicked);
		storehouseButton.onClick.AddListener(_OnStorehouseButtonClicked);
		toSkinButton.onClick.AddListener(_OnToSkinButtonClicked);
		toSkillButton.onClick.AddListener(delegate
		{
			HideUniversalCommonInfo();
			UIBase.fortressState.uiManager.ShowUI(UIBase.fortressState.uiManager.skill_GemPanel);
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "地下室-强化护符");
		});
		toShopButton.onClick.AddListener(delegate
		{
			if (UIBase.fortressState.uiManager.purchaseUIPanel.TryShowPurchasePanel())
			{
				HideUniversalCommonInfo();
			}
		});
		toSeasonDetailButton.onClick.AddListener(delegate
		{
			if (ApplicationGlobalVariableManager.EnableSeasonPanel.GetFlag())
			{
				HideUniversalCommonInfo();
				UIBase.fortressState.uiManager.seasonDetailPanel.ShowUI();
				ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-战令界面");
			}
		});
		toDailyQuestButton.onClick.AddListener(delegate
		{
			if (ApplicationGlobalVariableManager.EnableDailyQuestPanel.GetFlag())
			{
				HideUniversalCommonInfo();
				UIBase.fortressState.uiManager.dailyQuestPanel.ShowUI();
				ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-每日任务界面");
			}
		});
		toAchievementButton.onClick.AddListener(delegate
		{
			if (ApplicationGlobalVariableManager.EnableAchievementPanel.GetFlag())
			{
				HideUniversalCommonInfo();
				UIBase.fortressState.uiManager.achievementUIPanel.ShowUI();
				ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "档案馆-成就");
			}
		});
		toNewbieFestivalButton.onClick.AddListener(_OnToNewbieFestivalButtonClicked);
		toYuanQiActivityButton.onClick.AddListener(_OnToYuanQiActivityButtonClicked);
		toIntermissionPlayButton.onClick.AddListener(_OnToIntermissionPlayButtonClicked);
	}

	protected override void UpdateDisplay()
	{
		toGachaDrawButton.gameObject.SetActive(ApplicationGlobalVariableManager.EnableGachaDrawPanel.GetFlag());
		toSkillButton.gameObject.SetActive(ApplicationGlobalVariableManager.EnableSkillUpgrade.GetFlag());
		bool flag = ApplicationGlobalVariableManager.EnableFortressStoreHousePanel.GetFlag();
		storehouseButton.gameObject.SetActive(flag);
		bool active = flag;
		toSkinButton.gameObject.SetActive(active);
		SetToSkinLockMark(locked: false);
		SetToSkinRedDot(!UIBase.fortressState.uiManager.mainUIPanel.SuppressMainInterfaceRedDots && UIBase.applicationState.itemSkinManager.HasUnviewedChangedItemSkin);
		UpdateNewbieFestivalEntrance();
		UpdateYuanQiActivityEntrance();
		UpdateIntermissionPlayEntrance();
		UpdateCommonTopEntries();
	}

	public void FrameUpdate()
	{
		UpdateNewbieFestivalEntrance();
		UpdateYuanQiActivityEntrance();
		UpdateIntermissionPlayEntrance();
	}

	public void PromoteCommonTopArea(Transform commonParent)
	{
		if (commonParent == null)
		{
			return;
		}
		if (commonTopArea == null)
		{
			commonTopArea = base.transform.Find("ShiftInfoArea/TopArea") as RectTransform;
		}
		if (!(commonTopArea == null))
		{
			if (commonTopArea.parent != commonParent)
			{
				commonTopArea.SetParent(commonParent, worldPositionStays: false);
			}
			commonTopArea.SetAsLastSibling();
		}
	}

	public void UpdateCommonTopEntries()
	{
		bool flag = ApplicationGlobalVariableManager.EnableFortressStoreHousePanel.GetFlag();
		SetEntryVisibleAndLocked(toShopButton, flag, !ApplicationGlobalVariableManager.EnablePurchasePanel.GetFlag());
		SetEntryVisibleAndLocked(toSeasonDetailButton, flag, !ApplicationGlobalVariableManager.EnableSeasonPanel.GetFlag());
		SetEntryVisibleAndLocked(toDailyQuestButton, flag, !ApplicationGlobalVariableManager.EnableDailyQuestPanel.GetFlag());
		SetEntryVisibleAndLocked(toAchievementButton, visible: false, locked: false);
	}

	public void _OnToBaseGameButtonClicked()
	{
		HideUniversalCommonInfo();
		UIBase.fortressState.uiManager.ShowUI(UIBase.fortressState.uiManager.baseGameUIPanel);
	}

	private void _OnToCrewButtonClicked()
	{
		if (!UIBase.fortressState.uiManager.transforming)
		{
			HideUniversalCommonInfo();
			UIBase.fortressState.uiManager.characterInfoBasePanel.SetupFromMainRoom(UIBase.applicationState.newGameConfigManager.currentNewGameConfig.currentCharacter.state);
		}
	}

	public void _OnToGachaButtonClicked()
	{
		HideUniversalCommonInfo();
		UIBase.fortressState.uiManager.gachaPanel.ShowDefaultEntryPool();
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-探寻");
	}

	private void _OnStorehouseButtonClicked()
	{
		HideUniversalCommonInfo();
		UIBase.fortressState.uiManager.storehousePanel.Show();
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-仓库");
	}

	private void _OnToSkinButtonClicked()
	{
		HideUniversalCommonInfo();
		UIBase.fortressState.uiManager.storehousePanel.ShowSkinSetOnly();
	}

	private void HideUniversalCommonInfo()
	{
		UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
	}

	private void SetToSkinLockMark(bool locked)
	{
		if (toSkinLockMark != null)
		{
			toSkinLockMark.SetActive(locked);
			return;
		}
		Transform transform = toSkinButton.transform.Find("LockMark");
		if (transform != null)
		{
			transform.gameObject.SetActive(locked);
		}
	}

	private void SetToSkinRedDot(bool show)
	{
		SetChildActive(toSkinButton.transform, "FinishHint", active: false);
		SetChildActive(toSkinButton.transform, "RedDot", show);
		SetChildActive(toSkinButton.transform, "HintRedDot", show);
	}

	private void SetChildActive(Transform parent, string childName, bool active)
	{
		if (parent.name == childName)
		{
			parent.gameObject.SetActive(active);
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			SetChildActive(parent.GetChild(i), childName, active);
		}
	}

	private void SetEntryVisibleAndLocked(GameButton button, bool visible, bool locked)
	{
		button.gameObject.SetActive(visible);
		Transform transform = button.transform.Find("LockMark");
		if (transform != null)
		{
			transform.gameObject.SetActive(visible & locked);
		}
	}

	private void UpdateNewbieFestivalEntrance()
	{
		NewbieFestivalManager newbieFestivalManager = UIBase.applicationState.newbieFestivalManager;
		if (newbieFestivalManager == null || !newbieFestivalManager.isEntranceVisible)
		{
			toNewbieFestivalButton.gameObject.SetActive(value: false);
			newbieFestivalRedDot.SetActive(value: false);
			return;
		}
		toNewbieFestivalButton.gameObject.SetActive(value: true);
		toNewbieFestivalButton.interactable = true;
		newbieFestivalRedDot.SetActive(newbieFestivalManager.ShouldShowEntranceRedDot());
		if (!newbieFestivalManager.TryGetEntranceDisplayText(out var text))
		{
			text = string.Empty;
		}
		if (!(newbieFestivalButtonTextCache == text))
		{
			newbieFestivalButtonTextCache = text;
			newbieFestivalButtonText.text = text;
		}
	}

	private void _OnToNewbieFestivalButtonClicked()
	{
		NewbieFestivalManager newbieFestivalManager = UIBase.applicationState.newbieFestivalManager;
		if (newbieFestivalManager != null && newbieFestivalManager.isEntranceVisible)
		{
			HideUniversalCommonInfo();
			UIBase.fortressState.uiManager.newbieFestivalPanel.ShowUI();
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-新生游艺");
		}
	}

	private void UpdateYuanQiActivityEntrance()
	{
		YuanQiActivityManager yuanQiActivityManager = UIBase.applicationState.yuanQiActivityManager;
		if (yuanQiActivityManager == null || !yuanQiActivityManager.IsEntranceVisible)
		{
			toYuanQiActivityButton.gameObject.SetActive(value: false);
			yuanQiActivityRedDot.SetActive(value: false);
			return;
		}
		toYuanQiActivityButton.gameObject.SetActive(value: true);
		toYuanQiActivityButton.interactable = true;
		yuanQiActivityRedDot.SetActive(yuanQiActivityManager.ShouldShowEntranceRedDot());
		if (!yuanQiActivityManager.TryGetEntranceDisplayText(out var text))
		{
			text = string.Empty;
		}
		yuanQiActivityButtonText.text = text;
	}

	private void _OnToYuanQiActivityButtonClicked()
	{
		YuanQiActivityManager yuanQiActivityManager = UIBase.applicationState.yuanQiActivityManager;
		if (yuanQiActivityManager != null && yuanQiActivityManager.IsEntranceVisible)
		{
			if (yuanQiActivityManager.TryGetDaysUntilOpen(out var days))
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_activity_yuanqi_not_open_yet_remain_arg0_days.FormatWith(days));
				return;
			}
			yuanQiActivityManager.TryRefreshState(isForeground: true);
			HideUniversalCommonInfo();
			UIBase.fortressState.uiManager.yuanQiActivityPanel.ShowUI();
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "主仪式间-元气活动");
		}
	}

	private void UpdateIntermissionPlayEntrance()
	{
		IntermissionPlayManager intermissionPlayManager = UIBase.applicationState.intermissionPlayManager;
		if (!intermissionPlayManager.IsEntranceVisible())
		{
			intermissionPlayEntranceExposed = false;
			toIntermissionPlayButton.gameObject.SetActive(value: false);
			intermissionPlayRedDot.SetActive(value: false);
			return;
		}
		toIntermissionPlayButton.gameObject.SetActive(value: true);
		intermissionPlayCountdownText.text = intermissionPlayManager.GetEntranceActivityCountdownText();
		toIntermissionPlayButton.interactable = true;
		intermissionPlayRedDot.SetActive(intermissionPlayManager.HasRedPoint());
		if (!intermissionPlayEntranceExposed)
		{
			intermissionPlayEntranceExposed = true;
			ThinkingAnalyticsExtends.TrackIntermissionPlayEntranceExpose(UIBase.applicationState, intermissionPlayManager.Model.CurrentDayIndex, intermissionPlayManager.HasRedPoint());
		}
	}

	private void _OnToIntermissionPlayButtonClicked()
	{
		if (UIBase.applicationState.intermissionPlayManager.IsEntranceVisible())
		{
			ThinkingAnalyticsExtends.TrackIntermissionPlayEntranceClick(UIBase.applicationState, UIBase.applicationState.intermissionPlayManager.Model.CurrentDayIndex);
			HideUniversalCommonInfo();
			UIBase.fortressState.uiManager.intermissionPlayPanel.ShowUI();
		}
	}
}
