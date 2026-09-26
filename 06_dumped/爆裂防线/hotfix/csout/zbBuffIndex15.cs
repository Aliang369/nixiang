using UnityEngine;

public class zbBuffIndex15 : zbBuffBase
{
	private float currSlowDown = 1f;

	private float lastSlowDown = 1f;

	private float currInterval;

	public override void Reset()
	{
		base.Reset();
	}

	public override void RefreshProperty()
	{
		currInterval = 0f;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.AddSlowDownPercent(currSlowDown);
			lastSlowDown = currSlowDown;
		}
	}

	public override void Update()
	{
		base.Update();
		currInterval += Time.deltaTime;
		if (currInterval <= durationTime)
		{
			zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
			if ((Object)(object)monsterById != (Object)null)
			{
				currSlowDown *= 1f - currInterval / durationTime;
				monsterById.DecSlowDownPercent(lastSlowDown);
				monsterById.AddSlowDownPercent(currSlowDown);
				lastSlowDown = currSlowDown;
			}
		}
	}

	public override void Free()
	{
		base.Free();
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.DecSlowDownPercent(lastSlowDown);
		}
	}
}
