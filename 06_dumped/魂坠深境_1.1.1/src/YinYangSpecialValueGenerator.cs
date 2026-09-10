using System;
using UnityEngine;

public class YinYangSpecialValueGenerator : ValueGenerator
{
	private enum Type
	{
		TurnConvertYang = 1,
		TurnConvertYin = 2,
		TurnConvertTotal = 3,
		Difference = 10,
		YinYangTotal = 20,
		MinusYin = 21,
		MinusYang = 22,
		ConstantMaxYang = 30,
		ConstantMaxYin = 31
	}

	[SerializeField]
	private Type type;

	public override int? GenerateValue(GameEvent eve)
	{
		TrigramsManager trigramsManager = eve.gameState?.trigramsManager;
		if (trigramsManager == null)
		{
			return null;
		}
		return type switch
		{
			Type.TurnConvertYang => trigramsManager.turnConvertYang, 
			Type.TurnConvertYin => trigramsManager.turnConvertYin, 
			Type.TurnConvertTotal => trigramsManager.turnConvertYang + trigramsManager.turnConvertYin, 
			Type.Difference => Mathf.Abs((int)trigramsManager.yinPower - (int)trigramsManager.yangPower), 
			Type.YinYangTotal => (int)trigramsManager.yinPower + (int)trigramsManager.yangPower, 
			Type.MinusYin => -(int)trigramsManager.yinPower, 
			Type.MinusYang => -(int)trigramsManager.yangPower, 
			Type.ConstantMaxYang => trigramsManager.constantMaxYangPower, 
			Type.ConstantMaxYin => trigramsManager.constantMaxYinPower, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
