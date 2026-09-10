using System;

public class SelectItemAction : GameAction
{
	private readonly SelectItemInfo info;

	private readonly PlayerSelectItemsEffect.SelectPanelType selectPanelType;

	private bool started;

	public SelectItemAction(SelectItemInfo info, PlayerSelectItemsEffect.SelectPanelType selectPanelType)
	{
		this.info = info;
		this.selectPanelType = selectPanelType;
		started = false;
	}

	protected override void Update()
	{
		if (!started)
		{
			info.ResolvePool();
			if (info.shouldCancel != null && info.shouldCancel())
			{
				FinishAction();
				return;
			}
			started = true;
			SelectItemInfo selectItemInfo = info;
			selectItemInfo.onConfirm = (Action)Delegate.Combine(selectItemInfo.onConfirm, new Action(base.FinishAction));
			SelectItemInfo selectItemInfo2 = info;
			selectItemInfo2.onCancel = (Action)Delegate.Combine(selectItemInfo2.onCancel, new Action(base.FinishAction));
			info.TriggerSelect(gameState, selectPanelType, prepareSelect: false);
		}
	}
}
