using System;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class FortressGameRecordPanel : UIBase
{
	[SerializeField]
	public ReocrdInfoPanel recordInfoPanel;

	[SerializeField]
	private TextMeshProUGUI emptyRecordHintText;

	[SerializeField]
	private GameRecordInfoCell gameRecordCellPrefab;

	[SerializeField]
	private Transform gameRecordCellPivot;

	[NonSerialized]
	private CellUIPrefabPool<GameRecordInfoCell, GlobalGameRecordManager.GameRecordPreviewInfo> challengeInfoCellPool;

	protected override void Init()
	{
		InitUI(recordInfoPanel);
		recordInfoPanel.gameObject.SetActive(value: false);
		challengeInfoCellPool = GetPrefabPool<GameRecordInfoCell, GlobalGameRecordManager.GameRecordPreviewInfo>(gameRecordCellPrefab, gameRecordCellPivot);
	}

	protected override void UpdateDisplay()
	{
		emptyRecordHintText.gameObject.SetActive(UIBase.applicationState.recordManager.records.IsNullOrEmpty());
		challengeInfoCellPool.DespawnAll();
		foreach (GlobalGameRecordManager.GameRecordPreviewInfo item in UIBase.applicationState.recordManager.records.OrderByDescending((GlobalGameRecordManager.GameRecordPreviewInfo r) => r.finishDateTime))
		{
			challengeInfoCellPool.Spawn(item);
		}
	}

	public void ShowRecordInfoPanel(GameRecord record)
	{
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "档案馆-游戏记录详情");
		recordInfoPanel.Setup(record);
		recordInfoPanel.gameObject.SetActive(value: true);
	}

	public void ShowRecordInfoOverlay(GameRecord record, Transform overlayRoot)
	{
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "档案馆-游戏记录详情");
		recordInfoPanel.ShowAsOverlay(record, overlayRoot);
	}
}
