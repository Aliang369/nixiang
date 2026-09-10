using System;
using System.Collections.Generic;
using UnityEngine;

public class MaxValueGenerator : ValueGenerator
{
	[SerializeField]
	private ValueGenerator valueGenerator1;

	[SerializeField]
	private ValueGenerator valueGenerator2;

	public override int? GenerateValue(GameEvent eve)
	{
		int valueOrDefault = (valueGenerator1?.GenerateValue(eve)).GetValueOrDefault();
		int valueOrDefault2 = (valueGenerator2?.GenerateValue(eve)).GetValueOrDefault();
		return Math.Max(valueOrDefault, valueOrDefault2);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { valueGenerator1, valueGenerator2 };
	}
}
