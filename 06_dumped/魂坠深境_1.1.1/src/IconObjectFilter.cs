using System;
using ContainerOfSoul.Display.Fortress.DotManagers;
using UnityEngine;
using UnityEngine.UI;

public class IconObjectFilter : CellUI<IconObjectFilterPanel.IconFilterData>
{
	[SerializeField]
	private GameButton_Passive button;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private HintRedDot redDot;

	[NonSerialized]
	private IconObjectFilterPanel controller;

	[NonSerialized]
	private Func<bool?> shouldShowRedDot;

	protected override void Init()
	{
		button.StateTransition(ButtonSelectionState.Normal, instant: true);
		controller = GetComponentInParent<IconObjectFilterPanel>(includeInactive: true);
		if (redDot != null)
		{
			InitUI(redDot);
			redDot.Setup(new CustomModule
			{
				shouldShow = () => shouldShowRedDot?.Invoke() == true
			});
		}
		button.onClick.AddListener(delegate
		{
			if (controller.selected != this)
			{
				controller.selected = this;
				controller.OnChangeSelected();
			}
			else if (controller.canUnselect)
			{
				controller.selected = null;
				controller.OnChangeSelected();
			}
		});
	}

	protected override void OnSetup(IconObjectFilterPanel.IconFilterData setupData)
	{
		icon.sprite = setupData.icon;
		shouldShowRedDot = setupData.shouldShowRedDot;
	}

	protected override void UpdateDisplay()
	{
		if (controller.selected == this)
		{
			button.StateTransition(ButtonSelectionState.Selected, instant: false);
		}
		else
		{
			button.StateTransition(ButtonSelectionState.Normal, instant: false);
		}
	}
}
