public class CleanInvalidEnchantAndRuneAction : ExecuteAction
{
	public Item targetItem;

	public CleanInvalidEnchantAndRuneAction(Item targetItem)
	{
		this.targetItem = targetItem;
	}

	protected override void Update()
	{
		foreach (Enchant enchant in targetItem.Enchants)
		{
			if (enchant.def.addConstraint != null && enchant.def.addConstraint.CheckItemAccept(targetItem) == false)
			{
				gameState.actionManager.EnqueueAction(new ChangeEnchantAction(targetItem, null, enchant.def, -(int)enchant.value, popup: false));
			}
		}
		foreach (Rune rune in targetItem.Runes)
		{
			if (rune.def.CheckAddConstraint(targetItem) != true)
			{
				gameState.actionManager.EnqueueAction(new ChangeRuneAction(targetItem, rune.def, -rune.value, null));
			}
		}
	}
}
