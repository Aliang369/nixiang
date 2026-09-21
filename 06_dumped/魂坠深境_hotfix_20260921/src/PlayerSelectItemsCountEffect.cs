using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelectItemsCountEffect : Effect
{
	[SerializeField]
	private bool canCancel;

	[SerializeField]
	private ItemTargetSelector pool;

	[SerializeField]
	private ValueGenerator countGenerator;

	[SerializeField]
	[LocalizeText("选择卡牌效果描述")]
	[TextArea]
	public string panelHint;

	[SerializeField]
	private Effect postEffect;

	[SerializeField]
	private ItemConstraint selectable;

	[SerializeField]
	private GameEvent.EventBFTType selectorBft;

	[SerializeField]
	private PlayerSelectItemsEffect.SelectPanelType selectPanelType;

	[SerializeField]
	private ValueCompareCheckNode.CompareType type;

	[SerializeField]
	private CheckNode selectableCheckNode;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> lastSelectItems = eve.SelectedItems;
		GameEvent seve = eve.ShallowCopy();
		seve.SelectedItems = new List<Item>();
		SelectItemInfo info = SelectItemInfo.GenerateNodeSelectItemsCountInfo(seve, pool, countGenerator, selectable, type, delegate
		{
			postEffect?.Cast(seve);
			seve.SelectedItems = lastSelectItems;
		}, null, eve.GetEventBFT(selectorBft), () => canCancel, panelHint);
		if (selectableCheckNode != null)
		{
			Func<Item, bool> originalCanSelect = info.canSelect;
			info.canSelect = delegate(Item item)
			{
				if (originalCanSelect == null || originalCanSelect(item))
				{
					Item nodeTempItem = seve.NodeTempItem;
					seve.NodeTempItem = item;
					bool result = selectableCheckNode.Check(seve) == true;
					seve.NodeTempItem = nodeTempItem;
					return result;
				}
				return false;
			};
			Func<bool> originalShouldCancel = info.shouldCancel;
			info.shouldCancel = delegate
			{
				if (originalShouldCancel != null && originalShouldCancel())
				{
					return true;
				}
				List<Item> list = info.pool;
				if (list == null)
				{
					return true;
				}
				int num = 0;
				foreach (Item item in list)
				{
					if (info.canSelect(item))
					{
						num++;
					}
				}
				int num2 = countGenerator?.GenerateValue(seve) ?? (-1);
				return (countGenerator != null && num < num2 && (type == ValueCompareCheckNode.CompareType.Equal || type == ValueCompareCheckNode.CompareType.GreaterEqual)) ? true : false;
			};
		}
		Effect.EnqueueAction(eve, new SelectItemAction(info, selectPanelType));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { pool, countGenerator, postEffect, selectable, selectableCheckNode };
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(panelHint)
		};
	}
}
