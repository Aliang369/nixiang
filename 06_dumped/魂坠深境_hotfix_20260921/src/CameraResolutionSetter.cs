using System;
using UnityEngine;

public class CameraResolutionSetter : MonoBehaviour
{
	[SerializeField]
	private Camera camera;

	[NonSerialized]
	private int width;

	[NonSerialized]
	private int height;

	private void Awake()
	{
		width = Screen.width;
		height = Screen.height;
		RefreshResolution();
	}

	private void Update()
	{
		if (width != Screen.width || height != Screen.height)
		{
			width = Screen.width;
			height = Screen.height;
			RefreshResolution();
		}
	}

	private void RefreshResolution()
	{
		PlatformHelper.Type platform = PlatformHelper.GetPlatform();
		if (platform != PlatformHelper.Type.PC)
		{
			_ = platform - 2;
			_ = 1;
			if ((float)width / (float)height > RenderingManager.NormalResolutionAspectRatio)
			{
				camera.gateFit = Camera.GateFitMode.Vertical;
			}
			else
			{
				camera.gateFit = Camera.GateFitMode.Horizontal;
			}
		}
	}
}
