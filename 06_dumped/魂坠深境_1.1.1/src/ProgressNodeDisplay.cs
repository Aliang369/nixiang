using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressNodeDisplay : CellUI<int>
{
	[SerializeField]
	private Image nodeImage;

	[NonSerialized]
	private int progress;

	protected override void OnSetup(int dailyContract)
	{
		progress = dailyContract;
		int num = CellUI<int>.gameState.dungeonMapManager.currentDungeonMap.GetVisibleMapProgressLength() - 1;
		float width = (parent.transform as RectTransform).GetWidth();
		Vector2 anchoredPosition = (base.transform as RectTransform).anchoredPosition;
		anchoredPosition.x = (0f - width) / 2f + (float)progress / (float)num * width;
		(base.transform as RectTransform).anchoredPosition = anchoredPosition;
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		int currentLocalProgress = CellUI<int>.gameState.dungeonMapManager.currentLocalProgress;
		int num = CellUI<int>.gameState.dungeonMapManager.currentDungeonMap.GetVisibleMapProgressLength() - 1;
		DungeonMapDisplay.Config dungeonMapDisplayConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig;
		if (progress == 0)
		{
			nodeImage.sprite = dungeonMapDisplayConfig.node_SectionStart;
		}
		else if (progress <= currentLocalProgress)
		{
			nodeImage.sprite = dungeonMapDisplayConfig.node_Finished;
		}
		else if (progress < num)
		{
			nodeImage.sprite = dungeonMapDisplayConfig.node_Unfinished;
		}
		else if (CellUI<int>.gameState.dungeonMapManager.currentDungeonMap.VisibleEndLocation.type == PathLocation.LocationType.Boss)
		{
			nodeImage.sprite = dungeonMapDisplayConfig.node_Boss;
		}
		else
		{
			nodeImage.sprite = dungeonMapDisplayConfig.node_SectionEnd;
		}
	}
}
