using UnityEngine;

public class ChangeCoolDownAction : GameAction
{
	public int delta;

	public Item item;

	public Item source;

	public ChangeCoolDownAction(Item item, Item source, int delta)
	{
		this.item = item;
		this.source = source;
		this.delta = delta;
	}

	protected override void Update()
	{
		if (item == null)
		{
			FinishAction();
			return;
		}
		ProtectedInt cooldown = item.cooldown;
		Item obj = item;
		obj.cooldown = (int)obj.cooldown + delta;
		item.cooldown = Mathf.Clamp(item.cooldown, 0, int.MaxValue);
		int finalCoolDelta = (int)item.cooldown - (int)cooldown;
		if (finalCoolDelta < 0 && (int)item.cooldown <= 0)
		{
			EnqueueAction(new FlickerSlotVFXAction(item, halt: false));
			ItemDisplayBase activeItemSlotDisplay = ItemDisplayBase.GetActiveItemSlotDisplay(item);
			if (activeItemSlotDisplay != null)
			{
				gameState.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.Block_coolDown, activeItemSlotDisplay.transform, activeItemSlotDisplay.transform.position);
			}
			item.cooldown = 0;
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerChangeCoolDown(item, source, finalCoolDelta, item.cooldown);
		});
		FinishAction();
	}
}
