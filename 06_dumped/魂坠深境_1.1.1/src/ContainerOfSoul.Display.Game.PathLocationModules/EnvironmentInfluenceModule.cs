using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.PathLocationModules;

public class EnvironmentInfluenceModule : PathLocationDisplay.DisplayModule
{
	public GameObject environmentInfluenceArea;

	public Image environmentImage;

	public override void UpdateDisplay()
	{
		if (base.pathLocationDisplay.location.hideLoc || base.pathLocationDisplay.location.locationEnvironmentInfluence == null)
		{
			environmentInfluenceArea.SetActive(value: false);
			return;
		}
		environmentInfluenceArea.SetActive(value: true);
		environmentImage.sprite = base.pathLocationDisplay.location.locationEnvironmentInfluence.icon;
		environmentImage.color = Color.white;
	}
}
