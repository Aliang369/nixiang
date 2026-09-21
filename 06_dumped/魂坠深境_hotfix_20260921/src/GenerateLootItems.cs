using System.Collections.Generic;
using UnityEngine;

public class GenerateLootItems : NewItemGenerator
{
	[SerializeField]
	private ValueGenerator valueGenerator;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		if (eve.gameState?.generationManager == null)
		{
			return null;
		}
		int? num = valueGenerator.GenerateValue(eve);
		if (!num.HasValue)
		{
			return null;
		}
		return eve.gameState.generationManager.GenerateRandomLoot(num.Value);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { valueGenerator };
	}
}
