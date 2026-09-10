using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class ItemPriceDisplay : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private GameObject pricePanel;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private Image darkMask;

	[SerializeField]
	private GameButton priceButton;

	public override void UpdateDisplay()
	{
		StoreEvent.PricedItem pricedItem = (UIDisplayModule.gameState.UIManager.currentEventPanel as GoldStoreEventPanel).TryGetPricedItem(base.slotDisplay.item);
		if (pricedItem == null)
		{
			pricePanel.SetActive(value: false);
			return;
		}
		pricePanel.SetActive(value: true);
		nameText.text = $"{ScriptableObjectSingleton<GameDataManager>.instance.coinTextIcon.ToTMPSpriteText()}{pricedItem.price}";
		bool flag = UIDisplayModule.gameState.playerState.HasEnoughCoin(pricedItem.price);
		darkMask.gameObject.SetActive(!flag);
		priceButton.interactable = flag;
	}
}
