using UnityEngine;

public class zbBuffBeDamageUp : zbBuffBase
{
	private float m_Damage;

	public override void Reset()
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.beDamageUpPercent.Value = (float)monsterById.beDamageUpPercent - buffRecord.param;
		}
		base.Reset();
	}

	public override void RefreshProperty()
	{
		durationTime = extraTime;
		m_Damage = buffRecord.param;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.beDamageUpPercent.Value = (float)monsterById.beDamageUpPercent + buffRecord.param;
		}
	}

	public override void Free()
	{
		base.Free();
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			monsterById.beDamageUpPercent.Value = (float)monsterById.beDamageUpPercent - m_Damage;
		}
		m_Damage = 0f;
	}
}
