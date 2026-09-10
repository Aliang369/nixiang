using UnityEngine;

public static class ConvertablePivotExtend
{
	public static Vector3 GetWorldPosition(this Transform transform)
	{
		return new ConvertablePivot(transform).WorldPosition;
	}

	public static Transform GetVFXRoot(this Transform transform)
	{
		return new ConvertablePivot(transform).GetVFXRoot(Bootstrap.applicationState.runningGameState);
	}
}
