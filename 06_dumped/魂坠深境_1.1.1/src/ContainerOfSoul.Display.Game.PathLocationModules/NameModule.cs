using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Game.PathLocationModules;

public class NameModule : PathLocationDisplay.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI mainText;

	public override void Setup()
	{
		mainText.text = GetLocationText();
	}

	public override void UpdateDisplay()
	{
		mainText.text = GetLocationText();
	}

	public string GetLocationText()
	{
		if (base.pathLocationDisplay.location.overrideName != null)
		{
			return base.pathLocationDisplay.location.overrideName;
		}
		return base.pathLocationDisplay.location.GetLocationDisplayName();
	}
}
