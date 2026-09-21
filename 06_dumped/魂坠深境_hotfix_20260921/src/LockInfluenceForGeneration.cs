using System.Collections.Generic;

public class LockInfluenceForGeneration : Effect
{
	public InfluenceDefGenerator InfluenceDefGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		foreach (InfluenceDef item in (InfluenceDefGenerator?.GenerateInfluenceDefs(eve)).OrEmptyIfNull())
		{
			eve.gameState.generationManager.LockContentGeneration(item);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { InfluenceDefGenerator };
	}
}
