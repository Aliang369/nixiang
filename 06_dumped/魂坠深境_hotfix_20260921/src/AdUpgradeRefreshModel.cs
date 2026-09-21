using System;

public sealed class AdUpgradeRefreshModel : UpgradeRefreshModel
{
	private readonly Func<bool> enabledGetter;

	private readonly Func<int> maxCountGetter;

	private readonly Action regenerateAction;

	public override int MaxCount => maxCountGetter?.Invoke() ?? 0;

	public override bool Enabled => enabledGetter?.Invoke() ?? false;

	public AdUpgradeRefreshModel(Func<bool> enabledGetter, Func<int> maxCountGetter, Action regenerateAction)
	{
		this.enabledGetter = enabledGetter;
		this.maxCountGetter = maxCountGetter;
		this.regenerateAction = regenerateAction;
	}

	protected override void Regenerate()
	{
		regenerateAction?.Invoke();
	}
}
