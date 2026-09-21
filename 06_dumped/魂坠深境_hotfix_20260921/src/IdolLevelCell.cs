using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IdolLevelCell : CellUI<int>
{
	[SerializeField]
	private Slider levelSlider;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private IdolLevelArea levelArea;

	[SerializeField]
	private IdolNextLevelArea nextLevelArea;

	[SerializeField]
	private IdolUnavailableArea unavailableArea;

	[SerializeField]
	private LayoutElement layoutElement;

	[SerializeField]
	private float currentLevelMinHeight = 228f;

	[SerializeField]
	private float normalLevelMinHeight = 175f;

	[SerializeField]
	private float nextLevelSliderWidth = 24f;

	[SerializeField]
	private float normalSliderWidth = 12f;

	[NonSerialized]
	private Sequence sequence;

	protected override bool shouldUpdate
	{
		get
		{
			if (base.shouldUpdate)
			{
				return !idolLevelLinePanel.isPlayingAnim;
			}
			return false;
		}
	}

	public int level => base.setupData;

	private IdolLevelLinePanel idolLevelLinePanel => parent as IdolLevelLinePanel;

	public IdolState idolState => idolLevelLinePanel.idolState;

	protected override void Init()
	{
		InitUI(levelArea);
		InitUI(nextLevelArea);
		InitUI(unavailableArea);
	}

	protected override void OnSetup(int setupData)
	{
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		bool flag = base.setupData <= idolState.currentLevel;
		bool isUnavailableLevel = base.setupData == idolState.idolDef.maxLevel + 1;
		bool flag2 = ShouldShowNextLevelArea(base.setupData);
		bool showNextLevelArea = flag2 || ShouldShowNextLevelArea(base.setupData - 1);
		if (levelSlider != null)
		{
			levelSlider.gameObject.SetActive(base.setupData != 0);
			levelSlider.value = (flag ? 1 : 0);
		}
		levelText.text = $"{level}";
		UpdateLayoutHeight(flag2);
		UpdateSliderWidth(showNextLevelArea);
		UpdateRewardAreas(flag2, isUnavailableLevel);
	}

	private bool ShouldShowNextLevelArea(int level)
	{
		bool flag = level == idolState.currentLevel + 1;
		bool flag2 = level == idolState.idolDef.maxLevel + 1;
		if ((level > 0) & flag)
		{
			return !flag2;
		}
		return false;
	}

	private void UpdateLayoutHeight(bool showNextLevelArea)
	{
		layoutElement.minHeight = (showNextLevelArea ? currentLevelMinHeight : normalLevelMinHeight);
	}

	private void UpdateSliderWidth(bool showNextLevelArea)
	{
		RectTransform component = levelSlider.GetComponent<RectTransform>();
		if (!(component == null))
		{
			Vector2 sizeDelta = component.sizeDelta;
			sizeDelta.x = (showNextLevelArea ? nextLevelSliderWidth : normalSliderWidth);
			component.sizeDelta = sizeDelta;
		}
	}

	private void UpdateRewardAreas(bool showNextLevelArea, bool isUnavailableLevel)
	{
		List<GlobalRewardSetup> list = idolState?.idolDef?.GetLevel(level)?.reward;
		bool flag = level > 0 && !isUnavailableLevel && !showNextLevelArea && list != null && list.Count > 0;
		levelArea.gameObject.SetActive(flag);
		if (flag)
		{
			levelArea.Setup(new IdolLevelArea.Data
			{
				idolState = idolState,
				level = level
			});
		}
		nextLevelArea.gameObject.SetActive(showNextLevelArea);
		if (showNextLevelArea)
		{
			nextLevelArea.Setup(new IdolNextLevelArea.Data
			{
				idolState = idolState,
				level = level
			});
		}
		unavailableArea.gameObject.SetActive(isUnavailableLevel);
		if (isUnavailableLevel)
		{
			unavailableArea.Setup(level);
		}
	}

	public void UpgradeAnimStart()
	{
		if (levelSlider != null)
		{
			levelSlider.value = 0f;
		}
	}

	public void UpgradeAnim()
	{
		if (!(levelSlider == null))
		{
			levelSlider.value = 0f;
			sequence?.Kill();
			sequence = DOTween.Sequence();
			sequence.Append(DOTweenModuleUI.DOValue(levelSlider, 1f, idolLevelLinePanel.upgradeAnimTime, false));
			Sequence obj = sequence;
			obj.onKill = (TweenCallback)Delegate.Combine(obj.onKill, (TweenCallback)delegate
			{
				sequence = null;
			});
			sequence.Play();
		}
	}
}
