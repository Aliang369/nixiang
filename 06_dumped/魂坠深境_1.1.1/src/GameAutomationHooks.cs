public static class GameAutomationHooks
{
	private static IGameAutomationHandler handler;

	public static void SetHandler(IGameAutomationHandler newHandler)
	{
		handler = newHandler;
	}

	public static void ClearHandler(IGameAutomationHandler registeredHandler)
	{
		if (handler == registeredHandler)
		{
			handler = null;
		}
	}

	public static bool TryHandleBonus(GameStateInfo gameState, BattleBonus bonus, bool endEncounterAfterBonus, bool isBattle)
	{
		return handler?.TryHandleBonus(gameState, bonus, endEncounterAfterBonus, isBattle) ?? false;
	}

	public static bool TryHandleElementSelect<T>(GameStateInfo gameState, ElementSelectPanel<T>.SelectInfo info) where T : class
	{
		return handler?.TryHandleElementSelect(gameState, info) ?? false;
	}

	public static bool TryHandleSelectItem(GameStateInfo gameState, SelectItemInfo info)
	{
		return handler?.TryHandleSelectItem(gameState, info) ?? false;
	}
}
