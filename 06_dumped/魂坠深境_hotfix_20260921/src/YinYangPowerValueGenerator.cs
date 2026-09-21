using System;
using UnityEngine;

public class YinYangPowerValueGenerator : ValueGenerator
{
	[SerializeField]
	private TrigramsManager.YinYangType type;

	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.triggerType == GameEvent.TriggerType.OutBattleDisplay)
		{
			return null;
		}
		return type switch
		{
			TrigramsManager.YinYangType.Yin => eve.gameState?.trigramsManager?.yinPower, 
			TrigramsManager.YinYangType.Yang => eve.gameState?.trigramsManager?.yangPower, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
