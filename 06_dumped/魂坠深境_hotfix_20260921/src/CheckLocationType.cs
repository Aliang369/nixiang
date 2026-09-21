using UnityEngine;

public class CheckLocationType : CheckNode
{
	public enum CheckType
	{
		GameEventTargetLocation,
		CurrentLocation
	}

	[SerializeField]
	private CheckType checkType;

	[SerializeField]
	private PathLocation.LocationType type;

	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState == null)
		{
			return null;
		}
		PathLocation location = GetLocation(eve, checkType);
		if (location == null)
		{
			return null;
		}
		return location.type == type;
	}

	public static PathLocation GetLocation(GameEvent eve, CheckType checkType)
	{
		return checkType switch
		{
			CheckType.GameEventTargetLocation => eve.TargetLocation, 
			CheckType.CurrentLocation => eve.gameState?.dungeonMapManager?.currentLocation, 
			_ => null, 
		};
	}
}
