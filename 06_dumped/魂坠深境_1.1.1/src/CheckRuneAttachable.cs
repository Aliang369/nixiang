using System.Collections.Generic;

public class CheckRuneAttachable : CheckNode
{
	public RuneGenerator runeGenerator;

	public ItemTargetSelector ItemTargetSelector;

	public override bool? Check(GameEvent eve)
	{
		List<Rune> list = runeGenerator?.GenerateRunes(eve);
		List<Item> list2 = ItemTargetSelector?.GetTargetItems(eve);
		if (list == null || list2 == null)
		{
			return null;
		}
		foreach (Rune item in list)
		{
			foreach (Item item2 in list2)
			{
				if (item.def.CheckCanAddRuneTo(item2) != true)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { runeGenerator, ItemTargetSelector };
	}
}
