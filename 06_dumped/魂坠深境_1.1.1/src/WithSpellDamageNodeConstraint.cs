public class WithSpellDamageNodeConstraint : ItemConstraint
{
	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		if (item.itemSpells == null)
		{
			return false;
		}
		foreach (ItemSpell itemSpell in item.itemSpells)
		{
			if (itemSpell.spellNode.GetDescendantNodeOfType<DealDamage>().Count != 0)
			{
				return true;
			}
		}
		return false;
	}
}
