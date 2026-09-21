using System.Collections.Generic;
using UnityEngine;

public class ValueCompareCheckNode : CheckNode
{
	public enum CompareType
	{
		Equal,
		Greater,
		Less,
		GreaterEqual,
		LessEqual
	}

	[SerializeField]
	private CompareType type;

	[SerializeField]
	private ValueGenerator vga;

	[SerializeField]
	private ValueGenerator vgb;

	public static bool Compare(int va, int vb, CompareType type)
	{
		return type switch
		{
			CompareType.Equal => va == vb, 
			CompareType.Greater => va > vb, 
			CompareType.Less => va < vb, 
			CompareType.GreaterEqual => va >= vb, 
			CompareType.LessEqual => va <= vb, 
			_ => false, 
		};
	}

	public override bool? Check(GameEvent eve)
	{
		int? num = vga?.GenerateValue(eve);
		int? num2 = vgb?.GenerateValue(eve);
		if (!num.HasValue || !num2.HasValue)
		{
			return null;
		}
		return Compare(num.Value, num2.Value, type);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { vga, vgb };
	}
}
