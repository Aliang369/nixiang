using TMPro;
using UnityEngine;

public class ItemOverloadModule : ItemDisplayBase.DisplayModule
{
	[SerializeField]
	private RectTransform overLoadDisplay;

	[SerializeField]
	private TextMeshProUGUI overLoadDisplayText;

	[SerializeField]
	private Color toConsumColor;

	[SerializeField]
	private Color toKeepColor;

	public override void Init()
	{
		overLoadDisplayText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_keep_overflow_item.ColorText(toKeepColor);
	}

	public override void UpdateDisplay()
	{
		if (UIDisplayModule.gameState.battleState.turnPhase != TurnPhase.PlayerControlPhase && (!UIDisplayModule.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.OverflowConsumeOnTurnStart) || UIDisplayModule.gameState.battleState.turnPhase == TurnPhase.PlayerTurnStartPhase))
		{
			overLoadDisplay.gameObject.SetActive(value: false);
		}
		else
		{
			overLoadDisplay.gameObject.SetActive(ShouldOverload_Game(base.slotDisplay.item, UIDisplayModule.gameState));
		}
	}

	public bool ShouldOverload_Game(Item item, GameStateInfo gameState, Item sourceItem = null)
	{
		if (item.owner == null || !item.owner.ShouldOverflow(item))
		{
			return false;
		}
		return gameState.gameEventManager.TriggerResolveConsumeInfo(item, sourceItem, ConsumeItemAction.ConsumeType.Overflow, null, GameEvent.TriggerType.InBattleDisplay).reject;
	}
}
