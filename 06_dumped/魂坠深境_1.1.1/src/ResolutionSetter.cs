using System;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionSetter : MonoBehaviour
{
	public static Action refresh;

	[SerializeField]
	private CanvasScaler scaler;

	[NonSerialized]
	private int width;

	[NonSerialized]
	private int height;

	private void Awake()
	{
		width = Screen.width;
		height = Screen.height;
		RefreshResolution();
		refresh = (Action)Delegate.Combine(refresh, new Action(RefreshResolution));
	}

	private void OnDestroy()
	{
		refresh = (Action)Delegate.Remove(refresh, new Action(RefreshResolution));
	}

	private void Update()
	{
		if (!(base.gameObject == null) && (width != Screen.width || height != Screen.height))
		{
			width = Screen.width;
			height = Screen.height;
			RefreshResolution();
		}
	}

	private void RefreshResolution()
	{
		switch (PlatformHelper.GetPlatform())
		{
		case PlatformHelper.Type.PC:
			scaler.referenceResolution = new Vector2(1920f, 1450f);
			scaler.matchWidthOrHeight = 1f;
			break;
		default:
			scaler.referenceResolution = RenderingManager.BaseResolution;
			if ((float)width / (float)height > RenderingManager.NormalResolutionAspectRatio)
			{
				scaler.matchWidthOrHeight = 1f;
			}
			else
			{
				scaler.matchWidthOrHeight = 0f;
			}
			break;
		}
		UISafeArea[] componentsInChildren = GetComponentsInChildren<UISafeArea>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Refresh();
		}
	}
}
