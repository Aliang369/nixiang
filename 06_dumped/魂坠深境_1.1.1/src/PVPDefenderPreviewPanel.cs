using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public class PVPDefenderPreviewPanel : UIBase
{
	public Button closeButton;

	public PVPDefenderPreviewCell defenderPreviewCellPrefab;

	public Transform contentParent;

	[NonSerialized]
	private CellUIPrefabPool<PVPDefenderPreviewCell, PVPDefender> defenderPreviewCellPool;

	protected override void Init()
	{
		defenderPreviewCellPool = GetPrefabPool<PVPDefenderPreviewCell, PVPDefender>(defenderPreviewCellPrefab, contentParent);
		closeButton.onClick.AddListener(OnCloseButtonClick);
	}

	protected override void UpdateDisplay()
	{
		List<PVPDefender> list = UIBase.gameState?.newGameConfigData?.startGameInfo?.pvpDefenderThisGame ?? new List<PVPDefender>();
		defenderPreviewCellPool.Sync(list);
	}

	private void OnCloseButtonClick()
	{
		SetActiveAndDirty(active: false);
	}
}
