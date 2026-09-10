using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MultiCheckNode : CheckNode
{
	public enum Op
	{
		AllTrue,
		AnyTrue,
		AllFalse,
		AnyFalse
	}

	[SerializeField]
	private Op op;

	[SerializeField]
	private List<CheckNode> checks = new List<CheckNode>();

	public override bool? Check(GameEvent eve)
	{
		if (checks == null || checks.Count == 0)
		{
			return null;
		}
		return op switch
		{
			Op.AllTrue => checks.TrueForAll((CheckNode c) => c.Check(eve) == true), 
			Op.AnyTrue => checks.Find((CheckNode c) => c.Check(eve) == true) != null, 
			Op.AllFalse => checks.TrueForAll((CheckNode c) => c.Check(eve) == false), 
			Op.AnyFalse => checks.Find((CheckNode c) => c.Check(eve) == false) != null, 
			_ => throw new Exception($"Op type error: {(int)op}"), 
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		if (checks == null || checks.Count == 0)
		{
			return null;
		}
		return checks.Cast<IEffectNode>().ToList();
	}
}
