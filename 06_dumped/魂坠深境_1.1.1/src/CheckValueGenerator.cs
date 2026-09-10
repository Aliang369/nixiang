using System.Collections.Generic;
using UnityEngine;

public class CheckValueGenerator : ValueGenerator
{
	[SerializeField]
	private CheckNode checker;

	[SerializeField]
	private ValueGenerator trueValueGenerator;

	[SerializeField]
	private ValueGenerator falseValueGenerator;

	public override int? GenerateValue(GameEvent eve)
	{
		bool? flag = checker?.Check(eve);
		if (!flag.HasValue)
		{
			return null;
		}
		if (flag == true)
		{
			if (trueValueGenerator != null)
			{
				return trueValueGenerator.GenerateValue(eve);
			}
			return 0;
		}
		if (falseValueGenerator != null)
		{
			return falseValueGenerator.GenerateValue(eve);
		}
		return 0;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { checker, trueValueGenerator, falseValueGenerator };
	}
}
