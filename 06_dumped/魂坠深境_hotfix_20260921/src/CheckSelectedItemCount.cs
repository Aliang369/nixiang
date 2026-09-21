using System.Collections.Generic;
using UnityEngine;

public class CheckSelectedItemCount : CheckNode
{
	[SerializeField]
	private ValueCompareCheckNode.CompareType compareType;

	[SerializeField]
	private ValueGenerator countGenerator;

	public override bool? Check(GameEvent eve)
	{
		if (eve?.SelectedItems == null)
		{
			return null;
		}
		int? num = countGenerator?.GenerateValue(eve);
		if (!num.HasValue)
		{
			return null;
		}
		return compareType switch
		{
			ValueCompareCheckNode.CompareType.Equal => eve.SelectedItems.Count == num.Value, 
			ValueCompareCheckNode.CompareType.Greater => eve.SelectedItems.Count > num.Value, 
			ValueCompareCheckNode.CompareType.Less => eve.SelectedItems.Count < num.Value, 
			ValueCompareCheckNode.CompareType.GreaterEqual => eve.SelectedItems.Count >= num.Value, 
			ValueCompareCheckNode.CompareType.LessEqual => eve.SelectedItems.Count <= num.Value, 
			_ => false, 
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { countGenerator };
	}
}
