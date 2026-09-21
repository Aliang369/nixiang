using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

public class ConsumableModule_Game : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private Image consumableImage;

	public override void UpdateDisplay()
	{
		consumableImage.gameObject.SetActive(GetConsumable_Game(base.slotDisplay.item));
	}

	public bool GetConsumable_Game(Item item)
	{
		return !UIDisplayModule.gameState.gameEventManager.TriggerResolveConsumeInfo(item, null, ConsumeItemAction.ConsumeType.Overflow, null, GameEvent.TriggerType.InBattleDisplay).reject;
	}
}
