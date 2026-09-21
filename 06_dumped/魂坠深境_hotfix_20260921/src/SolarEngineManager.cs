public static class SolarEngineManager
{
	public static void PreInit()
	{
		SolarEngineAdapter.PreInit();
		SolarEngineAdapter.SetSuperProperty("te_distinct_id", ThinkingAnalyticsExtends.GetDistinctId());
		SolarEngineAdapter.SetSuperProperty("distro_id", GlobalConfigManager.instance.CurrentDistroConfigInfo.DistroId);
	}

	public static void OnLoginSuccess(long playerId, bool isNewUser, string sdkType)
	{
		SolarEngineAdapter.SetSuperProperty("te_account_id", playerId);
		SolarEngineAdapter.Init(GlobalConfigManager.instance.debugLogTA);
		if (isNewUser)
		{
			SolarEngineAdapter.TrackRegister(sdkType, "success");
		}
		SolarEngineAdapter.TrackLogin(sdkType, "success");
	}

	public static void TrackPurchase(string productName, string productId, int priceCents, bool success, string orderId, string payType = "")
	{
		SolarEngineAdapter.TrackPurchase(productName, productId, priceCents, success, orderId, "CNY", payType);
	}
}
