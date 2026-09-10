using System.Collections.Generic;

public class NestedModifier : DraftLocationModifier
{
	public List<DraftLocationModifier> modifiers;

	public override void ModifyLocation(PathLocation location, DraftPivot pivot, DungeonMap map, GameStateInfo gs)
	{
		foreach (DraftLocationModifier modifier in modifiers)
		{
			modifier.ModifyLocation(location, pivot, map, gs);
		}
	}
}
