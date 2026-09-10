using System;

public class NormalEventSelectItemAction : GameAction
{
	private readonly SelectItemInfo info;

	private bool started;

	public NormalEventSelectItemAction(SelectItemInfo info)
	{
		this.info = info;
		this.info.showUncollectedStatus = true;
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
			info.TriggerSelect(gameState, PlayerSelectItemsEffect.SelectPanelType.PileSelect, prepareSelect: false);
		}
	}
}
