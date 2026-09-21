using System.Collections.Generic;

public abstract class SelectGetItemBonus : BattleBonusObject
{
	[SaveField(null, null)]
	public List<Item> allItems;

	[SaveField(null, null)]
	public List<Item> selectItems;

	public int minSelectCount;

	public override bool isChoose => true;

	public static int maxSelectCount => 1;

	public override object infoObject
	{
		get
		{
			if (allItems.Count <= 0)
			{
				return null;
			}
			return allItems[0];
		}
	}
}
