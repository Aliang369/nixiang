using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using UnityEngine;

public class PVPDefenderRankPanel : UIBase
{
	public PVPDefenderRankCell defenderRankCellPrefab;

	public Transform defenderRankCellParent;

	public GameButton backButton;

	[NonSerialized]
	public CellUIPrefabPool<PVPDefenderRankCell, PVPDefenderRankData> defenderRankCellPool;

	[NonSerialized]
	private List<PVPDefenderRankData> defenderRankDataList;

	protected override void Init()
	{
		defenderRankCellPool = GetPrefabPool<PVPDefenderRankCell, PVPDefenderRankData>(defenderRankCellPrefab, defenderRankCellParent);
		backButton.onClick.AddListener(delegate
		{
			SetActiveAndDirty(active: false);
		});
	}

	protected override void UpdateDisplay()
	{
		if (defenderRankDataList == null)
		{
			FetchUpdateRankData(delegate
			{
				defenderRankCellPool.Sync(defenderRankDataList);
			});
		}
		else
		{
			defenderRankCellPool.Sync(defenderRankDataList);
		}
	}

	public async Task FetchUpdateRankData(Action afterUpdate)
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			FetchPVPRankListRequest body = new FetchPVPRankListRequest
			{
				PageIndex = 0
			};
			FetchPVPRankListResult fetchPVPRankListResult = await UIBase.applicationState.ChillyRoomSDK.PVP.FetchPVPRankListAsync(body);
			NetworkConvertContext context = new NetworkConvertContext(UIBase.applicationState);
			defenderRankDataList = fetchPVPRankListResult.DefenderRankDataList?.Select((ChillyRoom.Services.ContainerOfSoul.PVPDefenderRankData r) => r.ToClient(context)).ToList() ?? new List<PVPDefenderRankData>();
			afterUpdate?.Invoke();
		}
	}
}
