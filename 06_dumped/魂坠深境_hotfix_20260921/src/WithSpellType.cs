public class WithSpellType : ItemConstraint
{
	public IntentType type;

	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		bool value = false;
		if (item.itemSpells != null)
		{
			foreach (ItemSpell itemSpell in item.itemSpells)
			{
				if (itemSpell.IntentType == type)
				{
					value = true;
				}
			}
		}
		return value;
	}
}
