using Proto.Enums;
using UnityEngine;

public class zbBuffSlowDown : zbBuffBase
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
		durationTime = GetAttrValueByType(EAttrSkillType.SlowTime) + extraTime;
		currSlowDown = GetAttrValueByType(EAttrSkillType.SlowDown);
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
