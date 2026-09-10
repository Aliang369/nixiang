using System;

public static class EnumExtends
{
	public static bool IsAny(this Enum e, params Enum[] enums)
	{
		if (enums == null || enums.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < enums.Length; i++)
		{
			if (object.Equals(e, enums[i]))
			{
				return true;
			}
		}
		return false;
	}
}
