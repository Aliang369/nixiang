using System.Collections;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30072 : zbMonsterSkillBase
{
	public stMonsterSkillIndex30072 data = new stMonsterSkillIndex30072();

	private int monsterUUID;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		data.everyTime = monsterSkillRecord.param[0];
		data.durationTime = monsterSkillRecord.param[1];
		data.damagePercent = monsterSkillRecord.param[2];
		data.cureHpPercent = monsterSkillRecord.param[3];
		data.everyTimeKeep = 0f;
		data.durationTimeKeep = 0f;
		monsterUUID = monster.GetUUID();
	}

	public override void Update()
	{
		if (!IsBuffStop())
		{
			data.everyTimeKeep += Time.deltaTime;
			if (data.everyTimeKeep >= data.everyTime)
			{
				monster.canMoveOrAttack = false;
				data.everyTimeKeep = 0f;
				data.durationTimeKeep = data.durationTime;
				((MonoBehaviour)monster).StartCoroutine(ActionCoroutine());
				((MonoBehaviour)monster).StartCoroutine(CleanupSequence());
			}
		}
	}

	public IEnumerator ActionCoroutine()
	{
		monster.monsterIndex30072DamageSta = 0f;
		TrackEntry entry = monster.anim.spineAnimation.PlayTackAnimation(1, "skill2_enter", loop: false);
		yield return (object)new WaitForSeconds(entry.AnimationEnd);
		PlayTackAnimation(1, flag: true);
		yield return (object)new WaitForSeconds(data.durationTime - entry.AnimationEnd);
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null || monster.monsterIndex30072DamageSta >= (float)(long)monster.maxHp * data.cureHpPercent)
		{
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
			yield break;
		}
		entry = monster.anim.spineAnimation.PlayTackAnimation(1, "attack", loop: false);
		if (entry != null)
		{
			monster.anim.spineAnimation.skeletonAnimation.AnimationState.AddEmptyAnimation(1, entry.AnimationEnd, entry.AnimationEnd);
		}
		monster.canMoveOrAttack = true;
		monster.combatData.monsterBulletEffectPoolMgr.GetMonsterBulletEffect(monster.monsterRecord.id).Init(monster.combatData, monster, monster.GetAttrValueByType(EAttrMonsterType.Damage) * data.damagePercent);
	}

	public IEnumerator CleanupSequence()
	{
		while (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null) && !(monster.monsterIndex30072DamageSta >= (float)(long)monster.maxHp * data.cureHpPercent))
		{
			yield return null;
		}
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
		}
	}

	public override void ClearList()
	{
	}
}
