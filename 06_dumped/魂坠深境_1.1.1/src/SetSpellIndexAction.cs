public class SetSpellIndexAction : ExecuteAction
{
	public int index;

	public Item targetItem;

	public SetSpellIndexAction(Item targetItem, int index)
	{
		this.targetItem = targetItem;
		this.index = index;
	}

	protected override void Update()
	{
		targetItem.CurrentSpellIndex = index;
	}
}
