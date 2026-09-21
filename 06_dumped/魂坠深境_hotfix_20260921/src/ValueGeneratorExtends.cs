public static class ValueGeneratorExtends
{
	public static int? GetValue(this ValueGenerator valueGenerator, GameEvent eve, int? defaultValue = null)
	{
		if (valueGenerator != null)
		{
			return valueGenerator.GenerateValue(eve);
		}
		return defaultValue;
	}
}
