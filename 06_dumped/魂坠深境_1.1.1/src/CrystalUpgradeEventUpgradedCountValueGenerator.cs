public class CrystalUpgradeEventUpgradedCountValueGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		CrystalUpgradeWorldEvent crystalUpgradeWorldEvent = (eve.gameState.UIManager.currentEventPanel as CrystalUpgradeEventPanel)?.worldEvent as CrystalUpgradeWorldEvent;
		if (crystalUpgradeWorldEvent == null)
		{
			return null;
		}
		return crystalUpgradeWorldEvent.upgraded.Count;
	}
}
