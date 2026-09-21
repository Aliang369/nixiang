using System;
using System.Collections.Generic;

public sealed class CurrencyUpgradeRefreshModel : UpgradeRefreshModel
{
	private readonly GameStateInfo gameState;

	private readonly Func<bool> enabledGetter;

	private readonly Func<List<int>> pricesGetter;

	private readonly Func<MatterDef> matterGetter;

	private readonly Action regenerateAction;

	private readonly string consumeReason;

	public override int MaxCount => (pricesGetter?.Invoke()?.Count).GetValueOrDefault();

	public override bool Enabled => enabledGetter?.Invoke() ?? false;

	public override MatterDef Matter => matterGetter?.Invoke();

	public override int CurrentPrice
	{
		get
		{
			List<int> list = pricesGetter?.Invoke();
			if (list == null || base.UsedCount >= list.Count)
			{
				return 0;
			}
			return list[base.UsedCount];
		}
	}

	public override bool CanRefresh
	{
		get
		{
			if (base.Available && Matter != null && CurrentPrice > 0)
			{
				return gameState.applicationState.matterManager.HasEnoughMatter(Matter, CurrentPrice);
			}
			return false;
		}
	}

	public CurrencyUpgradeRefreshModel(GameStateInfo gameState, Func<bool> enabledGetter, Func<List<int>> pricesGetter, Func<MatterDef> matterGetter, Action regenerateAction, string consumeReason)
	{
		this.gameState = gameState;
		this.enabledGetter = enabledGetter;
		this.pricesGetter = pricesGetter;
		this.matterGetter = matterGetter;
		this.regenerateAction = regenerateAction;
		this.consumeReason = consumeReason;
	}

	protected override void Pay()
	{
		gameState.applicationState.matterManager.ConsumeMatter(Matter, CurrentPrice, consumeReason);
	}

	protected override void Regenerate()
	{
		regenerateAction?.Invoke();
	}
}
