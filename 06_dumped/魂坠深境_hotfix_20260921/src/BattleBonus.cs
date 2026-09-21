using System;
using System.Collections.Generic;

public class BattleBonus
{
	[SaveField(null, null)]
	public string title;

	public Action onEnd;

	[SaveField(null, null)]
	public List<BattleBonusObject> bonuses;

	public BattleBonus()
	{
		bonuses = new List<BattleBonusObject>();
	}

	public BattleBonus(List<BattleBonusObject> bonuses)
	{
		this.bonuses = bonuses;
	}
}
