using System.Collections.Generic;
using UnityEngine;

public class ConsumeOverflowSlotAction : GameAction
{
	public ItemContainer targetContainer;

	public ConsumeOverflowSlotAction(ItemContainer targetContainer)
	{
		this.targetContainer = targetContainer;
		Ignore(ActionLock.Type.RemoveItem);
	}

	protected override void Update()
	{
		List<Item> list = targetContainer.GetAllItems().FindAll((Item item) => item.IsOverflow);
		list.Reverse();
		if (list.Count == 0)
		{
			FinishAction();
			return;
		}
		BattleStateInfo.Config battleStateInfoConfig = ScriptableObjectSingleton<VFXDataPreset>.instance.battleStateInfoConfig;
		bool flag = list.Count > battleStateInfoConfig.overflowDetailedAnimationCount || gameState.applicationState.settingManager.battleTimeScaleTypeSetting == TimeScaleManager.TimeScaleType.X4;
		int num = Mathf.Max(battleStateInfoConfig.overflowCompressedItemsPerFrame, 1);
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			bool waitForNextFrame = flag && (num2 + 1) % num == 0 && num2 < list.Count - 1;
			EnqueueAction(new OverflowItemAction(list[num2], !flag, waitForNextFrame));
		}
		if (flag)
		{
			EnqueueAction(new WaitUntilAction(() => !gameState.actionManager.IsActionLocked(ActionLock.Type.RemoveItem)));
		}
		EnqueueAction(new PushContainerEmptySlotsAction(targetContainer));
		FinishAction();
	}
}
