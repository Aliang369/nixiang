using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class PathLineDisplay : CellUI<PathConnectionLine>
{
	[SerializeField]
	private EffectLineRenderer lineRenderer;

	[SerializeField]
	private RectTransform arrow;

	private PathConnectionLine connectionLine => base.setupData;

	protected override void OnSetup(PathConnectionLine _)
	{
		SetupLinePoints();
	}

	protected override void UpdateDisplay()
	{
		UpdateColor();
	}

	private void SetupLinePoints()
	{
		List<Vector3> list = MathHelper.SampleBezierLinePoints(connectionLine.controlPoints, 30);
		lineRenderer.Points = list.ConvertAll((Converter<Vector3, Vector2>)((Vector3 v3) => v3)).ToArray();
		arrow.anchoredPosition = list[list.Count - 1];
		arrow.transform.localRotation = Quaternion.Euler(0f, 0f, Vector3.SignedAngle(list[list.Count - 1] - list[list.Count - 2], Vector3.right, Vector3.back));
	}

	private void UpdateColor()
	{
		DungeonMapDisplay.Config dungeonMapDisplayConfig = CellUI<PathConnectionLine>.applicationState.vfxDataPreset.dungeonMapDisplayConfig;
		if (CellUI<PathConnectionLine>.gameState.recordManager.currentLevelVisitedLocations.Contains(connectionLine.from) && CellUI<PathConnectionLine>.gameState.recordManager.currentLevelVisitedLocations.Contains(connectionLine.to))
		{
			lineRenderer.color = dungeonMapDisplayConfig.finishedColor;
			lineRenderer.material = dungeonMapDisplayConfig.normalMaterial;
			arrow.GetComponent<Image>().color = dungeonMapDisplayConfig.finishedColor;
		}
		else if (CellUI<PathConnectionLine>.gameState.dungeonMapManager.currentLocation == connectionLine.from && CellUI<PathConnectionLine>.gameState.dungeonMapManager.CanJump(connectionLine.to))
		{
			lineRenderer.color = dungeonMapDisplayConfig.canJumpColor;
			lineRenderer.material = dungeonMapDisplayConfig.canReachMaterial;
			arrow.GetComponent<Image>().color = dungeonMapDisplayConfig.canJumpColor;
		}
		else if (CellUI<PathConnectionLine>.gameState.dungeonMapManager.currentLocation == connectionLine.from || (CellUI<PathConnectionLine>.gameState.dungeonMapManager.CanReach(connectionLine.from) && CellUI<PathConnectionLine>.gameState.dungeonMapManager.CanReach(connectionLine.to)))
		{
			lineRenderer.color = dungeonMapDisplayConfig.canReachColor;
			lineRenderer.material = dungeonMapDisplayConfig.normalMaterial;
			arrow.GetComponent<Image>().color = dungeonMapDisplayConfig.canReachColor;
		}
		else
		{
			lineRenderer.color = dungeonMapDisplayConfig.disableColor;
			arrow.GetComponent<Image>().color = dungeonMapDisplayConfig.disableColor;
			lineRenderer.material = dungeonMapDisplayConfig.normalMaterial;
		}
	}

	private void Set()
	{
		Setup(base.setupData);
	}
}
