public static class UncollectedItemDisplayHelper
{
	public const string UncollectedItemName = "? ? ?";

	public static bool ShouldHideItemName(ApplicationStateInfo applicationState, Item item, bool showUncollectedStatus)
	{
		return ShouldHideItemName(applicationState, item?.GetDef(), showUncollectedStatus);
	}

	public static bool ShouldHideItemName(ApplicationStateInfo applicationState, ItemDefinition itemDef, bool showUncollectedStatus)
	{
		if (showUncollectedStatus && itemDef != null && applicationState?.discoverManager != null && applicationState.assetCatalog != null && DiscoverManager.IsContentInConfiguredIndexPage(itemDef, applicationState.assetCatalog))
		{
			return !applicationState.discoverManager.IsDiscovered(itemDef);
		}
		return false;
	}

	public static string GetDisplayItemName(ApplicationStateInfo applicationState, Item item, bool showUncollectedStatus)
	{
		if (!ShouldHideItemName(applicationState, item, showUncollectedStatus))
		{
			return item?.GetItemName();
		}
		return "? ? ?";
	}

	public static string GetDisplayItemName(ApplicationStateInfo applicationState, ItemDefinition itemDef, bool showUncollectedStatus)
	{
		if (!ShouldHideItemName(applicationState, itemDef, showUncollectedStatus))
		{
			return itemDef?.itemName;
		}
		return "? ? ?";
	}
}
