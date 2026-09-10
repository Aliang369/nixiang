using System;
using System.Linq;

public class SelectChoiceAction : GameAction
{
	private readonly SelectChoiceInfo info;

	private bool started;

	public SelectChoiceAction(SelectChoiceInfo info)
	{
		this.info = info;
	}

	protected override void Update()
	{
		if (!started)
		{
			if (info.choices.Count == 0 || info.selectorBft == null)
			{
				FinishAction();
				return;
			}
			if (!info.selectorBft.isPlayer)
			{
				info.choices.First()?.onSelect?.Invoke();
				FinishAction();
				return;
			}
			SelectChoiceInfo selectChoiceInfo = info;
			selectChoiceInfo.onConfirm = (Action)Delegate.Combine(selectChoiceInfo.onConfirm, new Action(base.FinishAction));
			started = true;
			gameState.UIManager.choiceSelectPanel.Setup(info);
		}
	}
}
