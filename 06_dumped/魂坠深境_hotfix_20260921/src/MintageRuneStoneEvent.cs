public class MintageRuneStoneEvent : WorldEvent
{
	public class MintageRuneRoomConfig
	{
		public ClavisDef clavis1;

		public ClavisDef clavis2;

		public WorldEventChoiceText mintageChoiceText;

		public WorldEventChoiceText regenerateChoiceText;

		public WorldEventChoiceText enhanceRuneChoiceText;

		public int forceMintageRunePricePerRune = 10;

		public int switchRunePrice = 4;

		public int regenerateRunePrice = 10;
	}

	public override WorldEventDisplayType displayType => WorldEventDisplayType.MintageRuneRoom;

	public bool IsEnhanceRune
	{
		get
		{
			PathLocation currentLocation = gameState.dungeonMapManager.currentLocation;
			if (currentLocation == null)
			{
				return false;
			}
			return currentLocation.variantType == PathLocation.LocationVariantType.EnhanceRune;
		}
	}

	public override WorldEventPanel GetDisplayWorldEventPanel(GameStateInfo gameState)
	{
		return gameState.UIManager.worldEventPanel_MintageRune;
	}

	public void HideMintageEffectObject()
	{
		RoomObjectDisplay roomObjectDisplay = gameState.currentEncounter?.majorRoomObjectDisplay;
		if (roomObjectDisplay != null && roomObjectDisplay.hideObjectAfterEffect != null)
		{
			roomObjectDisplay.hideObjectAfterEffect.gameObject.SetActive(value: false);
		}
	}

	public void ShowMintageEffectObject()
	{
		RoomObjectDisplay roomObjectDisplay = gameState.currentEncounter?.majorRoomObjectDisplay;
		if (roomObjectDisplay != null && roomObjectDisplay.hideObjectAfterEffect != null)
		{
			roomObjectDisplay.hideObjectAfterEffect.gameObject.SetActive(value: true);
		}
	}
}
