using UnityEngine;

public class CheckSourceItemDef : CheckNode
{
	[SerializeField]
	private ItemDefGenerator def;

	public override bool? Check(GameEvent eve)
	{
		ItemDefinition itemDefinition = def.GenerateSingleItemDef(eve);
		if (itemDefinition == null)
		{
			return null;
		}
		return eve.SourceItem?.GetDef() == itemDefinition;
	}
}
