using System.Collections.Generic;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Fortress.Purchase;
using UnityEngine;

public class PVPModePurchasePanel : UIBase
{
	public GameButton backButton;

	public MatterCell matterCellPrefab;

	public RectTransform matterCellParent;

	public CellPurchaseGroupDisplay cellPurchaseGroupDisplay;

	private CellUIPrefabPool<MatterCell, MatterInfo> matterCellPrefabPool;

	protected override void Init()
	{
		InitUI(cellPurchaseGroupDisplay);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		matterCellPrefabPool = GetPrefabPool<MatterCell, MatterInfo>(matterCellPrefab, matterCellParent);
		UIBase.applicationState.eventManager.ReceiveRewardEvent.AddListener(_OnGetReward);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.ReceiveRewardEvent.RemoveListener(_OnGetReward);
	}

	protected override void UpdateDisplay()
	{
		if (ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpModePurchaseGroup != null)
		{
			cellPurchaseGroupDisplay.Setup(ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpModePurchaseGroup);
			cellPurchaseGroupDisplay.SetActiveAndDirty(active: true);
		}
		matterCellPrefabPool.DespawnAll();
		List<MatterInfo> list = new List<MatterInfo>();
		list.Add(UIBase.applicationState.matterManager.GetMatterInfo(ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpBonusMatter));
		list.AddRange(ScriptableObjectSingleton<PVPModeDateManager>.instance.resetPVPCompCostMatters.Select((MatterInfo m) => UIBase.applicationState.matterManager.GetMatterInfo(m.matterDef)));
		matterCellPrefabPool.Sync(list);
	}

	private void _OnBackButtonClicked()
	{
		SetActiveAndDirty(active: false);
	}

	private void _OnGetReward(GlobalRewardInfo rewardInfo)
	{
		SetDirty();
	}
}
