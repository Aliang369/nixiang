using System.Collections.Generic;
using UnityEngine;

public class CheckTargetItemDef : CheckNode
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
		return eve.TargetItem?.GetDef() == itemDefinition;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { def };
	}
}
