using System;

namespace ContainerOfSoul.Display.Fortress.DotManagers;

public class CustomModule : RedDotModule
{
	public Func<bool?> shouldShow;

	protected override bool? CheckImpl()
	{
		if (shouldShow == null)
		{
			return false;
		}
		return shouldShow?.Invoke();
	}
}
