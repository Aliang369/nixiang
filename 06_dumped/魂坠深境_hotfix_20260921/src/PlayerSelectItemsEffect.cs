using System.Collections.Generic;
using UnityEngine;

public class PlayerSelectItemsEffect : Effect
{
	public enum SelectPanelType
	{
		HorizontalSelect,
		PileSelect,
		Table
	}

	[SerializeField]
	[LocalizeText("玩家选择卡牌时的提示描述文本")]
	public string panelHint;

	[SerializeField]
	private ItemTargetSelector pool;

	[SerializeField]
	private CheckNode confirmable;

	[SerializeField]
	private CheckNode selectable;

	[SerializeField]
	private CheckNode shouldCancel;

	[SerializeField]
	private GameEvent.EventBFTType selectorBft;

	[SerializeField]
	private SelectPanelType selectPanelType;

	[SerializeField]
	private bool isSingleSelect;

	[SerializeField]
	private bool canCancel;

	[SerializeField]
	private bool limitSameCasterItemSelectionUntilQueueClean;

	[SerializeField]
	private Effect postEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item item = eve.CasterEnchant?.attachItem ?? eve.CasterItem;
		if (limitSameCasterItemSelectionUntilQueueClean && !eve.gameState.actionManager.TryRegisterShadowTraceSelection(item))
		{
			return NodeCastResult.Success;
		}
		List<Item> lastSelectItems = eve.SelectedItems;
		GameEvent seve = eve.ShallowCopy();
		seve.SelectedItems = new List<Item>();
		SelectItemInfo info = SelectItemInfo.GenerateNodeSelectItemsInfo(seve, pool, shouldCancel, selectable, confirmable, delegate
		{
			postEffect?.Cast(seve);
			seve.SelectedItems = lastSelectItems;
		}, null, () => canCancel, eve.GetEventBFT(selectorBft), panelHint, isSingleSelect);
		Effect.EnqueueAction(eve, new SelectItemAction(info, selectPanelType));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { pool, confirmable, selectable, postEffect };
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(panelHint)
		};
	}
}
