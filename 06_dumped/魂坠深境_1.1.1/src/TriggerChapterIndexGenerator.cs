using System;

public class TriggerChapterIndexGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		if (eve?.nodeTempValues != null && eve.nodeTempValues.TryGetValue("TriggerChapterIndexKey", out var value))
		{
			return Convert.ToInt32(value);
		}
		return eve?.CasterItem?.counter;
	}
}
