using System.Collections.Generic;

public class NightVisionPreventAmbushAbility : Ability
{
	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.BeforeEnterLocation;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			PathLocation targetLocation = eve.TargetLocation;
			if (targetLocation != null && targetLocation.variantType == PathLocation.LocationVariantType.AmbushFight)
			{
				targetLocation.variantType = PathLocation.LocationVariantType.None;
			}
		}
	}
}
