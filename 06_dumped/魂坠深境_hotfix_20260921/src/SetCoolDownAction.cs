using UnityEngine;

public class SetCoolDownAction : ExecuteAction
{
	public Item item;

	public Item source;

	public int target;

	public SetCoolDownAction(Item item, Item source, int target)
	{
		this.item = item;
		this.source = source;
		this.target = Mathf.Max(target, 0);
	}

	protected override void Update()
	{
		ProtectedInt cooldown = item.cooldown;
		item.cooldown = target;
		if ((int)item.cooldown < 0)
		{
			item.cooldown = 0;
		}
		int delta = (int)item.cooldown - (int)cooldown;
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerChangeCoolDown(item, source, delta, item.cooldown);
		});
	}
}
