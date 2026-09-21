using System;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.PathLocationModules;

public class CurrentIconModule : PathLocationDisplay.DisplayModule
{
	[SerializeField]
	private Image currentIcon;

	[SerializeField]
	private Animator currentIconAnim;

	[NonSerialized]
	private bool showing;

	public override void Setup()
	{
		Hide(force: true);
	}

	public override void UpdateDisplay()
	{
		switch (base.pathLocationDisplay.currentState)
		{
		case PathLocationDisplay.State.Disabled:
		case PathLocationDisplay.State.Idle:
		case PathLocationDisplay.State.Finished:
		case PathLocationDisplay.State.Selectable:
			Hide();
			break;
		case PathLocationDisplay.State.Focused:
			if (base.pathLocationDisplay.location.selectable)
			{
				Show();
			}
			else
			{
				Hide();
			}
			break;
		case PathLocationDisplay.State.Current_Unfinished:
		case PathLocationDisplay.State.Current_Finished:
			if (UIDisplayModule.gameState.UIManager.dungeonMapPanel.dungeonMapDisplay.focusedLocationDisplay == null && UIDisplayModule.gameState.UIManager.dungeonMapPanel.dungeonMapDisplay.selectedLocationDisplay == null)
			{
				Show();
			}
			else
			{
				Hide();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void Show()
	{
		if (!showing)
		{
			showing = true;
			((Component)(object)currentIconAnim).gameObject.SetActive(value: true);
			currentIconAnim.SetTrigger(AnimKeywords.show);
		}
	}

	private void Hide(bool force = false)
	{
		if (showing || force)
		{
			showing = false;
			((Component)(object)currentIconAnim).gameObject.SetActive(value: false);
		}
	}

	public void Select()
	{
		((Component)(object)currentIconAnim).gameObject.SetActive(value: true);
		currentIconAnim.SetTrigger(AnimKeywords.select);
	}
}
