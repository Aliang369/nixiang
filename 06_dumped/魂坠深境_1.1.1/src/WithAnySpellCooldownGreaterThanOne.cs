public class WithAnySpellCooldownGreaterThanOne : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		if (item == null || item.itemSpells == null)
		{
			return false;
		}
		foreach (ItemSpell itemSpell in item.itemSpells)
		{
			if (itemSpell.GetCoolDown(item) > 1)
			{
				return true;
			}
		}
		return false;
	}
}
