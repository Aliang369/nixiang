using System;
using UnityEngine;

public abstract class BattleBonusObject
{
	public bool view;

	public bool triggered;

	public Action onGotBonus;

	private Action onBonusCancelled;

	private Action onBonusUnavailable;

	public virtual bool isChoose => false;

	public abstract Sprite PreviewIcon { get; }

	public abstract string previewText { get; }

	public virtual string countText => "";

	public virtual string description => "";

	public virtual object infoObject => this;

	public void TriggerBonus(GameStateInfo gameState, Action onGotBonus, Action onBonusCancelled = null, Action onBonusUnavailable = null)
	{
		triggered = true;
		this.onGotBonus = onGotBonus;
		this.onBonusCancelled = onBonusCancelled;
		this.onBonusUnavailable = onBonusUnavailable;
		TriggerImpl(gameState);
	}

	protected void ConfirmGotBonus()
	{
		onGotBonus?.Invoke();
	}

	protected void ConfirmBonusCancelled()
	{
		onBonusCancelled?.Invoke();
	}

	protected void ConfirmBonusUnavailable()
	{
		onBonusUnavailable?.Invoke();
	}

	protected abstract void TriggerImpl(GameStateInfo gameState);
}
