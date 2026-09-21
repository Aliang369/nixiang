using UnityEngine;
using UnityEngine.EventSystems;

public class GemMountingBoardInput : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private GemMountingPanel panel;

	public void Setup(GemMountingPanel gemMountingPanel)
	{
		panel = gemMountingPanel;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (GameButton.enableAll)
		{
			if (panel == null)
			{
				panel = GetComponentInParent<GemMountingPanel>(includeInactive: true);
			}
			panel?.OnMountBoardClicked(eventData.position);
		}
	}
}
