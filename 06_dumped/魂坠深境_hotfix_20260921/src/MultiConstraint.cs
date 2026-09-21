using System;
using System.Collections.Generic;

public class MultiConstraint : ItemConstraint
{
	public enum Op
	{
		And,
		Or
	}

	public List<ItemConstraint> constraints;

	public Op op;

	public override bool? CheckItemAccept(Item item)
	{
		if (constraints == null)
		{
			return false;
		}
		bool? flag2;
		switch (op)
		{
		case Op.And:
			foreach (ItemConstraint constraint in constraints)
			{
				bool? flag3 = constraint.CheckItemAccept(item);
				flag2 = flag3;
				if (flag2 != true)
				{
					flag2 = flag3;
					goto end_IL_0017;
				}
			}
			return true;
		case Op.Or:
			foreach (ItemConstraint constraint2 in constraints)
			{
				bool? flag = constraint2.CheckItemAccept(item);
				if (flag != false)
				{
					flag2 = flag;
					goto end_IL_0017;
				}
			}
			return false;
		default:
			{
				throw new Exception($"type error:{(int)op}");
			}
			end_IL_0017:
			break;
		}
		return flag2;
	}

	public override List<IEffectNode> GetChildren()
	{
		if (constraints == null)
		{
			return new List<IEffectNode>();
		}
		return new List<IEffectNode>(constraints);
	}
}
