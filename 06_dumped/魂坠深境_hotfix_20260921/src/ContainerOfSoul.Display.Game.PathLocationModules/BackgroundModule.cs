using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.PathLocationModules;

public class BackgroundModule : PathLocationDisplay.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI hintText;

	[SerializeField]
	private Animator backgroundAnimator;

	[SerializeField]
	private Image modifyText;

	[NonSerialized]
	private bool showing;

	[NonSerialized]
	private PrefabPool<Image> modifyTextPool;

	public override void Init()
	{
		modifyTextPool = new PrefabPool<Image>(modifyText);
		modifyText.gameObject.SetActive(value: false);
		backgroundAnimator.cullingMode = (AnimatorCullingMode)0;
		backgroundAnimator.keepAnimatorStateOnDisable = true;
	}

	public override void Setup()
	{
		hintText.text = base.pathLocationDisplay.location.GetLocationDescriptionText();
		modifyTextPool.DespawnAll();
		Hide(force: true);
	}

	public override void UpdateDisplay()
	{
		switch (base.pathLocationDisplay.currentState)
		{
		case PathLocationDisplay.State.Disabled:
		case PathLocationDisplay.State.Idle:
		case PathLocationDisplay.State.Current_Finished:
		case PathLocationDisplay.State.Finished:
			if (base.pathLocationDisplay.dungeonMapDisplay.focusedLocationDisplay == base.pathLocationDisplay)
			{
				Show();
			}
			else
			{
				Hide();
			}
			break;
		case PathLocationDisplay.State.Focused:
			Show();
			break;
		case PathLocationDisplay.State.Selectable:
		{
			PathLocation currentLocation = UIDisplayModule.gameState.dungeonMapManager.currentLocation;
			if (currentLocation == null || currentLocation.finished)
			{
				Show();
			}
			else
			{
				Hide();
			}
			break;
		}
		case PathLocationDisplay.State.Current_Unfinished:
			Show();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void Show()
	{
		TraceLog.Debug($"[BackgroundModule] Show() - [L{base.pathLocationDisplay.location.draftMapIndex}] - {showing}");
		if (!showing)
		{
			showing = true;
			backgroundAnimator.speed = UIDisplayModule.applicationState.settingManager.GetUIScale();
			((Behaviour)(object)backgroundAnimator).enabled = true;
			backgroundAnimator.SetBool(AnimKeywords.show, true);
		}
	}

	private void Hide(bool force = false)
	{
		TraceLog.Debug($"[BackgroundModule] Hide({force}) - [L{base.pathLocationDisplay.location.draftMapIndex}] - {showing}");
		if (showing || force)
		{
			showing = false;
			backgroundAnimator.speed = UIDisplayModule.applicationState.settingManager.GetUIScale();
			((Behaviour)(object)backgroundAnimator).enabled = true;
			backgroundAnimator.SetBool(AnimKeywords.show, false);
			modifyTextPool.DespawnAll();
		}
	}
}
