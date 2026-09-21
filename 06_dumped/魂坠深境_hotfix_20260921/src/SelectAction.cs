using System;

public class SelectAction<T> : GameAction where T : class
{
	private readonly ElementSelectPanel<T>.SelectInfo info;

	private bool started;

	public SelectAction(ElementSelectPanel<T>.SelectInfo info)
	{
		this.info = info;
		started = false;
	}

	protected override void Update()
	{
		if (started)
		{
			return;
		}
		if (info.shouldCancel != null && info.shouldCancel())
		{
			FinishAction();
			return;
		}
		if (!info.selectorBft.isPlayer)
		{
			info.output.Add(info.pool[0]);
			info.onConfirm?.Invoke();
			FinishAction();
			return;
		}
		started = true;
		ElementSelectPanel<T>.SelectInfo selectInfo = info;
		selectInfo.onConfirm = (Action)Delegate.Combine(selectInfo.onConfirm, new Action(base.FinishAction));
		ElementSelectPanel<T>.SelectInfo selectInfo2 = info;
		selectInfo2.onCancel = (Action)Delegate.Combine(selectInfo2.onCancel, new Action(base.FinishAction));
		if (!GameAutomationHooks.TryHandleElementSelect(gameState, info))
		{
			if (typeof(T) == typeof(Influence))
			{
				gameState.UIManager.influenceSelectPanel.Setup(info as ElementSelectPanel<Influence>.SelectInfo);
			}
			else if (typeof(T) == typeof(Enchant))
			{
				gameState.UIManager.enchantSelectPanel.Setup(info as ElementSelectPanel<Enchant>.SelectInfo);
			}
			else if (typeof(T) == typeof(Rune))
			{
				gameState.UIManager.runeSelectPanel.Setup(info as ElementSelectPanel<Rune>.SelectInfo);
			}
			else if (typeof(T) == typeof(GameSkill))
			{
				gameState.UIManager.skillSelectPanel.Setup(info as ElementSelectPanel<GameSkill>.SelectInfo);
			}
			else
			{
				FinishAction();
			}
		}
	}
}
