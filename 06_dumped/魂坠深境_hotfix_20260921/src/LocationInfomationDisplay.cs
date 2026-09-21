using UnityEngine;
using UnityEngine.UI;

public class LocationInfomationDisplay : MonoBehaviour
{
	public PathLocationDisplay focusLocation;

	public Button jumpToButton;

	public Text LocationDescriptionText;

	public Text LocationNameText;

	private void Update()
	{
		if (!(focusLocation == null))
		{
			base.transform.position = focusLocation.transform.position;
			if (focusLocation.location.jumpAble || CSCommon.cheatJumpMap)
			{
				jumpToButton.interactable = true;
			}
			else
			{
				jumpToButton.interactable = false;
			}
			LocationNameText.text = focusLocation.location.GetLocationDisplayName();
			LocationDescriptionText.text = GetLocationDescription(focusLocation.location);
		}
	}

	public void ApplyLocation(PathLocationDisplay location)
	{
		focusLocation = location;
		base.gameObject.SetActive(value: true);
		base.transform.position = focusLocation.transform.position;
	}

	public string GetLocationDescription(PathLocation loc)
	{
		return "";
	}

	public void JumpToFocusLocation()
	{
		GameStateInfo gameState = SceneInfoSingleton.instance.gameState;
		if (gameState.actionManager.isEmpty)
		{
			gameState.actionManager.EnqueueAction(new EnterLocationAction(focusLocation.location));
		}
	}
}
