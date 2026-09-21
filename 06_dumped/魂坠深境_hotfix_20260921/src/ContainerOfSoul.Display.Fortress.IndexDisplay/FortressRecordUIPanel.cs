using System;
using DG.Tweening;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class FortressRecordUIPanel : UIBase
{
	[SerializeField]
	public GameButton backButton;

	[SerializeField]
	private SwitchPanel switchPanel;

	[SerializeField]
	private SwitchTag gameRecordTag;

	[SerializeField]
	private SwitchTag StatisticTag;

	[SerializeField]
	private FortressGameRecordPanel recordPanel;

	[SerializeField]
	private GameStatisticDisplayPanel statisticPanel;

	[SerializeField]
	private RectTransform itemInfoDisplayPivot;

	[NonSerialized]
	private Sequence tweenSequence;

	protected override void Init()
	{
		InitUI(recordPanel);
		InitUI(statisticPanel);
		InitUI(switchPanel);
		SwitchPanel obj = switchPanel;
		obj.onTagSwitched = (Action)Delegate.Combine(obj.onTagSwitched, (Action)delegate
		{
			recordPanel.gameObject.SetActive(value: false);
			statisticPanel.gameObject.SetActive(value: false);
			if (switchPanel.targetShowTag == gameRecordTag)
			{
				recordPanel.SetActiveAndDirty(active: true);
			}
			else if (switchPanel.targetShowTag == StatisticTag)
			{
				statisticPanel.SetActiveAndDirty(active: true);
			}
			SetDirty();
		});
		backButton.onClick.AddListener(delegate
		{
			SetActiveAndDirtyWithFadeAnim(active: false);
		});
		base.gameObject.SetActive(value: false);
		itemInfoDisplayPivot.gameObject.SetActive(value: true);
		switchPanel.Select(gameRecordTag);
	}

	public void ShowRecordInfoPanel(GameRecord record)
	{
		if (record != null)
		{
			SetActiveAndDirtyWithFadeAnim(active: true);
			recordPanel.ShowRecordInfoPanel(record);
		}
	}

	public void ShowRecordInfoOverlay(GameRecord record, Transform overlayRoot)
	{
		if (record != null && !(overlayRoot == null))
		{
			recordPanel.ShowRecordInfoOverlay(record, overlayRoot);
		}
	}
}
