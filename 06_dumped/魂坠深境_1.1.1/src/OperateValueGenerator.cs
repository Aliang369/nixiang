using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OperateValueGenerator : ValueGenerator
{
	public enum Op
	{
		Add = 1,
		Sub,
		Mul,
		Div,
		Min,
		Max,
		MaxClamp,
		MinClamp,
		Inverse,
		Sign,
		Percent,
		AddOne,
		SubOne,
		LeastCommonMultipleNoZero,
		Abs,
		Mirror,
		Distance
	}

	[SerializeField]
	private ValueGenerator first;

	[SerializeField]
	private ValueGenerator second;

	[SerializeField]
	private readonly List<ValueGenerator> mutilVg;

	[SerializeField]
	private Op op;

	public override int? GenerateValue(GameEvent eve)
	{
		int? num = 0;
		if (first != null)
		{
			num = first.GenerateValue(eve);
		}
		int? num2 = 0;
		if (second != null)
		{
			num2 = second.GenerateValue(eve);
		}
		List<int?> list = new List<int?>();
		if (mutilVg != null)
		{
			foreach (ValueGenerator item2 in mutilVg)
			{
				if (item2 != null)
				{
					int? item = item2.GenerateValue(eve);
					list.Add(item);
				}
			}
		}
		switch (op)
		{
		case Op.Add:
			if (mutilVg != null)
			{
				if (list.TrueForAll((int? a) => !a.HasValue))
				{
					return null;
				}
				return list.FindAll((int? a) => a.HasValue).Sum();
			}
			if (!num.HasValue && !num2.HasValue)
			{
				return null;
			}
			return num.GetValueOrDefault() + num2.GetValueOrDefault();
		case Op.Sub:
			if (!num.HasValue && !num2.HasValue)
			{
				return null;
			}
			return num.GetValueOrDefault() - num2.GetValueOrDefault();
		case Op.Mul:
			if (mutilVg != null)
			{
				if (list.Contains(null))
				{
					return null;
				}
				return list.Aggregate((int? a, int? b) => a * b);
			}
			if (!num.HasValue || !num2.HasValue)
			{
				return null;
			}
			return num * num2;
		case Op.Div:
			if (!num.HasValue || !num2.HasValue)
			{
				return null;
			}
			return num / num2;
		case Op.Min:
			if (mutilVg != null)
			{
				if (list.Contains(null))
				{
					return null;
				}
				return list.Min();
			}
			if (!num.HasValue || !num2.HasValue)
			{
				return null;
			}
			return Mathf.Min(num.Value, num2.Value);
		case Op.Max:
			if (mutilVg != null)
			{
				if (list.Contains(null))
				{
					return null;
				}
				return list.Max();
			}
			if (!num.HasValue || !num2.HasValue)
			{
				return null;
			}
			return Mathf.Max(num.Value, num2.Value);
		case Op.MaxClamp:
			if (!num.HasValue || !num2.HasValue)
			{
				return null;
			}
			return Mathf.Clamp(num.Value, int.MinValue, num2.Value);
		case Op.MinClamp:
			if (!num.HasValue || !num2.HasValue)
			{
				return null;
			}
			return Mathf.Clamp(num.Value, num2.Value, int.MaxValue);
		case Op.Inverse:
			if (!num.HasValue)
			{
				return null;
			}
			return -num;
		case Op.Sign:
			if (!num.HasValue)
			{
				return null;
			}
			return (num < 0) ? (-1) : ((num > 0) ? 1 : 0);
		case Op.Percent:
			if (!num.HasValue || !num2.HasValue)
			{
				return null;
			}
			return Mathf.RoundToInt((float)num.Value * ((float)num2.Value / 100f));
		case Op.AddOne:
			if (!num.HasValue)
			{
				return 1;
			}
			return num + 1;
		case Op.SubOne:
			if (!num.HasValue)
			{
				return null;
			}
			return num - 1;
		case Op.LeastCommonMultipleNoZero:
			if (mutilVg != null)
			{
				list.RemoveAll((int? t) => t == 0);
				return MathHelper.lcm(list.FindAll((int? a) => a.HasValue).ConvertAll((int? a) => a.Value).ToArray());
			}
			if (!num.HasValue || !num2.HasValue)
			{
				return null;
			}
			if (num != 0)
			{
				return (num2 != 0) ? MathHelper.lcm(num.Value, num2.Value) : 0;
			}
			return num2;
		case Op.Abs:
			if (!num.HasValue)
			{
				return null;
			}
			return Mathf.Abs(num.Value);
		case Op.Mirror:
			if (!num.HasValue || !num2.HasValue)
			{
				return null;
			}
			return num2 + (num2 - num);
		case Op.Distance:
			if (!num.HasValue || !num2.HasValue)
			{
				return null;
			}
			return Mathf.Abs(num.Value - num2.Value);
		default:
			return 0;
		}
	}

	private bool NeedMultiValues()
	{
		if (!IsSingleOperator() && op != Op.Distance)
		{
			return op != Op.Percent;
		}
		return false;
	}

	private bool IsSingleOperator()
	{
		if (op != Op.Inverse && op != Op.Sign && op != Op.AddOne && op != Op.SubOne)
		{
			return op == Op.Abs;
		}
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode> { first, second };
		if (mutilVg != null)
		{
			foreach (ValueGenerator item in mutilVg)
			{
				list.Add(item);
			}
		}
		return list;
	}
}
