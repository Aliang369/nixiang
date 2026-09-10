using System;

namespace ContainerOfSoul.Display.Game.BattleDisplay;

[Serializable]
public class EnemyItemInputModule : ItemDisplayBase.DisplayModule
{
	public override void Init()
	{
		InputModule itemSlotInputModule = base.slotDisplay.itemSlotInputModule;
		itemSlotInputModule.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(itemSlotInputModule.onHandleInput, new Action<InputModuleBase>(_OnHandleInput));
	}

	private void _OnHandleInput(InputModuleBase inputModule)
	{
		if (UIDisplayModule.gameState.battleState.turnPhase == TurnPhase.PlayerControlPhase)
		{
			if (inputModule.pointerClicked)
			{
				HandlePointerClicked();
			}
			if (inputModule.pointerEnter)
			{
				HandlePointerEnter();
			}
			if (inputModule.pointerExit)
			{
				HandlePointerExit();
			}
		}
	}

	private void HandlePointerClicked()
	{
		if (!base.slotDisplay.item.IsHidden(UIDisplayModule.gameState))
		{
			UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo.CancelSpell();
		}
	}

	private void HandlePointerEnter()
	{
		PlayerControlInfo controlInfo = UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo;
		if (controlInfo.state == PlayerControlInfo.State.GettingTarget && controlInfo.targetItem != base.slotDisplay.item && controlInfo.CheckItemAsTargetAble(base.slotDisplay.item))
		{
			controlInfo.SelectSpellTarget(base.slotDisplay.item);
		}
	}

	private void HandlePointerExit()
	{
		PlayerControlInfo controlInfo = UIDisplayModule.gameState.UIManager.playerItemArea.controlInfo;
		if (controlInfo.state == PlayerControlInfo.State.GettingTarget && UIDisplayModule.gameState.UIManager.playerItemArea.targetDisplay?.item == base.slotDisplay.item)
		{
			controlInfo.SelectSpellTarget(null);
		}
	}
}
