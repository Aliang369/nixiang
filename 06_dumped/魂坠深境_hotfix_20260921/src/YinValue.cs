public class YinValue : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.type != GameEvent.Type.ChangeYinYangPower)
		{
			return null;
		}
		return eve.amount;
	}
}
