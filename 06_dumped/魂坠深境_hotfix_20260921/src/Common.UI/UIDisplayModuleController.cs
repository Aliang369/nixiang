using System;
using System.Collections.Generic;

namespace Common.UI;

public class UIDisplayModuleController
{
	[NonSerialized]
	public List<MonoDisplayModule> displayModules;

	public UIDisplayModuleController(UIBase parent)
	{
		displayModules = new List<MonoDisplayModule>(parent.GetComponents<MonoDisplayModule>());
	}

	public T GetModule<T>() where T : MonoDisplayModule
	{
		foreach (MonoDisplayModule displayModule in displayModules)
		{
			if (displayModule is T result)
			{
				return result;
			}
		}
		return null;
	}
}
