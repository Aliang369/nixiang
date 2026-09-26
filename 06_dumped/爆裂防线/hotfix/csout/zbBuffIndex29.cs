using UnityEngine;

public class zbBuffIndex29 : zbBuffBase
{
	private float currLocalScale;

	public override void Reset()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.DecLocalScalePercent(currLocalScale);
		}
		base.Reset();
		currLocalScale = 0f;
	}

	public override void RefreshProperty()
	{
		durationTime = extraTime;
		currLocalScale = param1;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.AddLocalScalePercent(currLocalScale);
		}
	}

	public override void Free()
	{
		base.Free();
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.DecLocalScalePercent(currLocalScale);
		}
		currLocalScale = 0f;
	}
}
