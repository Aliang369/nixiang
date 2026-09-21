using System;

public class PivotManager : InGameManager
{
	public enum PivotType
	{
		None,
		PlayerTablePivot,
		PlayerDrawPilePivot,
		PlayerWastePilePivot,
		EnemyPivot,
		CompanionPivot,
		EventSelectPilePivot,
		DrawPileButton,
		WastePileButton,
		DeckPileButton
	}

	public interface IWithPivot
	{
		ConvertablePivot GetPivot(GameStateInfo gameState);
	}

	public ConvertablePivot crystalDisplayPivot => base.uiManager.battlePanel.crystalDisplayPivot;

	public ConvertablePivot coinDisplayPivot => base.uiManager.battlePanel.coinDisplayPivot;

	public ConvertablePivot gemDisplayPivot => base.uiManager.battlePanel.gemDisplayPivot;

	public ConvertablePivot GetPivot(PivotType type)
	{
		return type switch
		{
			PivotType.PlayerTablePivot => base.gameState.UIManager.battleItemInfoPanel.playerTablePivot, 
			PivotType.PlayerDrawPilePivot => base.gameState.UIManager.battleItemInfoPanel.playerDrawPilePivot, 
			PivotType.PlayerWastePilePivot => base.gameState.UIManager.battleItemInfoPanel.playerWastePilePivot, 
			PivotType.EnemyPivot => base.gameState.UIManager.battleItemInfoPanel.enemyPivot, 
			PivotType.CompanionPivot => base.gameState.UIManager.battleItemInfoPanel.companionPivot, 
			PivotType.EventSelectPilePivot => base.gameState.UIManager.battleItemInfoPanel.eventSelectPilePivot, 
			PivotType.DrawPileButton => base.gameState.UIManager.battlePanel.drawPileButtonPivot, 
			PivotType.WastePileButton => base.gameState.UIManager.battlePanel.wastePileButtonPivot, 
			PivotType.DeckPileButton => base.gameState.UIManager.battlePanel.deckButtonPivot, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
