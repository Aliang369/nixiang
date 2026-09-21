using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameTest;

public class TestItemDisplay : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public ItemPackSelectArea attachedArea;

	public ItemDefinition attachedItem;

	public Text nameDisplay;

	private void Start()
	{
		if (attachedItem != null)
		{
			GetComponent<Image>().sprite = attachedItem.Icon;
			nameDisplay.text = attachedItem.itemName;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		attachedArea.TransferItem(this);
	}
}
