using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress;

public class FortressMainUIPanel : FortressUIManager.SubPanel, IDisplayModulesController
{
	public enum MainPanelLayer
	{
		StargazingRoom = 10,
		IdolRoom,
		Library,
		MainRoom
	}

	private enum CommunityEntryMode
	{
		Hidden,
		Discord,
		BenefitsInfo
	}

	[Serializable]
	public class MainPanelLayerSetup
	{
		public MainPanelLayer layer;

		public MainUISwitchButton button;

		public TextMeshProUGUI buttonText;

		public Image lockImage;

		public GameObject layerPanel;

		public RectTransform layerNPCPivot;

		public RoomRef roomRef;

		private ClavisDef roomLock => layer switch
		{
			MainPanelLayer.StargazingRoom => ScriptableObjectSingleton<GameDataManager>.instance.lockStargazingRoom, 
			MainPanelLayer.IdolRoom => ScriptableObjectSingleton<GameDataManager>.instance.lockIdolRoom, 
			MainPanelLayer.Library => ScriptableObjectSingleton<GameDataManager>.instance.lockLibraryRoom, 
			_ => null, 
		};

		public bool isLocked
		{
			get
			{
				if (roomLock != null)
				{
					return !roomLock.isUnlocked;
				}
				return false;
			}
		}

		public string lockedHint
		{
			get
			{
				if (!roomLock.hiddenClavis)
				{
					return string.Format(ScriptableObjectSingleton<TextDataPreset>.instance.term_room_lock_need_arg0, roomLock.NameWithColor);
				}
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_room_lock_need_explor;
			}
		}
	}

	[SerializeField]
	private MainPanel_IdolRoom mainPanelIdolRoom;

	[SerializeField]
	public MainPanel_MainRoom mainPanelMainRoom;

	[SerializeField]
	private StoryPoint suppressRedDotsUntilStoryPointFinished;

	[SerializeField]
	private GameButton ToIndex;

	[SerializeField]
	private TextMeshProUGUI indexBookCollectRateText;

	[SerializeField]
	private TextMeshProUGUI indexBookTitleText;

	[SerializeField]
	private GameButton ToRecord;

	[SerializeField]
	private GameButton ToAchievement;

	[SerializeField]
	private GameButton fortressSettingButton;

	[SerializeField]
	private GameButton fortressAnnouncementButton;

	[SerializeField]
	private GameButton enterPVPModeButton;

	[SerializeField]
	private TextMeshProUGUI pvpModeRemainOpenTimeText;

	[SerializeField]
	private FestivalCheckInEntry festivalCheckInEntry;

	[SerializeField]
	private GameButton monthCheckButton;

	[SerializeField]
	private GameButton openMailPanelButton;

	[SerializeField]
	private GameButton forerunnerLegacyButton;

	[SerializeField]
	private OpenTestQuestionnairePanel openTestQuestionnairePanel;

	[SerializeField]
	private GameButton bugReportButton;

	[SerializeField]
	private MatterCell creditMatterDisplay;

	[SerializeField]
	private MatterCell characterMatterDisplay;

	[SerializeField]
	private MatterCell essenceMatterDisplay;

	[SerializeField]
	private RectTransform matterDescriptionPanel;

	[SerializeField]
	private GameObject announcementHintRedDot;

	[SerializeField]
	private GameObject mailHintRedDot;

	[SerializeField]
	private GameObject dailyCheckInRedDot;

	[SerializeField]
	private GameObject eventCheckInRedDot;

	[SerializeField]
	private GameObject seasonDetailRedDot;

	[SerializeField]
	private GameObject dailyQuestInRedDot;

	[SerializeField]
	private GameObject achievementRedDot;

	[SerializeField]
	private TextMeshProUGUI dailyQuestStateText;

	[SerializeField]
	private PlayerInfoPanel playerInfoPanel;

	[SerializeField]
	private GameButton discordButton;

	[SerializeField]
	private Image communityEntryIcon;

	[SerializeField]
	private Sprite overseaCommunityIcon;

	[SerializeField]
	private Sprite chinaCommunityIcon;

	[SerializeField]
	private GameObject discordCommunityText;

	[SerializeField]
	private GameObject benefitsInfoText;

	[NonSerialized]
	private RectTransform discordButtonRect;

	[NonSerialized]
	private Vector2 discordDefaultAnchoredPosition;

	[NonSerialized]
	private Vector2 discordFestivalSlotAnchoredPosition;

	[NonSerialized]
	private CommunityEntryMode communityEntryMode;

	[NonSerialized]
	private GameObject forerunnerLegacyRedDot;

	[SerializeField]
	public MainUISwitchGroup mainUISwitchGroup;

	[NonSerialized]
	public List<FortressNPC> fortressNpcs;

	public MainPanel_IdolRoom MainPanelIdolRoom => mainPanelIdolRoom;

	public bool SuppressMainInterfaceRedDots
	{
		get
		{
			if (suppressRedDotsUntilStoryPointFinished != null && UIBase.applicationState?.storyPointManager != null)
			{
				return !suppressRedDotsUntilStoryPointFinished.IsFinished(UIBase.applicationState);
			}
			return false;
		}
	}

	protected override void Init()
	{
		if (TryResolveMainPanelIdolRoom())
		{
			InitUI(mainPanelIdolRoom);
		}
		InitUI(mainPanelMainRoom);
		InitUI(openTestQuestionnairePanel);
		InitUI(characterMatterDisplay);
		InitUI(creditMatterDisplay);
		InitUI(essenceMatterDisplay);
		InitUI(mainUISwitchGroup);
		InitUI(playerInfoPanel);
		InitUI(festivalCheckInEntry);
		SetupMatterDisplay();
		base.gameObject.SetActive(value: true);
		mainPanelMainRoom.PromoteCommonTopArea(base.transform);
		communityEntryMode = GetCommunityEntryMode();
		SetupDiscordButtonLayout();
		SetupCommunityEntryDisplay();
		fortressNpcs = new List<FortressNPC>();
		ResolveLibraryAchievementEntry();
		ResolveForerunnerLegacyRedDot();
		ToIndex.onClick.AddListener(delegate
		{
			if (IsIndexBookUnlocked())
			{
				HideUniversalCommonInfo();
				manager.ShowUI(UIBase.fortressState.uiManager.indexUIPanel);
			}
		});
		ToIndex.onNotInteractableClick.AddListener(delegate
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableFortressStoreHousePanel), 2f);
		});
		fortressSettingButton.onClick.AddListener(_OnFortressSettingButtonClicked);
		fortressAnnouncementButton.onClick.AddListener(_OnFortressAnnouncementButtonClicked);
		monthCheckButton.onClick.AddListener(delegate
		{
			HideUniversalCommonInfo();
			UIBase.fortressState.uiManager.monthCheckPanel.ShowUI();
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "通用-每日签到");
		});
		openMailPanelButton.onClick.AddListener(delegate
		{
			HideUniversalCommonInfo();
			UIBase.fortressState.uiManager.fortressMailPanel.FetchMailAndShow();
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "通用-邮箱");
		});
		forerunnerLegacyButton.onClick.AddListener(_OnToForerunnerButtonClicked);
		enterPVPModeButton.onClick.AddListener(_OnEnterPVPModeButtonClicked);
		discordButton.onClick.AddListener(_OnCommunityEntryButtonClicked);
		ToRecord.onClick.AddListener(_OnToRecordButtonClicked);
		if (ToAchievement != null)
		{
			ToAchievement.onClick.AddListener(delegate
			{
				if (IsAchievementUnlocked())
				{
					HideUniversalCommonInfo();
					UIBase.fortressState.uiManager.achievementUIPanel.ShowUI();
					ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "档案馆-成就");
				}
			});
			ToAchievement.onNotInteractableClick.AddListener(delegate
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableAchievementPanel), 2f);
			});
		}
		UIBase.applicationState.eventManager.matterChangeEvent.AddListener(_OnMatterChange);
		UIBase.applicationState.eventManager.achievementAchieveEvent.AddListener(_OnAchievementAchieved);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		RefreshForerunnerRewardStateIfStale();
		UpdateFortressRoomEnable();
		UpdateDailyQuestState();
		mainPanelMainRoom.UpdateCommonTopEntries();
		bool flag = festivalCheckInEntry.ShouldShow();
		UpdateDiscordButtonVisibility(flag);
		festivalCheckInEntry.gameObject.SetActive(flag);
		if (flag)
		{
			festivalCheckInEntry.Refresh();
		}
		monthCheckButton.gameObject.SetActive(UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableDailyCheckIn));
		openMailPanelButton.gameObject.SetActive(UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableMailButton));
		ResolveForerunnerLegacyRedDot();
		if (forerunnerLegacyRedDot != null)
		{
			forerunnerLegacyRedDot.SetActive(ShouldShowForerunnerLegacyRewardRedDot());
		}
		seasonDetailRedDot.SetActive(ShouldShowSeasonRedDot());
		if (achievementRedDot != null)
		{
			achievementRedDot.SetActive(ShouldShowAchievementRedDot());
		}
		if (ScriptableObjectSingleton<PVPModeDateManager>.instance.IsPVPModeGameUnlocked)
		{
			enterPVPModeButton.interactable = true;
			pvpModeRemainOpenTimeText.gameObject.SetActive(value: false);
			return;
		}
		enterPVPModeButton.interactable = false;
		pvpModeRemainOpenTimeText.gameObject.SetActive(value: true);
		pvpModeRemainOpenTimeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unlock_after_time_arg0.FormatWith(TextDisplayHelper.GetRoughTimeSpanText(ScriptableObjectSingleton<PVPModeDateManager>.instance.RemainTimeToUnlockPVPMode));
	}

	private void UpdateFortressRoomEnable()
	{
		ResolveLibraryAchievementEntry();
		MainPanelLayerSetup mainPanelLayerSetup = mainUISwitchGroup.mainPanelLayerSetups.FirstOrDefault((MainPanelLayerSetup s) => s.layer == MainPanelLayer.Library);
		bool flag = mainPanelLayerSetup != null && !mainPanelLayerSetup.isLocked;
		bool flag2 = IsIndexBookUnlocked();
		bool flag3 = IsAchievementUnlocked();
		ToIndex.gameObject.SetActive(flag);
		ToIndex.interactable = flag2;
		if (ToAchievement != null)
		{
			ToAchievement.gameObject.SetActive(flag);
			ToAchievement.interactable = flag3;
		}
		SetButtonLockMark(ToIndex, flag && !flag2);
		SetButtonLockMark(ToAchievement, flag && !flag3);
		UpdateIndexBookCollectRateText(flag & flag2);
		forerunnerLegacyButton.gameObject.SetActive(UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressForerunnerLegacyPanel));
		enterPVPModeButton.gameObject.SetActive(ScriptableObjectSingleton<FortressDataManager>.instance.enablePVPMode && UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnablePurchasePanel));
		openTestQuestionnairePanel.gameObject.SetActive(value: false);
	}

	private void ResolveForerunnerLegacyRedDot()
	{
		if (forerunnerLegacyRedDot == null && forerunnerLegacyButton != null)
		{
			forerunnerLegacyRedDot = forerunnerLegacyButton.transform.Find("HintRedDot")?.gameObject;
		}
	}

	private bool ShouldShowForerunnerLegacyRewardRedDot()
	{
		if (!SuppressMainInterfaceRedDots && UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressForerunnerLegacyPanel))
		{
			return UIBase.applicationState.forerunnerManager.HasPendingShareReward;
		}
		return false;
	}

	private void RefreshForerunnerRewardStateIfStale()
	{
		if (UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressForerunnerLegacyPanel))
		{
			UIBase.applicationState.forerunnerManager.RefreshForerunnerProfileDataIfStale(delegate
			{
				SetDirty();
			});
		}
	}

	private void UpdateIndexBookCollectRateText(bool visible)
	{
		if (indexBookCollectRateText == null)
		{
			indexBookCollectRateText = ToIndex.transform.Find("CollectRateText")?.GetComponent<TextMeshProUGUI>();
		}
		if (!(indexBookCollectRateText == null))
		{
			indexBookCollectRateText.gameObject.SetActive(visible);
			if (visible)
			{
				float allPageCollectAvgRatio = UIBase.applicationState.discoverManager.GetAllPageCollectAvgRatio();
				indexBookCollectRateText.text = TextDisplayHelper.NumToPercentageWithSmallPercentSign(allPageCollectAvgRatio);
				UpdateIndexBookCollectRateLayout();
			}
		}
	}

	private void UpdateIndexBookCollectRateLayout()
	{
		if (indexBookTitleText == null)
		{
			indexBookTitleText = ToIndex.transform.Find("Text (TMP)")?.GetComponent<TextMeshProUGUI>();
		}
		if (!(indexBookTitleText == null))
		{
			RectTransform rectTransform = indexBookTitleText.rectTransform;
			RectTransform rectTransform2 = indexBookCollectRateText.rectTransform;
			indexBookTitleText.ForceMeshUpdate();
			float num = indexBookTitleText.GetRenderedValues(onlyVisibleCharacters: false).x;
			if (num <= 0f)
			{
				num = Mathf.Min(indexBookTitleText.preferredWidth, rectTransform.rect.width);
			}
			rectTransform2.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + num + 14f, rectTransform2.anchoredPosition.y);
		}
	}

	private void SetButtonLockMark(GameButton button, bool locked)
	{
		if (!(button == null))
		{
			Transform transform = button.transform.Find("LockMark");
			if (transform != null)
			{
				transform.gameObject.SetActive(locked);
			}
		}
	}

	private void ResolveLibraryAchievementEntry()
	{
		if (ToAchievement == null && ToIndex != null)
		{
			ToAchievement = ToIndex.transform.parent.Find("ToAchievement")?.GetComponent<GameButton>();
		}
		GameObject gameObject = ((!(ToAchievement != null)) ? null : ToAchievement.transform.Find("HintRedDot")?.gameObject);
		if (gameObject != null)
		{
			achievementRedDot = gameObject;
		}
	}

	private bool IsIndexBookUnlocked()
	{
		return UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableFortressStoreHousePanel);
	}

	private void SetupDiscordButtonLayout()
	{
		discordButtonRect = discordButton.transform as RectTransform;
		discordDefaultAnchoredPosition = discordButtonRect.anchoredPosition;
		discordFestivalSlotAnchoredPosition = discordDefaultAnchoredPosition;
		RectTransform rectTransform = festivalCheckInEntry.ButtonTransform.parent as RectTransform;
		if (!(rectTransform == null) && !(rectTransform.parent != discordButtonRect.parent))
		{
			discordFestivalSlotAnchoredPosition.x = rectTransform.anchoredPosition.x + discordButtonRect.rect.width * discordButtonRect.pivot.x;
		}
	}

	private void UpdateDiscordButtonVisibility(bool festivalCheckInVisible)
	{
		discordButton.gameObject.SetActive(communityEntryMode != CommunityEntryMode.Hidden && IsIndexBookUnlocked());
		discordButtonRect.anchoredPosition = (festivalCheckInVisible ? discordDefaultAnchoredPosition : discordFestivalSlotAnchoredPosition);
	}

	private static CommunityEntryMode GetCommunityEntryMode()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		ChannelType channelType = CSCommon.channelType;
		if ((int)channelType == 2)
		{
			return CommunityEntryMode.Discord;
		}
		if ((int)channelType == 10)
		{
			return CommunityEntryMode.BenefitsInfo;
		}
		if ((int)channelType == 3)
		{
			if (!GlobalConfigManager.instance.isOverSea)
			{
				return CommunityEntryMode.BenefitsInfo;
			}
			return CommunityEntryMode.Discord;
		}
		return CommunityEntryMode.Hidden;
	}

	private void SetupCommunityEntryDisplay()
	{
		switch (communityEntryMode)
		{
		case CommunityEntryMode.Discord:
			SetCommunityEntryIcon(overseaCommunityIcon);
			discordCommunityText.SetActive(value: true);
			benefitsInfoText.SetActive(value: false);
			break;
		case CommunityEntryMode.BenefitsInfo:
			SetCommunityEntryIcon(chinaCommunityIcon);
			discordCommunityText.SetActive(value: false);
			benefitsInfoText.SetActive(value: true);
			break;
		default:
			discordCommunityText.SetActive(value: false);
			benefitsInfoText.SetActive(value: false);
			break;
		}
	}

	private void SetCommunityEntryIcon(Sprite sprite)
	{
		RectTransform rectTransform = communityEntryIcon.rectTransform;
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		float num = anchoredPosition.x - rectTransform.rect.width * rectTransform.pivot.x;
		communityEntryIcon.sprite = sprite;
		communityEntryIcon.SetNativeSize();
		anchoredPosition.x = num + rectTransform.rect.width * rectTransform.pivot.x;
		rectTransform.anchoredPosition = anchoredPosition;
	}

	private bool IsAchievementUnlocked()
	{
		return UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableAchievementPanel);
	}

	private void UpdateDailyQuestState()
	{
		int count = UIBase.applicationState.routineQuestManager.currentDailyQuests.Count;
		int num = UIBase.applicationState.routineQuestManager.currentDailyQuests.Count((RoutineQuest q) => q.AllLayerFinished);
		bool flag = IsDailyQuestPanelUnlocked();
		dailyQuestStateText.transform.parent.gameObject.SetActive(flag && (UIBase.applicationState.routineQuestManager.currentDailyQuests.Count((RoutineQuest q) => q.GetRewardAble) > 0 || UIBase.applicationState.routineQuestManager.currentDailyQuests.Count((RoutineQuest q) => !q.AllLayerFinished) > 0));
		dailyQuestStateText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(num, count);
		dailyQuestInRedDot.SetActive(ShouldShowDailyQuestRedDot());
	}

	protected override void Dispose()
	{
		discordButton.onClick.RemoveListener(_OnCommunityEntryButtonClicked);
		UIBase.applicationState.eventManager.matterChangeEvent.RemoveListener(_OnMatterChange);
		UIBase.applicationState.eventManager.achievementAchieveEvent.RemoveListener(_OnAchievementAchieved);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	private void _OnCommunityEntryButtonClicked()
	{
		string text = communityEntryMode switch
		{
			CommunityEntryMode.Discord => GlobalConfigManager.instance.discordLink, 
			CommunityEntryMode.BenefitsInfo => GlobalConfigManager.instance.benefitsInfoLink, 
			_ => null, 
		};
		if (!string.IsNullOrEmpty(text))
		{
			Application.OpenURL(text);
		}
	}

	private void _OnFortressSettingButtonClicked()
	{
		HideUniversalCommonInfo();
		UIBase.fortressState.uiManager.settingPanel.SetActiveAndDirtyWithFadeAnim(active: true);
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "通用-设置");
	}

	private void _OnFortressAnnouncementButtonClicked()
	{
		HideUniversalCommonInfo();
		UIBase.fortressState.uiManager.fortressAnnouncementPanel.OpenAnnouncementPanel();
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "通用-公告");
	}

	private void _OnToForerunnerButtonClicked()
	{
		HideUniversalCommonInfo();
		StartCoroutine(ShowForerunnerLegacyOverviewPanelWhenReadyCor());
	}

	private IEnumerator ShowForerunnerLegacyOverviewPanelWhenReadyCor()
	{
		UIBase.applicationState.forerunnerManager.RefreshForerunnerProfileData(delegate
		{
			SetDirty();
		});
		if (UIBase.applicationState.forerunnerManager.forerunnerProfileDataLoading)
		{
			WaitPanelBase.Token waitPanel = BootstrapExtra.instance.waitPanelBase.Show(base.textData.gameAsyncOperationText.term_forerunnerProfileDataLoading);
			yield return new WaitWhile(() => UIBase.applicationState.forerunnerManager.forerunnerProfileDataLoading);
			BootstrapExtra.instance.waitPanelBase.Hide(waitPanel);
		}
		if (!UIBase.applicationState.forerunnerManager.forerunnerProfileDataLoaded)
		{
			BootstrapExtra.instance.blockPanel.Show(ScriptableObjectSingleton<TextDataPreset>.instance.term_forerunner_data_load_failed, null, BootstrapExtra.instance.blockPanel.Hide, null, ScriptableObjectSingleton<TextDataPreset>.instance.term_return);
			yield break;
		}
		UIBase.fortressState.uiManager.forerunnerLegacyOverviewPanel.ShowUI();
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "档案馆-先行者遗物");
	}

	private void _OnEnterPVPModeButtonClicked()
	{
		HideUniversalCommonInfo();
		StartCoroutine(ShowPVPModeMainPanelWhenReadyCor());
	}

	private IEnumerator ShowPVPModeMainPanelWhenReadyCor()
	{
		if (!UIBase.applicationState.pvpModeManager.pvpModeProfileDataLoaded)
		{
			if (UIBase.applicationState.pvpModeManager.pvpModeProfileDataLoading)
			{
				WaitPanelBase.Token waitPanel = BootstrapExtra.instance.waitPanelBase.Show(base.textData.gameAsyncOperationText.term_pvpModeProfileDataLoading);
				yield return new WaitWhile(() => UIBase.applicationState.pvpModeManager.pvpModeProfileDataLoading);
				BootstrapExtra.instance.waitPanelBase.Hide(waitPanel);
			}
			if (!UIBase.applicationState.pvpModeManager.pvpModeProfileDataLoaded)
			{
				BootstrapExtra.instance.blockPanel.Show(ScriptableObjectSingleton<TextDataPreset>.instance.term_pvp_data_load_failed, null, BootstrapExtra.instance.blockPanel.Hide, null, ScriptableObjectSingleton<TextDataPreset>.instance.term_return);
				yield break;
			}
		}
		yield return WaitRecordProfileDataReadyCor();
		if (UIBase.applicationState.recordManager.recordProfileDataLoaded)
		{
			UIBase.fortressState.uiManager.pvpModeMainPanel.ShowUI();
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "异步联机-PVP模式主界面");
		}
	}

	private void _OnToRecordButtonClicked()
	{
		HideUniversalCommonInfo();
		StartCoroutine(ShowRecordPanelWhenReadyCor());
	}

	private IEnumerator ShowRecordPanelWhenReadyCor()
	{
		yield return WaitRecordProfileDataReadyCor();
		if (UIBase.applicationState.recordManager.recordProfileDataLoaded)
		{
			UIBase.fortressState.uiManager.recordPanel.SetActiveAndDirtyWithFadeAnim(active: true);
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "档案馆-游戏记录与统计数据");
		}
	}

	private IEnumerator WaitRecordProfileDataReadyCor()
	{
		if (UIBase.applicationState.recordManager.recordProfileDataLoaded)
		{
			yield break;
		}
		if (UIBase.applicationState.recordManager.recordProfileDataLoading)
		{
			WaitPanelBase.Token waitPanel = BootstrapExtra.instance.waitPanelBase.Show(base.textData.gameAsyncOperationText.term_load_game_record);
			yield return new WaitWhile(() => UIBase.applicationState.recordManager.recordProfileDataLoading);
			BootstrapExtra.instance.waitPanelBase.Hide(waitPanel);
		}
		if (!UIBase.applicationState.recordManager.recordProfileDataLoaded)
		{
			BootstrapExtra.instance.blockPanel.Show(ScriptableObjectSingleton<TextDataPreset>.instance.term_game_record_data_load_failed, null, BootstrapExtra.instance.blockPanel.Hide, null, ScriptableObjectSingleton<TextDataPreset>.instance.term_return);
		}
	}

	private void _OnMatterChange(MatterInfo mi)
	{
		SetupMatterDisplay();
		SetDirty();
	}

	private void _OnAchievementAchieved(Achievement achievement)
	{
		SetDirty();
	}

	private void SetupMatterDisplay()
	{
		characterMatterDisplay.Setup(UIBase.applicationState.matterManager.CommonCurrencyMatter);
		creditMatterDisplay.Setup(UIBase.applicationState.matterManager.CreditMatter);
		essenceMatterDisplay.Setup(UIBase.applicationState.matterManager.EssenceMatter);
		matterDescriptionPanel.gameObject.SetActive(value: false);
	}

	public void ConfigureMatterDisplayInfoPanel(ICommonInfoPanel infoPanel)
	{
		characterMatterDisplay.SetInfoPanel(infoPanel);
		creditMatterDisplay.SetInfoPanel(infoPanel);
		essenceMatterDisplay.SetInfoPanel(infoPanel);
	}

	private void _OnEmptyAreaClicked()
	{
		HideUniversalCommonInfo();
		matterDescriptionPanel.gameObject.SetActive(value: false);
	}

	private void HideUniversalCommonInfo()
	{
		UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
	}

	public void TrySpawnNPC(FortressNPC npcPrefab)
	{
		MainPanelLayerSetup mainPanelLayerSetup = mainUISwitchGroup.mainPanelLayerSetups.Find((MainPanelLayerSetup t) => t.layer == npcPrefab.layerType);
		FortressNPC fortressNPC = UnityEngine.Object.Instantiate(npcPrefab, mainPanelLayerSetup.layerNPCPivot);
		InitUI(fortressNPC);
		fortressNpcs.Add(fortressNPC);
	}

	private bool TryResolveMainPanelIdolRoom()
	{
		if (mainPanelIdolRoom != null)
		{
			return true;
		}
		if (mainUISwitchGroup == null)
		{
			return false;
		}
		MainPanelLayerSetup mainPanelLayerSetup = mainUISwitchGroup.mainPanelLayerSetups?.Find((MainPanelLayerSetup t) => t.layer == MainPanelLayer.IdolRoom);
		if (mainPanelLayerSetup?.layerPanel == null)
		{
			return false;
		}
		mainPanelIdolRoom = mainPanelLayerSetup.layerPanel.GetComponent<MainPanel_IdolRoom>();
		if (mainPanelIdolRoom == null)
		{
			Debug.LogWarning("FortressMainUIPanel skipped mainPanelIdolRoom initialization because the prefab reference could not be resolved.", this);
		}
		return mainPanelIdolRoom != null;
	}

	private bool IsSeasonPanelUnlocked()
	{
		return UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSeasonPanel);
	}

	private bool IsDailyQuestPanelUnlocked()
	{
		return UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableDailyQuestPanel);
	}

	private bool ShouldShowSeasonRedDot()
	{
		if (!SuppressMainInterfaceRedDots && IsSeasonPanelUnlocked())
		{
			return UIBase.fortressState.uiManager.seasonDetailPanel.ToShowRedDot();
		}
		return false;
	}

	private bool ShouldShowDailyQuestRedDot()
	{
		if (!SuppressMainInterfaceRedDots && IsDailyQuestPanelUnlocked())
		{
			return UIBase.applicationState.routineQuestManager.currentDailyQuests.Count((RoutineQuest q) => q.GetRewardAble) > 0;
		}
		return false;
	}

	private bool ShouldShowAchievementRedDot()
	{
		if (!SuppressMainInterfaceRedDots && IsAchievementUnlocked())
		{
			return UIBase.applicationState.achievementManager.globalAchievements.Count((Achievement q) => q.GetRewardAble) > 0;
		}
		return false;
	}
}
