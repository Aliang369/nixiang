using System.Collections.Generic;

public class PoolGenerateRune : RuneGenerator
{
	public ValueGenerator countGenerator;

	public QualityLevel filterQuality;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { countGenerator };
	}

	public override List<Rune> GenerateRunes(GameEvent eve)
	{
		if (eve.triggerType != GameEvent.TriggerType.Real)
		{
			return null;
		}
		int count = countGenerator?.GenerateValue(eve) ?? 1;
		return eve.gameState.generationManager.GenerateMintageRunes(count, null, filterQuality);
	}
}
