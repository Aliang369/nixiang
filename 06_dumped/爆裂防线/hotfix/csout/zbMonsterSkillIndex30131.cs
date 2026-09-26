using System;
using System.Collections;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30131 : zbMonsterSkillBase
{
	private int monsterUUID;

	public bool actioning;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin1", loop: true);
		int num = 0;
		monster.monsterSkillIndex30131.everyTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30131.rangeDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30131.monsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30131.monsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30131.hpPre30132 = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex3013 = 1;
		monster.monsterSkillIndexClass = this;
		monster.monster30131Class = this;
		monster.monsterSkillIndex30131.everyTimeKeep = 0f;
		monsterUUID = monster.GetUUID();
		monster.skillMgr.CreateSkill(monster, 30134);
	}

	public override void TriggerDieSkill(Action callback)
	{
		((MonoBehaviour)monster).StartCoroutine(TriggerDieSkillIEnumerator(callback));
	}

	public IEnumerator TriggerDieSkillIEnumerator(Action callback)
	{
		monster.monsterSkillIndex3013 = 0;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(1, "skill4", loop: false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		callback?.Invoke();
	}

	public override void Save(stMonsterBoss monsterBoss)
	{
		monsterBoss.monsterSkillIndex301x = monster.monsterSkillIndex3013;
	}

	public override void RestoreData(stMonsterBoss monsterBoss)
	{
		monster.monsterSkillIndex3013 = monsterBoss.monsterSkillIndex301x;
		if (monster.monsterSkillIndex3013 >= 0 && monster.monsterSkillIndex3013 <= 3)
		{
			monster.anim.spineAnimation.PlayTackAnimation(2, "skin" + monster.monsterSkillIndex3013, loop: true);
		}
	}

	public override void Update()
	{
		if (monster.monsterSkillIndex3013 != 1)
		{
			return;
		}
		if ((float)(long)monster.curHp / (float)(long)monster.maxHp < monster.monsterSkillIndex30131.hpPre30132 && !actioning)
		{
			monster.monsterSkillIndex3013 = 2;
			((MonoBehaviour)monster).StartCoroutine(monster.monster30132Class.Trigger30132());
		}
		else if (!IsBuffStop() && !actioning)
		{
			monster.monsterSkillIndex30131.everyTimeKeep += Time.deltaTime;
			if (monster.monsterSkillIndex30131.everyTimeKeep >= monster.monsterSkillIndex30131.everyTime)
			{
				monster.monsterSkillIndex30131.everyTimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
			}
		}
	}

	private IEnumerator PlaySkill1Action()
	{
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill1", loop: false);
		yield return (object)new WaitForSeconds(17f / 30f);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			((MonoBehaviour)monster).StartCoroutine(SpawnMonsterMonster());
			yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 17f / 30f);
			actioning = false;
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
		}
	}

	private IEnumerator SpawnMonsterMonster()
	{
		Vector3 vector3 = Vector3.left * (float)((Rand.Range(0, 2) != 0) ? 1 : (-1)) + ((Component)monster).transform.position;
		zbMonsterOnceEffect effect = combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(301311);
		effect.InitData(combatData, 301311, vector3, "animation", _selfControl: true);
		TrackEntry shootTrack = effect.PlayEffectAnim("animation", loop: false);
		for (int i = 0; i < monster.monsterSkillIndex30131.monsterNum; i++)
		{
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30131.monsterTid, vector3, ((Component)monster).transform.parent);
			yield return null;
		}
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd);
		effect.Free();
	}

	public override void ClearList()
	{
	}
}
