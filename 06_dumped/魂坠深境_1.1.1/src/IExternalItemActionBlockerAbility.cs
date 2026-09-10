public interface IExternalItemActionBlockerAbility
{
	bool BlocksExternalItemAction(Item item, Enchant enchant, object source, ExternalItemActionBlockType actionType);

	void OnExternalItemActionBlocked(GameStateInfo gameState, Item item, Enchant enchant, object source, ExternalItemActionBlockType actionType);
}
