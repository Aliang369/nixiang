public interface IUpgradeChoicePresentation
{
	bool UsesWillPower { get; }

	bool ShowWillPowerProgress { get; }

	bool ShowWillPowerCost { get; }

	bool ShowBackButton { get; }

	bool AdRefreshRequiresRootBlessUnlock { get; }
}
