using System;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FinalRenderCamera : MonoBehaviour
{
	[SerializeField]
	public Camera finalCamera;

	[SerializeField]
	public Camera environmentCamera;

	[SerializeField]
	public Camera environmentCenterCamera;

	[NonSerialized]
	private int width;

	[NonSerialized]
	private int height;

	public float CameraFOV
	{
		get
		{
			return environmentCamera.fieldOfView;
		}
		set
		{
			environmentCamera.fieldOfView = value;
			environmentCenterCamera.fieldOfView = value;
		}
	}

	public float EnvCenterRenderTextureScale
	{
		get
		{
			return (float)environmentCenterCamera.targetTexture.width / (float)width;
		}
		set
		{
			if (environmentCenterCamera.targetTexture != null)
			{
				environmentCenterCamera.targetTexture.Release();
			}
			Vector2Int scaledTargetResolution = GetScaledTargetResolution(environmentCenterCamera, value);
			environmentCenterCamera.targetTexture = new RenderTexture(scaledTargetResolution.x, scaledTargetResolution.y, 16, RenderTextureFormat.ARGB32);
			environmentCenterCamera.targetTexture.name = $"敌人渲染RT-{scaledTargetResolution.x}{scaledTargetResolution.y}]";
			environmentCenterCamera.targetTexture.filterMode = FilterMode.Trilinear;
			((ScriptableRendererData)RenderingManager.Renderer_final).rendererFeatures.OfType<FullScreenPassRendererFeature>().ForEach(delegate(FullScreenPassRendererFeature f)
			{
				f.passMaterial.mainTexture = environmentCenterCamera.targetTexture;
			});
		}
	}

	public float EnvRenderTextureScale
	{
		get
		{
			return (float)environmentCamera.targetTexture.width / (float)width;
		}
		set
		{
			if (environmentCamera.targetTexture != null)
			{
				environmentCamera.targetTexture.Release();
			}
			Vector2Int scaledTargetResolution = GetScaledTargetResolution(environmentCamera, value);
			environmentCamera.targetTexture = new RenderTexture(scaledTargetResolution.x, scaledTargetResolution.y, 16, RenderTextureFormat.ARGB32);
			environmentCamera.targetTexture.name = $"环境渲染RT-{scaledTargetResolution.x}{scaledTargetResolution.y}]";
			environmentCamera.targetTexture.filterMode = FilterMode.Trilinear;
			((ScriptableRendererData)RenderingManager.Renderer_envCenter_PostProcess).rendererFeatures.OfType<FullScreenPassRendererFeature>().ForEach(delegate(FullScreenPassRendererFeature f)
			{
				f.passMaterial.mainTexture = environmentCamera.targetTexture;
			});
			((ScriptableRendererData)RenderingManager.Renderer_envCenter_NoPostProcess).rendererFeatures.OfType<FullScreenPassRendererFeature>().ForEach(delegate(FullScreenPassRendererFeature f)
			{
				f.passMaterial.mainTexture = environmentCamera.targetTexture;
			});
		}
	}

	private Vector2Int GetScaledTargetResolution(Camera cam, float scale)
	{
		Vector2 targetResolution = new Vector2(scale * (float)RenderingManager.BaseResolution.x, scale * (float)RenderingManager.BaseResolution.y);
		Vector2 fitResolution = GetFitResolution(cam, new Vector2(Screen.width, Screen.height), targetResolution);
		return new Vector2Int((int)fitResolution.x, (int)fitResolution.y);
	}

	private Vector2 GetFitResolution(Camera cam, Vector2 screenResolution, Vector2 targetResolution)
	{
		if (cam.gateFit == Camera.GateFitMode.Vertical)
		{
			if (screenResolution.y < targetResolution.y)
			{
				return screenResolution;
			}
			return screenResolution * (targetResolution.y / screenResolution.y);
		}
		if (cam.gateFit == Camera.GateFitMode.Horizontal)
		{
			if (screenResolution.x < targetResolution.x)
			{
				return screenResolution;
			}
			return screenResolution * (targetResolution.x / screenResolution.x);
		}
		return screenResolution;
	}

	private void Awake()
	{
		width = Screen.width;
		height = Screen.height;
		environmentCamera.forceIntoRenderTexture = true;
		EnvCenterRenderTextureScale = 1f;
		EnvRenderTextureScale = 1f;
		RefreshResolution();
		SetupSortOrder();
	}

	private void SetupSortOrder()
	{
		environmentCenterCamera.transparencySortMode = TransparencySortMode.CustomAxis;
		environmentCenterCamera.transparencySortAxis = Vector3.forward;
		environmentCamera.transparencySortMode = TransparencySortMode.CustomAxis;
		environmentCamera.transparencySortAxis = Vector3.forward;
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
			if ((float)width / (float)height > 1.7777778f)
			{
				finalCamera.gateFit = Camera.GateFitMode.Vertical;
				environmentCenterCamera.gateFit = Camera.GateFitMode.Vertical;
				environmentCamera.gateFit = Camera.GateFitMode.Vertical;
			}
			else
			{
				finalCamera.gateFit = Camera.GateFitMode.Horizontal;
				environmentCenterCamera.gateFit = Camera.GateFitMode.Vertical;
				environmentCamera.gateFit = Camera.GateFitMode.Vertical;
			}
		}
	}
}
