using UnityEngine;

public class zbBuffIndex16 : zbBuffBase
{
	public float moveRange;

	public override void RefreshProperty()
	{
		durationTime = extraTime;
		moveRange = param1;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			if (monsterById.GetCurPositionXY().x > monsterById.combatData.nodeRootPositonX)
			{
				moveRange = 0f - param1;
			}
			else
			{
				moveRange = param1;
			}
		}
	}

	public override void Free()
	{
		base.Free();
		moveRange = 0f;
	}
}
