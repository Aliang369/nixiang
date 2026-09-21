using System;
using System.Collections.Generic;

public class OpChecKNode : CheckNode
{
	public enum Op
	{
		And,
		Or,
		NotOrNull,
		Not
	}

	public CheckNode n1;

	public CheckNode n2;

	public Op op;

	public override bool? Check(GameEvent eve)
	{
		switch (op)
		{
		case Op.And:
		{
			bool? flag = n1?.Check(eve);
			bool? flag2 = n2?.Check(eve);
			if (!flag.HasValue || !flag2.HasValue)
			{
				return null;
			}
			return flag == true && flag2 == true;
		}
		case Op.Or:
		{
			bool? flag = n1?.Check(eve);
			bool? flag2 = n2?.Check(eve);
			if (!flag.HasValue || !flag2.HasValue)
			{
				return null;
			}
			return flag == true || flag2 == true;
		}
		case Op.NotOrNull:
		{
			bool? flag = n1?.Check(eve);
			return !flag.HasValue || flag == false;
		}
		case Op.Not:
		{
			bool? flag = n1?.Check(eve);
			return flag.HasValue && flag == false;
		}
		default:
			throw new Exception($"op type error{(int)op}");
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { n1, n2 };
	}
}
