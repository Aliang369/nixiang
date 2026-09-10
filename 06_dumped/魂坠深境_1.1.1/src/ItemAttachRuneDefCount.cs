using System.Collections.Generic;

public class ItemAttachRuneDefCount : ValueGenerator
{
	public ItemTargetSelector target;

	public override int? GenerateValue(GameEvent eve)
	{
		HashSet<RuneDef> hashSet = new HashSet<RuneDef>();
		List<Item> targetItems = target.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		foreach (Item item in targetItems)
		{
			foreach (Rune rune in item.Runes)
			{
				hashSet.Add(rune.def);
			}
		}
		return hashSet.Count;
	}
}
