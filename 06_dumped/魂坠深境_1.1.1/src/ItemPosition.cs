using UnityEngine;

public static class ItemPosition
{
	public static Vector3? GetCanvasWorldPosition(this Item item, GameStateInfo gameState, Canvas canvas)
	{
		if (gameState == null)
		{
			return null;
		}
		return item.GetPivot(gameState)?.ToCanvasWorldPosition(canvas);
	}

	public static Vector3? GetCanvasWorldPosition(this Item item, GameStateInfo gameState, CanvasType canvasType)
	{
		if (gameState?.UIManager == null)
		{
			return null;
		}
		return item.GetPivot(gameState)?.ToCanvasWorldPosition(gameState.UIManager.GetCanvas(canvasType));
	}

	public static Vector3? GetCanvasWorldPosition(GameStateInfo gameState, PivotManager.PivotType type, CanvasType canvasType)
	{
		if (gameState?.UIManager == null)
		{
			return null;
		}
		return gameState.pivotManager.GetPivot(type)?.ToCanvasWorldPosition(gameState.UIManager.GetCanvas(canvasType));
	}

	public static Vector3? GetMainCanvasWorldPosition(this ConvertablePivot pivot, GameStateInfo gameState)
	{
		if ((Object)(object)gameState?.UIManager?.mainCanvas == null)
		{
			return null;
		}
		return pivot?.ToCanvasWorldPosition(gameState.UIManager.mainCanvas);
	}

	public static Vector3? GetMainCanvasWorldPosition(this Item item, GameStateInfo gameState)
	{
		if ((Object)(object)gameState?.UIManager?.mainCanvas == null)
		{
			return null;
		}
		return item?.GetPivot(gameState)?.ToCanvasWorldPosition(gameState.UIManager.mainCanvas);
	}

	public static Vector3? Get3DCameraCanvasWorldPosition(this ConvertablePivot pivot, GameStateInfo gameState)
	{
		if ((Object)(object)gameState?.UIManager?.ui3DCameraCanvas == null)
		{
			return null;
		}
		return pivot?.ToCanvasWorldPosition(gameState.UIManager.ui3DCameraCanvas);
	}

	public static Vector3? Get3DCameraCanvasWorldPosition(this Item item, GameStateInfo gameState)
	{
		if ((Object)(object)gameState?.UIManager?.ui3DCameraCanvas == null)
		{
			return null;
		}
		return item?.GetPivot(gameState)?.ToCanvasWorldPosition(gameState.UIManager.ui3DCameraCanvas);
	}

	public static Vector3? Get3DWorldCanvasWorldPosition(this ConvertablePivot pivot, GameStateInfo gameState)
	{
		if ((Object)(object)gameState?.UIManager?.ui3DWorldCanvas == null)
		{
			return null;
		}
		return pivot?.ToCanvasWorldPosition(gameState.UIManager.ui3DWorldCanvas);
	}

	public static Vector3? Get3DWorldCanvasWorldPosition(this Item item, GameStateInfo gameState)
	{
		if ((Object)(object)gameState?.UIManager?.ui3DWorldCanvas == null)
		{
			return null;
		}
		return item?.GetPivot(gameState)?.ToCanvasWorldPosition(gameState.UIManager.ui3DWorldCanvas);
	}
}
