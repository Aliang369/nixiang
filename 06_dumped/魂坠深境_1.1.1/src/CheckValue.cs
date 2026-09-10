using System.Collections.Generic;
using UnityEngine;

public class CheckValue : CheckNode
{
	public enum CheckType
	{
		Even,
		Odd,
		EqualZero,
		GreaterThanZero,
		SmallThanZero,
		EqualOne,
		NotZero,
		SmallEqualThanZeo,
		GreaterThanOne
	}

	[SerializeField]
	private CheckType type;

	[SerializeField]
	private ValueGenerator valueGenerator;

	public override bool? Check(GameEvent eve)
	{
		int? num = valueGenerator?.GenerateValue(eve);
		if (!num.HasValue)
		{
			return null;
		}
		return type switch
		{
			CheckType.Even => num % 2 == 0, 
			CheckType.Odd => num % 2 == 1, 
			CheckType.EqualZero => num == 0, 
			CheckType.GreaterThanZero => num > 0, 
			CheckType.SmallThanZero => num < 0, 
			CheckType.EqualOne => num == 1, 
			CheckType.NotZero => num != 0, 
			CheckType.SmallEqualThanZeo => num <= 0, 
			CheckType.GreaterThanOne => num > 1, 
			_ => false, 
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { valueGenerator };
	}
}
