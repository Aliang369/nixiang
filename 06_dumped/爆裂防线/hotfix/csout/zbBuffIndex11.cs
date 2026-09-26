using UnityEngine;

public class zbBuffIndex11 : zbBuffBase
{
	public override void Reset()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.addPercentMass /= buffRecord.param;
		}
		base.Reset();
	}

	public override void RefreshProperty()
	{
		durationTime = extraTime;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.addPercentMass *= buffRecord.param;
		}
	}

	public override void Free()
	{
		base.Free();
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.addPercentMass /= buffRecord.param;
		}
	}
}
