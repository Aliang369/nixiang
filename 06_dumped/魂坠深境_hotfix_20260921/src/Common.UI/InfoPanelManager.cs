using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common.UI;

public class InfoPanelManager
{
	private readonly List<IInfoPanelController> controllers = new List<IInfoPanelController>();

	public void HideAllPanel(Transform hideSource)
	{
		foreach (IInfoPanelController item in controllers.ToList())
		{
			if (item.HasShowingPanel() && (!(hideSource != null) || !hideSource.IsChildOf(item.GetUINode().rectTransform)))
			{
				item.OnHideAllInfo();
			}
		}
	}

	public void AddController(IInfoPanelController controller)
	{
		controllers.Add(controller);
	}

	public void RemoveController(IInfoPanelController controller)
	{
		controllers.Remove(controller);
	}
}
