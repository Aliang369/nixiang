using TMPro;
using UnityEngine;

public class PickItemSelectorModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI text;

	public override void Setup()
	{
		text.gameObject.SetActive(value: true);
		text.text = $"+{base.slotDisplay.item.counter}";
	}
}
