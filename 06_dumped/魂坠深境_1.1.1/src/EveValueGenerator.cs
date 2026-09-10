using System;
using UnityEngine;

public class EveValueGenerator : ValueGenerator
{
	public enum ValueType
	{
		Amount,
		Delta,
		DamageSubCounter,
		Amount2
	}

	[SerializeField]
	private ValueType type;

	public override int? GenerateValue(GameEvent eve)
	{
		return type switch
		{
			ValueType.Amount => eve.amount, 
			ValueType.Amount2 => eve.amount2, 
			ValueType.Delta => eve.deltaValue, 
			ValueType.DamageSubCounter => eve.damage?.subCounter ?? 0, 
			_ => throw new Exception($"type error: {(int)type}"), 
		};
	}
}
