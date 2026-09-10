using System;
using Common.UI;
using UnityEngine;

public class PVPDefenderBattleRecordPanel : UIBase
{
	public GameButton backButton;

	public PVPDefenderBattleRecordCell battleRecordCellPrefab;

	public Transform battleRecordCellParent;

	[NonSerialized]
	public CellUIPrefabPool<PVPDefenderBattleRecordCell, PVPDefenderBattleRecord> battleRecordCellPool;

	[NonSerialized]
	private PVPDefender defender;

	protected override void Init()
	{
		battleRecordCellPool = GetPrefabPool<PVPDefenderBattleRecordCell, PVPDefenderBattleRecord>(battleRecordCellPrefab, battleRecordCellParent);
		backButton.onClick.AddListener(delegate
		{
			SetActiveAndDirty(active: false);
		});
	}

	public void Setup(PVPDefender defender)
	{
		this.defender = defender;
	}

	protected override void UpdateDisplay()
	{
		if (defender != null)
		{
			battleRecordCellPool.Sync(defender.battleRecords);
		}
	}
}
