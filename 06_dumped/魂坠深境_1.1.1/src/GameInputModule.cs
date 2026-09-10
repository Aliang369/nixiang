using UnityEngine.EventSystems;

public class GameInputModule : StandaloneInputModule
{
	public PointerEventData GetPointerData(int id = 0, PointerEventData.InputButton state = PointerEventData.InputButton.Left)
	{
		return base.GetMousePointerEventData(id).GetButtonState(state).eventData.buttonData;
	}
}
