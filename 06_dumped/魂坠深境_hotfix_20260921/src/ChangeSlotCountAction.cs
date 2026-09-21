using UnityEngine;

public class ChangeSlotCountAction : ExecuteAction
{
	private readonly ItemContainer container;

	private int count;

	private readonly bool temp;

	public ChangeSlotCountAction(ItemContainer container, int count, bool temp)
	{
		this.container = container;
		this.count = count;
		this.temp = temp;
	}

	protected override void Update()
	{
		if (temp)
		{
			count = Mathf.Max(container.tempFixedSlotCount + count, 0);
		}
		else
		{
			count = Mathf.Max(container.persistentFixedSlotCount + count, 0);
		}
		container.SetSlotCount(count, temp);
		if (!temp)
		{
			gameState.gameEventManager.TriggerContainerSlotCountChange(container, count);
		}
	}
}
