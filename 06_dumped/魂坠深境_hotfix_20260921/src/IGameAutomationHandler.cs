public interface IGameAutomationHandler
{
	bool TryHandleBonus(GameStateInfo gameState, BattleBonus bonus, bool endEncounterAfterBonus, bool isBattle);

	bool TryHandleElementSelect<T>(GameStateInfo gameState, ElementSelectPanel<T>.SelectInfo info) where T : class;

	bool TryHandleSelectItem(GameStateInfo gameState, SelectItemInfo info);
}
