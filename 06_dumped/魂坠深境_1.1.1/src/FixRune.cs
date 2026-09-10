using System.Collections.Generic;

public class FixRune : RuneGenerator
{
	public RuneDef runeDef;

	public ValueGenerator stackGenerator;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { stackGenerator };
	}

	public override List<Rune> GenerateRunes(GameEvent eve)
	{
		int value = stackGenerator?.GenerateValue(eve) ?? 1;
		Rune item = new Rune(runeDef, value);
		return new List<Rune> { item };
	}
}
