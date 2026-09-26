using System.Collections;
using Proto.Enums;
using UnityEngine;

public class zbMonsterSkillIndex30042 : zbMonsterSkillBase
{
	public bool actioning;

	private int monsterUUID;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex30042.skillIndexTake = true;
		monster.monsterIndex30042.preLostHpPercent = monsterSkillRecord.param[0];
		monster.monsterIndex30042.cureHpPercent = monsterSkillRecord.param[1];
		monster.monsterIndex30042.attrMonsterType = (int)monsterSkillRecord.param[2];
		monster.monsterIndex30042.attrPercent = monsterSkillRecord.param[3];
		monster.monsterIndex30042.damagePercent = monsterSkillRecord.param[4];
		monster.monsterIndex30042.skillBase = this;
		monster.monsterIndex30042.lastLostHp = 0L;
		actioning = false;
		monsterUUID = monster.GetUUID();
	}

	public override void CallBackLostHp(long value)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		if (actioning)
		{
			return;
		}
		monster.monsterIndex30042.lastLostHp -= value;
		if (IsBuffStop())
		{
			return;
		}
		long num = (long)(monster.monsterIndex30042.preLostHpPercent * (float)(long)monster.maxHp);
		if (monster.monsterIndex30042.lastLostHp >= num)
		{
			while (monster.monsterIndex30042.lastLostHp >= num)
			{
				monster.monsterIndex30042.lastLostHp = monster.monsterIndex30042.lastLostHp - num;
			}
			actioning = true;
			PlayTackAnimation(1, flag: true);
			float vector2Distance = zbUtil.GetVector2Distance(((Component)monster).transform.position, new Vector3(monster.initPositionX, 7.4f, ((Component)monster).transform.position.z));
			zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
			zbExtraDirection2.direction = zbUtil.GetTargetNormalized(((Component)monster).transform.position, new Vector3(monster.initPositionX, 7.4f, ((Component)monster).transform.position.z));
			zbExtraDirection2.duration = vector2Distance / 15f;
			zbExtraDirection2.speed = 15f;
			monster.canMoveOrAttack = false;
			bool fight = false;
			if (monster.IsEnterAttackRange())
			{
				fight = true;
			}
			((MonoBehaviour)monster).StartCoroutine(CleanupSequence(zbExtraDirection2.duration, fight, zbExtraDirection2));
		}
	}

	public IEnumerator CleanupSequence(float duration, bool fight, zbExtraDirection extraDirection)
	{
		yield return (object)new WaitForSeconds(23f / 30f);
		if (fight)
		{
			if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
			{
				yield break;
			}
			zbCombatSystem.Instance.combatFormula.CalcWallDamage(null, monster.combatData.defense, (int)(monster.monsterIndex30042.damagePercent * monster.GetAttrValueByType(EAttrMonsterType.Damage)), ((Component)monster).transform.position.x);
		}
		monster.AddExtraDirectionList(extraDirection);
		yield return (object)new WaitForSeconds(duration);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			monster.canMoveOrAttack = true;
			monster.anim.spineAnimation.StopAnimation(1);
			actioning = false;
			monster.ChangeHp(null, null, (long)(monster.monsterIndex30042.cureHpPercent * (float)(long)monster.maxHp));
			monster.MonsterExtraProperties[(EAttrMonsterType)monster.monsterIndex30042.attrMonsterType] += monster.monsterIndex30042.attrPercent;
		}
	}

	public override void ClearList()
	{
		monster.monsterIndex30042.skillIndexTake = false;
	}
}
