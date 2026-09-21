using System;
using UnityEngine;

public class ChangeTempHealthValueGenerator : ValueGenerator
{
	private enum TempHealthValueType
	{
		ActualDelta,
		ActualRemoved,
		RequestDelta,
		RequestRemoved
	}

	[SerializeField]
	private TempHealthValueType type;

	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.type != GameEvent.Type.BftHealthChange || !eve.boolValue)
		{
			return null;
		}
		return type switch
		{
			TempHealthValueType.ActualDelta => eve.deltaValue, 
			TempHealthValueType.ActualRemoved => Math.Max(0, -eve.deltaValue), 
			TempHealthValueType.RequestDelta => eve.amount, 
			TempHealthValueType.RequestRemoved => Math.Max(0, -eve.amount), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
