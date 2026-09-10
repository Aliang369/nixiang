using System.Collections.Generic;

public class SpiderWebbedAbility : Ability
{
	private const string MagneticRepulsionItemGuid = "9c9aa84216c8ae2479c0b28af85bb77e";

	private const string LightningAttractionItemGuid = "8d36abb1bfbad174eb70c242ca55b244";

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.AfterSwapItem;
		yield return GameEvent.Type.MoveSlotIndex;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve) || eve.CasterEnchant?.attachItem == null)
		{
			return false;
		}
		if (eve.type == GameEvent.Type.AfterSwapItem)
		{
			if (eve.SourceItem != eve.CasterEnchant.attachItem)
			{
				return eve.TargetItem == eve.CasterEnchant.attachItem;
			}
			return true;
		}
		if (eve.type == GameEvent.Type.MoveSlotIndex && eve.TargetItem == eve.CasterEnchant.attachItem)
		{
			if (!(eve.SourceItem?.def?.id == "9c9aa84216c8ae2479c0b28af85bb77e"))
			{
				return eve.SourceItem?.def?.id == "8d36abb1bfbad174eb70c242ca55b244";
			}
			return true;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			eve.gameState.actionManager.EnqueueAction(new ChangeEnchantAction(eve.CasterEnchant.attachItem, eve.SourceItem, eve.CasterEnchant.def, -1));
			string text = (eve.CasterEnchant.def.Name + "-1").ColorText(eve.CasterEnchant.def.displayColor);
			PopupTextAction action = new PopupTextAction(eve.CasterEnchant.attachItem, text, ScriptableObjectSingleton<VFXDataPreset>.instance.enchantPopupTextPrefab)
			{
				popupIcon = eve.CasterEnchant.def.Icon
			};
			eve.gameState.actionManager.EnqueueAction(action);
		}
	}
}
