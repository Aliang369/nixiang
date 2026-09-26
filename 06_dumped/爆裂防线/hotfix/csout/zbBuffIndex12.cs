using UnityEngine;

public class zbBuffIndex12 : zbBuffBase
{
	private float currSlowDown;

	public override void Reset()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.DecSlowDownPercent(currSlowDown);
		}
		base.Reset();
		currSlowDown = 0f;
	}

	public override void RefreshProperty()
	{
		durationTime = extraTime;
		currSlowDown = buffRecord.param;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.AddSlowDownPercent(currSlowDown);
		}
	}

	public override void Free()
	{
		base.Free();
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.DecSlowDownPercent(currSlowDown);
		}
	}
}
