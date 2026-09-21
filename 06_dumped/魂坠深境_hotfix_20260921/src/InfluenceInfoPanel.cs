using System;
using Common.UI;
using UnityEngine;

public class InfluenceInfoPanel : CommonInfoPanelBase, IInfoPanelController
{
	[SerializeField]
	private InfluenceDisplay influenceDisplayPrefab;

	[SerializeField]
	private RectTransform playerInfluencePivot;

	[SerializeField]
	private RectTransform enemyInfluencePivot;

	[NonSerialized]
	private RectTransform influenceDisplayParent;

	[NonSerialized]
	private CellUIPrefabPool<InfluenceDisplay, Influence> influenceDisplayPool;

	bool IInfoPanelController.HasShowingPanel()
	{
		return influenceDisplayPool.activeCount > 0;
	}

	void IInfoPanelController.OnHideAllInfo()
	{
		HideAllCommonInfo();
	}

	protected override void Init()
	{
		base.Init();
		influenceDisplayParent = influenceDisplayPrefab.transform.parent.RectTransform();
		influenceDisplayPool = GetPrefabPool<InfluenceDisplay, Influence>(influenceDisplayPrefab, influenceDisplayParent);
		UIBase.gameState.UIManager.infoPanelManager.AddController(this);
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.gameState.UIManager.infoPanelManager.RemoveController(this);
	}

	protected override void UpdateInfo()
	{
		if (currentCommonInfoData == null)
		{
			influenceDisplayPool.DespawnAll();
			return;
		}
		influenceDisplayPool.DespawnAll();
		Influence influence = currentCommonInfoData.target as Influence;
		InfluenceDisplay influenceDisplay = influenceDisplayPool.Spawn(influence);
		if (influence != null && influence.OwnerBft?.isPlayer == true)
		{
			influenceDisplay.transform.position = playerInfluencePivot.transform.position;
		}
		else
		{
			influenceDisplay.transform.position = enemyInfluencePivot.transform.position;
		}
		UnityHelper.MoveTargetRectToNeighborRectInAreaRect(influenceDisplay.rectTransform, currentCommonInfoData.pivotRectTransform, base.rectTransform);
	}
}
