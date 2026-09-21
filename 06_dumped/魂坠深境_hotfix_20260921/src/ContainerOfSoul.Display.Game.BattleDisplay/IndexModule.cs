using System;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class IndexModule : ItemDisplayBase.DisplayModule
{
	[NonSerialized]
	public IndexDisplay indexDisplay;

	public override void Setup()
	{
		indexDisplay = UIDisplayModule.gameState.UIManager.playerItemArea.playerOverloadDisplay.AddIndexDisplay(base.slotDisplay);
	}

	public override void Unset()
	{
		UIDisplayModule.gameState.UIManager.playerItemArea.playerOverloadDisplay.RemoveIndexDisplays(indexDisplay);
	}
}
