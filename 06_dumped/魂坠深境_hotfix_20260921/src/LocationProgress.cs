public class LocationProgress : ValueGenerator
{
	public CheckLocationType.CheckType locationType;

	public override int? GenerateValue(GameEvent eve)
	{
		return CheckLocationType.GetLocation(eve, locationType)?.localProgress;
	}
}
