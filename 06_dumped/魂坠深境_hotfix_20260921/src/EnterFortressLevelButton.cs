using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

public class EnterFortressLevelButton : SerializedMonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public void OnPointerClick(PointerEventData eventData)
	{
		Bootstrap.instance.Load(BootStrapLoadData.LoadToFortress(Bootstrap.instance.CurrentAccountID));
	}
}
