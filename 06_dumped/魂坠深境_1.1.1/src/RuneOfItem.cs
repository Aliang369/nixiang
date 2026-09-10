using System.Collections.Generic;

public class RuneOfItem : RuneGenerator
{
	public ItemTargetSelector itemSelector;

	public bool clone;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemSelector };
	}

	public override List<Rune> GenerateRunes(GameEvent eve)
	{
		if (eve.triggerType != GameEvent.TriggerType.Real)
		{
			return null;
		}
		List<Item> list = itemSelector?.GetTargetItems(eve);
		if (list.IsNullOrEmpty())
		{
			return null;
		}
		List<Rune> list2 = new List<Rune>();
		foreach (Item item in list)
		{
			foreach (Rune rune in item.Runes)
			{
				list2.Add(clone ? new Rune(rune.def, rune.value) : rune);
			}
		}
		return list2;
	}
}
