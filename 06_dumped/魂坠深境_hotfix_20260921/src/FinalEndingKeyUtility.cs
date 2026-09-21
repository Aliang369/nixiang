using System.Linq;

public static class FinalEndingKeyUtility
{
	public static ItemDefinition KeyDef => ScriptableObjectSingleton<GameDataManager>.instance?.finalEndingKeyItem;

	public static bool IsFinalEndingKey(Item item)
	{
		if (item?.GetDef() != null)
		{
			return item.GetDef() == KeyDef;
		}
		return false;
	}

	public static Item FindPlayerFinalEndingKey(GameStateInfo gameState)
	{
		return gameState?.playerState?.DeckContainer?.GetAllItems().FirstOrDefault(IsFinalEndingKey);
	}

	public static bool HasPlayerFinalEndingKey(GameStateInfo gameState)
	{
		return FindPlayerFinalEndingKey(gameState) != null;
	}

	public static Item FindPlayerRepairedFinalEndingKey(GameStateInfo gameState)
	{
		return gameState?.playerState?.DeckContainer?.GetAllItems().FirstOrDefault((Item item) => IsFinalEndingKey(item) && item.upgraded);
	}

	public static bool HasPlayerRepairedFinalEndingKey(GameStateInfo gameState)
	{
		return FindPlayerRepairedFinalEndingKey(gameState) != null;
	}
}
