using System.Collections.Generic;

public class LockItemForGeneration : Effect
{
	public ItemDefGenerator ItemDefGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		foreach (ItemDefinition item in (ItemDefGenerator?.GenerateItemDefs(eve)).OrEmptyIfNull())
		{
			eve.gameState.generationManager.LockContentGeneration(item);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { ItemDefGenerator };
	}
}
