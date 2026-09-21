using TMPro;
using UnityEngine;

public class RecommendItemPanelModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private TextMeshProUGUI hint;

	public override void UpdateDisplay()
	{
		hint.gameObject.SetActive(value: false);
		if (UIDisplayModule.gameState != null && UIDisplayModule.gameState.playerState.bft.deckContainer.items.Find((Item i) => i.def == base.item.def) != null)
		{
			hint.gameObject.SetActive(value: true);
		}
	}
}
