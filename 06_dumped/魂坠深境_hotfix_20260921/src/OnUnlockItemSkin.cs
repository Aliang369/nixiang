using System;
using System.Collections.Generic;
using System.Linq;

public class OnUnlockItemSkin : GlobalTriggerObserver
{
	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public override List<IEffectNode> GetChildren()
	{
		return null;
	}

	protected override void InitImpl(ApplicationStateInfo applicationState, Action onTrigger)
	{
		bool triggered = false;
		applicationState.eventManager.unlockItemSkinEvent.AddListener(CheckAndTrigger);
		ItemSkinDef itemSkin = (from state in applicationState.itemSkinManager.unlockedItemSkinLevels.OrEmptyIfNull()
			select state?.skinDef).FirstOrDefault((ItemSkinDef itemSkinDef) => itemSkinDef != null && itemSkinDef.IsValidAsset());
		CheckAndTrigger(itemSkin);
		void CheckAndTrigger(ItemSkinDef itemSkinDef)
		{
			if (!triggered && !(itemSkinDef == null) && itemSkinDef.IsValidAsset())
			{
				triggered = true;
				onTrigger();
			}
		}
	}
}
