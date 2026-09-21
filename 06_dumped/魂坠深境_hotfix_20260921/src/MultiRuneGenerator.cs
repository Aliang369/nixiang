using System.Collections.Generic;

public class MultiRuneGenerator : RuneGenerator
{
	public List<RuneGenerator> generators;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>(generators);
	}

	public override List<Rune> GenerateRunes(GameEvent eve)
	{
		List<Rune> list = new List<Rune>();
		foreach (RuneGenerator item in generators.OrEmptyIfNull())
		{
			list.AddRange(item.GenerateRunes(eve));
		}
		return list;
	}
}
