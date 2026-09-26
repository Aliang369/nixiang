using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbBuffBeHitTodo : zbBuffBase
{
	private float damageInterval;

	private float currInterval;

	private Vector2 targetMonsterPos;

	private List<zbMonster> monsterRangeList;

	public override void Reset()
	{
		base.Reset();
		currInterval = 0f;
		damageInterval = 0f;
		monsterRangeList.Clear();
	}

	public override void RefreshProperty()
	{
		damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		durationTime = extraTime;
	}

	public override void Update()
	{
		currInterval += Time.deltaTime;
		if (currInterval >= damageInterval)
		{
			currInterval = 0f;
			BuffBeHitTodo();
		}
	}

	protected void BuffBeHitTodo()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			targetMonsterPos.x = monsterById.cachPositionX;
			targetMonsterPos.y = monsterById.cachPositionY;
			monsterRangeList.Clear();
			combatData.monsters.GetRangeRandomMonsterList(targetMonsterPos, monsterRangeList, 5f, 1, monsterById.GetUUID());
			if (monsterRangeList.Count > 0)
			{
				zbSkillSystem.Instance.CreateSubSkill((int)buffRecord.param, skillBase.hero, skillBase, skillAttrList, ((Component)monsterRangeList[0]).transform.position, Vector2.up, 0, monsterRangeList[0].GetUUID());
			}
		}
	}

	public zbBuffBeHitTodo()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		targetMonsterPos = Vector2.zero;
		monsterRangeList = new List<zbMonster>();
		base._002Ector();
	}
}
