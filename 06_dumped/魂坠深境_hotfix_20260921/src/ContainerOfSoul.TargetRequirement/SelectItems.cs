using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContainerOfSoul.TargetRequirement;

public class SelectItems : CastPrepare
{
	[SerializeField]
	private ItemTargetSelector pool;

	[SerializeField]
	private CheckNode confirmable;

	[SerializeField]
	private CheckNode selectable;

	[SerializeField]
	private CheckNode shouldCancel;

	[SerializeField]
	private PlayerSelectItemsEffect.SelectPanelType selectPanelType;

	[SerializeField]
	private bool isSingleSelect;

	[SerializeField]
	[LocalizeText("SelectItems施放准备描述")]
	public string panelHint;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { pool, confirmable, selectable, shouldCancel };
	}

	public override void TriggerPrepare(GameEvent eve, Action onConfirm, Action onCancel = null)
	{
		SelectItemInfo selectItemInfo = SelectItemInfo.GenerateNodeSelectItemsInfo(eve, pool, shouldCancel, selectable, confirmable, onConfirm, onCancel, () => true, CastPrepare.GetCasterOwner(eve), panelHint, isSingleSelect);
		selectItemInfo.ResolvePool();
		selectItemInfo.TriggerSelect(eve.gameState, selectPanelType, prepareSelect: true);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(panelHint)
		};
	}
}
