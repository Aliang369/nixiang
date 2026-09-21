using System.Collections.Generic;
using UnityEngine;

public class CheckItemGenerator : NewItemGenerator
{
	[SerializeField]
	private CheckNode checker;

	[SerializeField]
	private NewItemGenerator falseNode;

	[SerializeField]
	private NewItemGenerator trueNode;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		bool? flag = checker.Check(eve);
		if (flag.HasValue)
		{
			if (flag == true)
			{
				if (trueNode != null)
				{
					return trueNode.GenerateNewItems(eve);
				}
				return new List<Item>();
			}
			if (falseNode != null)
			{
				return falseNode.GenerateNewItems(eve);
			}
			return new List<Item>();
		}
		return null;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { checker, falseNode, trueNode };
	}
}
