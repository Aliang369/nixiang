using System.Collections.Generic;
using UnityEngine;

public class ItemCount : ValueGenerator
{
	[SerializeField]
	private ItemTargetSelector pool;

	public override int? GenerateValue(GameEvent eve)
	{
		return pool.GetTargetItems(eve)?.Count;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { pool };
	}
}
