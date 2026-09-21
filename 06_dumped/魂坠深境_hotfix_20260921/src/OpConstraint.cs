using System;
using System.Collections.Generic;

public class OpConstraint : ItemConstraint
{
	public enum Op
	{
		None,
		And,
		Or,
		Not
	}

	public ItemConstraint con1;

	public ItemConstraint con2;

	public Op op;

	public override bool? CheckItemAccept(Item item)
	{
		switch (op)
		{
		case Op.None:
			return false;
		case Op.And:
		{
			bool? flag = con1.CheckItemAccept(item);
			bool? flag2 = con2.CheckItemAccept(item);
			if (!flag.HasValue || !flag2.HasValue)
			{
				return null;
			}
			return flag.Value && flag2.Value;
		}
		case Op.Or:
		{
			bool? flag = con1.CheckItemAccept(item);
			bool? flag2 = con2.CheckItemAccept(item);
			if (!flag.HasValue || !flag2.HasValue)
			{
				return null;
			}
			return flag.Value || flag2.Value;
		}
		case Op.Not:
		{
			bool? flag = con1.CheckItemAccept(item);
			if (!flag.HasValue)
			{
				return null;
			}
			return !flag.Value;
		}
		default:
			throw new Exception($"type errpr: {(int)op}");
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { con1, con2 };
	}
}
