using System.Collections.Generic;
using UnityEngine;

public class NewItemSelector : ItemTargetSelector
{
	[SerializeField]
	private NewItemGenerator newItemGenerator;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		return newItemGenerator.GenerateNewItems(eve);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { newItemGenerator };
	}
}
