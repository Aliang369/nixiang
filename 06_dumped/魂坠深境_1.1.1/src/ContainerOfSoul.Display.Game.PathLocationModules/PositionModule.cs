using UnityEngine;

namespace ContainerOfSoul.Display.Game.PathLocationModules;

public class PositionModule : PathLocationDisplay.DisplayModule
{
	public override void Setup()
	{
		((RectTransform)base.transform).anchoredPosition = base.pathLocationDisplay.location.displayPosition;
	}
}
