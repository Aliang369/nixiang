using UnityEngine;

public class DamageValueGenerator : ValueGenerator
{
	public enum ValueType
	{
		SubCounter,
		FinalDamage,
		DamageCommand,
		OverflowDamage
	}

	[SerializeField]
	private ValueType type;

	public override int? GenerateValue(GameEvent eve)
	{
		if (type == ValueType.DamageCommand)
		{
			return eve.damagecommand.baseDamage;
		}
		if (eve.damage == null)
		{
			return null;
		}
		return type switch
		{
			ValueType.SubCounter => eve.damage.subCounter, 
			ValueType.FinalDamage => eve.damage.finalDamage, 
			ValueType.OverflowDamage => eve.damage.overflowDamage, 
			_ => 0, 
		};
	}
}
