using System;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

public class PVPModeGameRecordPanel : UIBase
{
	public PVPModeGameRecordCell cellPrefab;

	public RectTransform contentParent;

	public TextMeshProUGUI noRecordHint;

	public GameButton backButton;

	[NonSerialized]
	public CellUIPrefabPool<PVPModeGameRecordCell, GlobalGameRecordManager.GameRecordPreviewInfo> cellPool;

	protected override void Init()
	{
		cellPool = GetPrefabPool<PVPModeGameRecordCell, GlobalGameRecordManager.GameRecordPreviewInfo>(cellPrefab, contentParent);
		backButton.onClick.AddListener(delegate
		{
			SetActiveAndDirty(active: false);
		});
	}

	protected override void UpdateDisplay()
	{
		cellPool.Sync(UIBase.applicationState.pvpModeManager.AllChallengeRecordHistory.ToList());
		noRecordHint.gameObject.SetActive(cellPool.activeCount == 0);
	}
}
