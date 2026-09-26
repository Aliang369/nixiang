using UnityEngine;

public class zbBuffIndex30 : zbBuffBase
{
	private float currMass;

	public override void Reset()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.DecMassPercent(currMass);
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
			monsterById.AddMassPercent(currMass);
		}
	}

	public override void Free()
	{
		base.Free();
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.DecMassPercent(currMass);
		}
		currMass = 0f;
	}
}
