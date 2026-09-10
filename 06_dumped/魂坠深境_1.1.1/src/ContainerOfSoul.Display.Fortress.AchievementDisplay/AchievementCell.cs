using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.AchievementDisplay;

public class AchievementCell : CellUI<Achievement>
{
	private static readonly Color activeIconColor = new Color(1f, 1f, 1f, 1f);

	private static readonly Color finishedDateTextColor = new Color(0.690566f, 0.6402352f, 0.5198789f, 1f);

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image gotMark;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private GameButton receiveRewardButton;

	[SerializeField]
	private TextMeshProUGUI achieveCountText;

	[SerializeField]
	private TextMeshProUGUI receiveRewardButtonText;

	[SerializeField]
	private GlobalItemDisplay rewardCellPrefab;

	[SerializeField]
	private RectTransform rewardCellParent;

	[NonSerialized]
	public Achievement achievement;

	[NonSerialized]
	private TextLinkObjectDisplayer descriptionTextLinkObjectDisplayer;

	[NonSerialized]
	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> rewardCellPool;

	[NonSerialized]
	private FortressAchievementUIPanel parentPanel;

	[NonSerialized]
	private Vector2 gotMarkAnchoredPosition;

	[NonSerialized]
	private Vector2 achieveCountTextAnchorMin;

	[NonSerialized]
	private Vector2 achieveCountTextAnchorMax;

	[NonSerialized]
	private Vector2 achieveCountTextAnchoredPosition;

	[NonSerialized]
	private Vector2 achieveCountTextSizeDelta;

	[NonSerialized]
	private Vector2 achieveCountTextPivot;

	[NonSerialized]
	private Color achieveCountTextColor;

	[NonSerialized]
	private bool achieveCountTextEnableAutoSizing;

	[NonSerialized]
	private float achieveCountTextFontSizeMin;

	[NonSerialized]
	private float achieveCountTextFontSizeMax;

	[NonSerialized]
	private TextAlignmentOptions achieveCountTextAlignment;

	protected override void Init()
	{
		rewardCellPool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(rewardCellPrefab, rewardCellParent);
		receiveRewardButton.onClick.AddListener(_OnReceiveRewardButtonClicked);
		parentPanel = GetUIInParents<FortressAchievementUIPanel>();
		descriptionTextLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<Achievement>.applicationState, descriptionText, base.rectTransform);
		gotMarkAnchoredPosition = gotMark.rectTransform.anchoredPosition;
		achieveCountTextAnchorMin = achieveCountText.rectTransform.anchorMin;
		achieveCountTextAnchorMax = achieveCountText.rectTransform.anchorMax;
		achieveCountTextAnchoredPosition = achieveCountText.rectTransform.anchoredPosition;
		achieveCountTextSizeDelta = achieveCountText.rectTransform.sizeDelta;
		achieveCountTextPivot = achieveCountText.rectTransform.pivot;
		achieveCountTextColor = achieveCountText.color;
		achieveCountTextEnableAutoSizing = achieveCountText.enableAutoSizing;
		achieveCountTextFontSizeMin = achieveCountText.fontSizeMin;
		achieveCountTextFontSizeMax = achieveCountText.fontSizeMax;
		achieveCountTextAlignment = achieveCountText.alignment;
	}

	protected override void OnSetup(Achievement dailyContract)
	{
		achievement = dailyContract;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		icon.sprite = achievement.achievementDef.icon;
		nameText.text = achievement.achievementDef.achievementName;
		descriptionText.text = achievement.GetDescription(CellUI<Achievement>.applicationState, out var generator);
		descriptionTextLinkObjectDisplayer.SetGenerator(generator);
		rewardCellPool.DespawnAll();
		foreach (GlobalRewardSetup achieveReward in achievement.achievementDef.AchieveRewards)
		{
			rewardCellPool.Spawn(achieveReward);
		}
		if (achievement.Type == AchievementDef.AchievementType.GlobalAchievement)
		{
			receiveRewardButton.gameObject.SetActive(value: true);
			achieveCountText.gameObject.SetActive(value: false);
			receiveRewardButton.interactable = achievement.achieved && !achievement.achieved;
			receiveRewardButton.gameObject.SetActive(!achievement.rewardReceived);
			gotMark.gameObject.SetActive(achievement.rewardReceived);
			UpdateFinishedDateDisplay();
			if (!achievement.achieved)
			{
				icon.color = activeIconColor;
				receiveRewardButton.interactable = false;
				receiveRewardButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_achievement_lock;
			}
			else if (!achievement.rewardReceived)
			{
				icon.color = activeIconColor;
				receiveRewardButton.interactable = parentPanel?.CanClaimAchievementRewards ?? true;
				receiveRewardButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_achievement_get_bonus;
			}
			else if (achievement.rewardReceived)
			{
				icon.color = activeIconColor;
				receiveRewardButton.interactable = false;
				receiveRewardButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_achievement_bonus_got;
			}
			switch (achievement.Layer)
			{
			case AchievementDef.AchievementLayer.Intro:
				icon.color *= ScriptableObjectSingleton<FortressDataManager>.instance.introAchievementColor;
				break;
			case AchievementDef.AchievementLayer.Progress:
				icon.color *= ScriptableObjectSingleton<FortressDataManager>.instance.progressAchievementColor;
				break;
			default:
				icon.color *= ScriptableObjectSingleton<FortressDataManager>.instance.specialAchievementColor;
				break;
			}
		}
		else
		{
			icon.color = activeIconColor;
			receiveRewardButton.gameObject.SetActive(value: false);
			ResetFinishedDateDisplay();
			achieveCountText.gameObject.SetActive(value: true);
		}
	}

	private void UpdateFinishedDateDisplay()
	{
		if (!achievement.rewardReceived || string.IsNullOrWhiteSpace(achievement.finishedLocalDate))
		{
			ResetFinishedDateDisplay();
			return;
		}
		gotMark.rectTransform.anchoredPosition = gotMarkAnchoredPosition + new Vector2(0f, 16f);
		RectTransform obj = achieveCountText.rectTransform;
		RectTransform rectTransform = receiveRewardButton.transform as RectTransform;
		obj.anchorMin = rectTransform.anchorMin;
		obj.anchorMax = rectTransform.anchorMax;
		obj.pivot = new Vector2(0.5f, 0.5f);
		obj.anchoredPosition = rectTransform.anchoredPosition + new Vector2(0f, -42f);
		obj.sizeDelta = new Vector2(286f, 36f);
		achieveCountText.enableAutoSizing = true;
		achieveCountText.fontSizeMin = 14f;
		achieveCountText.fontSizeMax = 22f;
		achieveCountText.alignment = TextAlignmentOptions.Center;
		achieveCountText.color = finishedDateTextColor;
		achieveCountText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_achievement_finished_date_arg0.FormatWith(achievement.finishedLocalDate);
		achieveCountText.gameObject.SetActive(value: true);
	}

	private void ResetFinishedDateDisplay()
	{
		gotMark.rectTransform.anchoredPosition = gotMarkAnchoredPosition;
		RectTransform obj = achieveCountText.rectTransform;
		obj.anchorMin = achieveCountTextAnchorMin;
		obj.anchorMax = achieveCountTextAnchorMax;
		obj.anchoredPosition = achieveCountTextAnchoredPosition;
		obj.sizeDelta = achieveCountTextSizeDelta;
		obj.pivot = achieveCountTextPivot;
		achieveCountText.color = achieveCountTextColor;
		achieveCountText.enableAutoSizing = achieveCountTextEnableAutoSizing;
		achieveCountText.fontSizeMin = achieveCountTextFontSizeMin;
		achieveCountText.fontSizeMax = achieveCountTextFontSizeMax;
		achieveCountText.alignment = achieveCountTextAlignment;
		achieveCountText.text = string.Empty;
		achieveCountText.gameObject.SetActive(value: false);
	}

	private void _OnReceiveRewardButtonClicked()
	{
		parentPanel?.ClaimAchievementRewards(achievement);
	}
}
