public class CheckHasValue : CheckNode
{
	public ValueGenerator valueGenerator;

	public override bool? Check(GameEvent eve)
	{
		return valueGenerator.GenerateValue(eve).HasValue;
	}
}
