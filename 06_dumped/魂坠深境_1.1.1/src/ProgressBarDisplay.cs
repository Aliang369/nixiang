using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarDisplay : UIBase
{
	[SerializeField]
	private Image progressbar;

	[SerializeField]
	private Image currentNodeDisplay;

	[SerializeField]
	private Image nextNodeDisplay;

	[SerializeField]
	private ProgressNodeDisplay progressNodeDisplayPrefab;

	[SerializeField]
	private RectTransform progressNodeDisplayParent;

	[NonSerialized]
	private CellUIPrefabPool<ProgressNodeDisplay, int> progressNodeDisplayPool;

	[NonSerialized]
	private List<ProgressNodeDisplay> progressNodeDisplays;

	protected override void Init()
	{
		progressNodeDisplayPool = GetPrefabPool<ProgressNodeDisplay, int>(progressNodeDisplayPrefab, progressNodeDisplayParent);
		progressNodeDisplays = new List<ProgressNodeDisplay>();
	}

	protected override void UpdateDisplay()
	{
		UpdateCurrentNode();
		UpdateNextNode();
	}

	private void UpdateCurrentNode()
	{
		int currentLocalProgress = UIBase.gameState.dungeonMapManager.currentLocalProgress;
		if (currentLocalProgress < 0 || currentLocalProgress >= progressNodeDisplays.Count)
		{
			currentNodeDisplay.gameObject.SetActive(value: false);
			return;
		}
		currentNodeDisplay.gameObject.SetActive(value: true);
		currentNodeDisplay.transform.position = progressNodeDisplays[currentLocalProgress].transform.position;
	}

	private void UpdateNextNode()
	{
		int num = UIBase.gameState.dungeonMapManager.currentLocalProgress + 1;
		if (num < 0 || num >= progressNodeDisplays.Count)
		{
			nextNodeDisplay.gameObject.SetActive(value: false);
			return;
		}
		nextNodeDisplay.gameObject.SetActive(value: true);
		nextNodeDisplay.transform.position = progressNodeDisplays[num].transform.position;
	}

	public void Setup()
	{
		SetProgressBarMaterial();
		int visibleMapProgressLength = UIBase.gameState.dungeonMapManager.currentDungeonMap.GetVisibleMapProgressLength();
		for (int i = 0; i < visibleMapProgressLength; i++)
		{
			ProgressNodeDisplay item = progressNodeDisplayPool.Spawn(i);
			progressNodeDisplays.Add(item);
		}
	}

	public void Unset()
	{
		progressNodeDisplayPool.DespawnAll();
		progressNodeDisplays.Clear();
	}

	private void SetProgressBarMaterial()
	{
		progressbar.material = UnityEngine.Object.Instantiate(progressbar.material);
		GradientColorKey[] colorKeys = ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig.progressBarGradient.colorKeys;
		Level currentLevel = UIBase.gameState.contractState.currentLevel;
		if ((object)currentLevel != null && currentLevel.overrideDungeonMapColor)
		{
			colorKeys = UIBase.gameState.contractState.currentLevel.dungeonMapProgressColor.colorKeys;
		}
		Material materialForRendering = progressbar.materialForRendering;
		if (colorKeys.Length > 1)
		{
			materialForRendering.SetColor(ShaderFields._Color1, colorKeys[0].color);
			materialForRendering.SetFloat(ShaderFields._Point1, colorKeys[0].time);
		}
		else
		{
			materialForRendering.SetColor(ShaderFields._Color1, Color.white);
			materialForRendering.SetFloat(ShaderFields._Point1, 1f);
		}
		if (colorKeys.Length > 2)
		{
			materialForRendering.SetColor(ShaderFields._Color2, colorKeys[1].color);
			materialForRendering.SetFloat(ShaderFields._Point2, colorKeys[1].time);
		}
		else
		{
			materialForRendering.SetColor(ShaderFields._Color2, Color.white);
			materialForRendering.SetFloat(ShaderFields._Point2, 1f);
		}
		if (colorKeys.Length > 2)
		{
			materialForRendering.SetColor(ShaderFields._Color3, colorKeys[2].color);
			materialForRendering.SetFloat(ShaderFields._Point3, colorKeys[2].time);
		}
		else
		{
			materialForRendering.SetColor(ShaderFields._Color3, Color.white);
			materialForRendering.SetFloat(ShaderFields._Point3, 1f);
		}
		if (colorKeys.Length > 3)
		{
			materialForRendering.SetColor(ShaderFields._Color4, colorKeys[3].color);
			materialForRendering.SetFloat(ShaderFields._Point4, colorKeys[3].time);
		}
		else
		{
			materialForRendering.SetColor(ShaderFields._Color4, Color.white);
			materialForRendering.SetFloat(ShaderFields._Point4, 1f);
		}
		if (colorKeys.Length > 4)
		{
			materialForRendering.SetColor(ShaderFields._Color5, colorKeys[4].color);
			materialForRendering.SetFloat(ShaderFields._Point5, colorKeys[4].time);
		}
		else
		{
			materialForRendering.SetColor(ShaderFields._Color5, Color.white);
			materialForRendering.SetFloat(ShaderFields._Point5, 1f);
		}
		if (colorKeys.Length > 5)
		{
			materialForRendering.SetColor(ShaderFields._Color6, colorKeys[5].color);
			materialForRendering.SetFloat(ShaderFields._Point6, colorKeys[5].time);
		}
		else
		{
			materialForRendering.SetColor(ShaderFields._Color6, Color.white);
			materialForRendering.SetFloat(ShaderFields._Point6, 1f);
		}
	}
}
