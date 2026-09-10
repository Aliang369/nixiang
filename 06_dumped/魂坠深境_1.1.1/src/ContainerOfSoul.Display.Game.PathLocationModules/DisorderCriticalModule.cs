using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.PathLocationModules;

public class DisorderCriticalModule : PathLocationDisplay.DisplayModule
{
	public GameObject disorderCriticalArea;

	public Image disorderCriticalImage;

	public override void UpdateDisplay()
	{
		InfluenceDef disorderCriticalInfluence = base.pathLocationDisplay.location.disorderCriticalInfluence;
		bool flag = !base.pathLocationDisplay.location.hideLoc && disorderCriticalInfluence != null;
		disorderCriticalArea.SetActive(flag);
		if (flag)
		{
			disorderCriticalImage.sprite = disorderCriticalInfluence.MapIcon;
			disorderCriticalImage.color = Color.white;
			disorderCriticalImage.raycastTarget = true;
			WorldEventInfluenceInfoTrigger worldEventInfluenceInfoTrigger = disorderCriticalArea.GetComponent<WorldEventInfluenceInfoTrigger>();
			if (worldEventInfluenceInfoTrigger == null)
			{
				worldEventInfluenceInfoTrigger = disorderCriticalArea.AddComponent<WorldEventInfluenceInfoTrigger>();
			}
			worldEventInfluenceInfoTrigger.Setup(new Influence(disorderCriticalInfluence), UIHelper.GetDefaultCommonInfoPanel(), disorderCriticalImage.rectTransform);
		}
	}
}
