using System;
using System.Collections.Generic;
using ContainerOfSoul.Display.Fortress;
using ContainerOfSoul.Display.Fortress.DotManagers;

public class HintRedDot : CellUI<RedDotModule>
{
	[NonSerialized]
	public List<HintRedDot> childrenRedDots = new List<HintRedDot>();

	[NonSerialized]
	private FortressMainUIPanel fortressMainPanel;

	public RedDotModule redDotModule => base.setupData;

	protected override bool shouldUpdate => true;

	protected override void Init()
	{
		fortressMainPanel = GetUIInParents<FortressMainUIPanel>();
	}

	protected override void UpdateDisplay()
	{
		base.gameObject.SetActive(ShouldShow());
	}

	private bool ShouldShow()
	{
		FortressMainUIPanel fortressMainUIPanel = fortressMainPanel;
		if ((object)fortressMainUIPanel != null && fortressMainUIPanel.SuppressMainInterfaceRedDots)
		{
			return false;
		}
		if (redDotModule != null && redDotModule.CheckShouldShow())
		{
			return true;
		}
		if (!childrenRedDots.IsNullOrEmpty())
		{
			foreach (HintRedDot childrenRedDot in childrenRedDots)
			{
				if (!(childrenRedDot == null) && childrenRedDot.ShouldShow())
				{
					return true;
				}
			}
		}
		return false;
	}
}
