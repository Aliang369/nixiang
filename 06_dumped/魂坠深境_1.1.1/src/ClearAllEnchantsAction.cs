using System.Collections.Generic;

public class ClearAllEnchantsAction : ExecuteAction
{
	private readonly Item targetItem;

	private readonly object source;

	public ClearAllEnchantsAction(Item targetItem, object source)
	{
		this.targetItem = targetItem;
		this.source = source;
	}

	protected override void Update()
	{
		if (targetItem == null)
		{
			return;
		}
		foreach (Enchant item in new List<Enchant>(targetItem.Enchants))
		{
			if (item != null)
			{
				gameState.gameEventManager.TriggerBeforeRemoveEnchant(source, targetItem, item);
				targetItem.RemoveEnchant(item);
			}
		}
	}
}
