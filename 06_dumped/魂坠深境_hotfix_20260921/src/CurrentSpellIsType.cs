public class CurrentSpellIsType : ItemConstraint
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
		if (item.CurrentSpell != null && item.CurrentSpell.IntentType == type)
		{
			value = true;
		}
		return value;
	}
}
