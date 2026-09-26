using UnityEngine;

public class zbBuffIndex34 : zbBuffBase
{
	private float currMass;

	public override void Reset()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.DecHitRatePercent(currMass);
		}
		base.Reset();
		currMass = 0f;
	}

	public override void RefreshProperty()
	{
		durationTime = extraTime;
		currMass = param1;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.AddHitRatePercent(currMass);
		}
	}

	public override void Free()
	{
		base.Free();
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.DecHitRatePercent(currMass);
		}
		currMass = 0f;
	}
}
