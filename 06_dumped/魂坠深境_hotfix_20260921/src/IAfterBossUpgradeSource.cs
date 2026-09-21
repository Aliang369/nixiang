using System.Collections.Generic;

public interface IAfterBossUpgradeSource
{
	List<AfterBossUpgrade> AllAfterBossUpgrade { get; }

	UpgradeRefreshModel CurrencyRefresh { get; }

	UpgradeRefreshModel AdRefresh { get; }

	void OnUpgradeTriggered(AfterBossUpgrade upgrade);
}
