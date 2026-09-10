using System;
using System.Collections.Generic;

public static class StringExtends
{
	public static int[] AllIndexesOf(this string str, string substr, bool ignoreCase = false)
	{
		if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(substr))
		{
			throw new ArgumentException("String or substring is not specified.");
		}
		List<int> list = new List<int>();
		int startIndex = 0;
		while ((startIndex = str.IndexOf(substr, startIndex, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) != -1)
		{
			list.Add(startIndex++);
		}
		return list.ToArray();
	}
}
