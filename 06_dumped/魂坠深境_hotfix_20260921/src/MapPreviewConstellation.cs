using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapPreviewConstellation : UIBase
{
	public Constellation constellation;

	public Image constellationImage;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI text;

	public TextMeshProUGUI timeText;

	public TextMeshProUGUI appendText;

	public GameButton selectButton;

	public GlobalItemDisplay rewardDisplayPrefab;

	public RectTransform rewardDisplayParent;

	public RectTransform rewardDisplayArea;

	public GameObject lockedMask;

	[NonSerialized]
	private StarMapPanel mapPanel;

	[NonSerialized]
	public CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> rewardDisplayPool;

	protected override void Init()
	{
		mapPanel = GetUIInParents<StarMapPanel>();
		selectButton.onClick.AddListener(_OnSelectButtonClicked);
		rewardDisplayPool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(rewardDisplayPrefab, rewardDisplayParent);
	}

	protected override void UpdateDisplay()
	{
		Constellation constellation = mapPanel.ResolveCatalogConstellation(this.constellation);
		lockedMask.SetActive(!CanOpen());
		text.text = constellation.GetNumText();
		titleText.text = constellation.constellationName;
		if (constellation.constellationType == Constellation.ConstellationType.WeeklyChallenge)
		{
			timeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_contract_remain_time_arg0_to_expire.FormatWith(TextDisplayHelper.GetRoughTimeSpanText(DateTime.UtcNow.GetMonday4AM() - DateTime.UtcNow));
		}
		else if (DateTime.UtcNow < constellation.startDate.UtcDateTime)
		{
			timeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_to_unlock_after_arg0.FormatWith(TextDisplayHelper.GetTimeSpanText(constellation.startDate.UtcDateTime - DateTime.UtcNow));
		}
		else if (DateTime.UtcNow < constellation.endDate.UtcDateTime)
		{
			timeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_end_remain1.FormatWith(TextDisplayHelper.GetTimeSpanText(constellation.endDate.UtcDateTime - DateTime.UtcNow));
		}
		else
		{
			timeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_end;
		}
		rewardDisplayPool.DespawnAll();
		if (constellation.reward != null)
		{
			rewardDisplayArea.gameObject.SetActive(value: true);
			foreach (GlobalRewardSetup item in constellation.reward.rewards.OrEmptyIfNull())
			{
				rewardDisplayPool.Spawn(item);
			}
		}
		else
		{
			rewardDisplayArea.gameObject.SetActive(value: false);
		}
		if (constellation.constellationType == Constellation.ConstellationType.SeasonChallenge)
		{
			appendText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_season_fixed_hard_challenge_constellation;
			appendText.color = ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.seasonChallengeConstellationTextColor;
		}
		else if (constellation.constellationType == Constellation.ConstellationType.WeeklyChallenge)
		{
			appendText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_shuffle_weekly_challenge_constellation;
			appendText.color = ScriptableObjectSingleton<VFXDataPreset>.instance.starMapDisplayConfig.dailyChallengeConstellationTextColor;
		}
	}

	public bool CanOpen()
	{
		if (constellation == null)
		{
			return false;
		}
		return (mapPanel?.ResolveCatalogConstellation(constellation) ?? constellation).CanOpen(DateTime.UtcNow);
	}

	private void _OnSelectButtonClicked()
	{
		if (CanOpen())
		{
			mapPanel.OpenConstellation(this);
		}
	}
}
