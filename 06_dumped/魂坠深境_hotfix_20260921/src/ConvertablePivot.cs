using UnityEngine;

public class ConvertablePivot : PivotManager.IWithPivot
{
	public Canvas canvas;

	public Transform transform;

	public Vector3 WorldPosition => transform?.position ?? Vector3.zero;

	public ConvertablePivot(Transform transform, Canvas canvas)
	{
		this.transform = transform;
		this.canvas = canvas;
	}

	public ConvertablePivot(Transform transform)
	{
		this.transform = transform;
		Canvas[] componentsInParent = transform.GetComponentsInParent<Canvas>(includeInactive: true);
		if (componentsInParent.Length != 0)
		{
			canvas = componentsInParent[0];
		}
	}

	public Vector3 ToCanvasWorldPosition(Canvas canvas)
	{
		if ((Object)(object)canvas == (Object)(object)this.canvas)
		{
			return WorldPosition;
		}
		return WorldPosition.WorldPositionFromCanvasToCanvas(this.canvas, canvas);
	}

	public Transform GetVFXRoot(GameStateInfo gameState)
	{
		if ((Object)(object)canvas == null)
		{
			return gameState.UIManager.sceneVFXRoot;
		}
		if ((Object)(object)canvas == (Object)(object)gameState.UIManager.mainCanvas)
		{
			return gameState.UIManager.uiVFXRoot;
		}
		if ((Object)(object)canvas == (Object)(object)gameState.UIManager.ui3DBaseCanvas)
		{
			return gameState.UIManager.ui3DBaseCanvasFXRoot;
		}
		if ((Object)(object)canvas == (Object)(object)gameState.UIManager.ui3DCameraCanvas)
		{
			return gameState.UIManager.ui3DCameraCanvasVFXRoot;
		}
		if ((Object)(object)canvas == (Object)(object)gameState.UIManager.ui3DWorldCanvas)
		{
			return gameState.UIManager.ui3DWorldCanvasVFXRoot;
		}
		return gameState.UIManager.sceneVFXRoot;
	}

	public ConvertablePivot GetPivot(GameStateInfo gameState)
	{
		return this;
	}
}
