using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.PathLocationModules;

public class IconModule : PathLocationDisplay.DisplayModule
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image customPivotHint;

	public override void Setup()
	{
		icon.sprite = base.pathLocationDisplay.location.GetLocationIcon();
		customPivotHint.gameObject.SetActive(base.pathLocationDisplay.location.variantType != PathLocation.LocationVariantType.None);
	}
}
