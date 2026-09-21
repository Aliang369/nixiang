using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContainerOfSoul.TargetRequirement;

public class SelectItemsCount : CastPrepare
{
	[SerializeField]
	private ValueGenerator countGenerator;

	[SerializeField]
	private bool isSingleSelect;

	[SerializeField]
	[LocalizeText("玩家选择卡牌时的提示描述文本")]
	public string panelHint;

	[SerializeField]
	private ItemTargetSelector pool;

	[SerializeField]
	private ItemConstraint selectable;

	[SerializeField]
	private PlayerSelectItemsEffect.SelectPanelType selectPanelType;

	[SerializeField]
	private ValueCompareCheckNode.CompareType type;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { countGenerator, pool, selectable };
	}

	public override void TriggerPrepare(GameEvent eve, Action onConfirm, Action onCancel = null)
	{
		SelectItemInfo selectItemInfo = SelectItemInfo.GenerateNodeSelectItemsCountInfo(eve, pool, countGenerator, selectable, type, onConfirm, onCancel, CastPrepare.GetCasterOwner(eve), () => true, panelHint);
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
