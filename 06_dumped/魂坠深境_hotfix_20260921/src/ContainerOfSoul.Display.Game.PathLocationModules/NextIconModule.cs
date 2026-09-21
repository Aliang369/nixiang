using System;
using UnityEngine;

namespace ContainerOfSoul.Display.Game.PathLocationModules;

public class NextIconModule : PathLocationDisplay.DisplayModule
{
	[SerializeField]
	private Animator nextIconAnim;

	[NonSerialized]
	private bool showing;

	public override void UpdateDisplay()
	{
		switch (base.pathLocationDisplay.currentState)
		{
		case PathLocationDisplay.State.Disabled:
		case PathLocationDisplay.State.Idle:
		case PathLocationDisplay.State.Current_Unfinished:
		case PathLocationDisplay.State.Current_Finished:
		case PathLocationDisplay.State.Finished:
			Hide();
			break;
		case PathLocationDisplay.State.Selectable:
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
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void Show()
	{
		if (!showing)
		{
			showing = true;
			((Component)(object)nextIconAnim).gameObject.SetActive(value: true);
			nextIconAnim.SetTrigger(AnimKeywords.show);
		}
	}

	private void Hide(bool force = false)
	{
		if (showing || force)
		{
			showing = false;
			((Component)(object)nextIconAnim).gameObject.SetActive(value: false);
		}
	}
}
