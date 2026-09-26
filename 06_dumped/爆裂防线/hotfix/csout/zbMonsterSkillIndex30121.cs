using System;
using System.Collections;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30121 : zbMonsterSkillBase
{
	private int monsterUUID;

	public bool actioning;

	public override void RefreshProperty()
	{
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		base.RefreshProperty();
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin1", loop: true);
		int num = 0;
		monster.monsterSkillIndex30121.everyTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30121.monsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30121.monsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30121.summonPos = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30121.monsterRedTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30121.redCd = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30121.monsterGreenTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30121.greenCd = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30121.stopDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30121.hpPre30122 = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30121.skillBornDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30121.summonPos += monster.combatData.defense.GetAlertPositionY();
		monster.monsterSkillIndex30121.stopDistance += monster.combatData.defense.GetAlertPositionY();
		monster.monsterSkillIndex3012 = 1;
		monster.monsterSkillIndexClass = this;
		monster.monster30121Class = this;
		monster.monsterSkillIndex30121.everyTimeKeep = 0f;
		monster.monsterSkillIndex30121.redCdKeep = 0f;
		monster.monsterSkillIndex30121.greenCdKeep = 0f;
		monsterUUID = monster.GetUUID();
		Vector3 position = default(Vector3);
		((Vector3)(ref position))._002Ector(((Component)monster).gameObject.transform.position.x, monster.monsterSkillIndex30121.skillBornDistance + combatData.defense.GetAlertPositionY(), ((Component)monster).gameObject.transform.position.z);
		((Component)monster).gameObject.transform.position = position;
	}

	public override void TriggerDieSkill(Action callback)
	{
		((MonoBehaviour)monster).StartCoroutine(TriggerDieSkillIEnumerator(callback));
	}

	public IEnumerator TriggerDieSkillIEnumerator(Action callback)
	{
		monster.monsterSkillIndex3012 = 0;
		for (int i = 0; i < monster.monster30123Class.checkMonsterUUID.Count; i++)
		{
			zbMonster monsterById = combatData.monsters.GetMonsterById(monster.monster30123Class.checkMonsterUUID[i]);
			if ((Object)(object)monsterById != (Object)null)
			{
				monsterById.canMoveOrAttack = true;
			}
		}
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(1, "skill5", loop: false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		callback?.Invoke();
	}

	public override void Save(stMonsterBoss monsterBoss)
	{
		monsterBoss.monsterSkillIndex301x = monster.monsterSkillIndex3012;
		monsterBoss.trigger30123Num = monster.monsterSkillIndex30122.trigger30123Num;
		monsterBoss.usedTime = monster.monsterSkillIndex30123.usedTime;
	}

	public override void RestoreData(stMonsterBoss monsterBoss)
	{
		monster.monsterSkillIndex3012 = monsterBoss.monsterSkillIndex301x;
		monster.monsterSkillIndex30122.trigger30123Num = monsterBoss.trigger30123Num;
		monster.monsterSkillIndex30123.usedTime = monsterBoss.usedTime;
		if (monster.monsterSkillIndex3012 >= 0 && monster.monsterSkillIndex3012 <= 3)
		{
			monster.anim.spineAnimation.PlayTackAnimation(2, "skin" + monster.monsterSkillIndex3012, loop: true);
		}
		if (monster.monsterSkillIndex3012 == 3)
		{
			((MonoBehaviour)monster).StartCoroutine(monster.monster30123Class.Trigger30123(monster.monsterSkillIndex30123.usedTime));
		}
	}

	public override void Update()
	{
		if (monster.monsterSkillIndex3012 == 1 && (float)(long)monster.curHp / (float)(long)monster.maxHp < monster.monsterSkillIndex30121.hpPre30122 && !actioning)
		{
			monster.monsterSkillIndex3012 = 2;
			((MonoBehaviour)monster).StartCoroutine(monster.monster30122Class.Trigger30122());
		}
		else if (!IsBuffStop() && monster.monsterSkillIndex3012 == 1 && !actioning)
		{
			monster.monsterSkillIndex30121.everyTimeKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30121.everyTimeKeep >= monster.monsterSkillIndex30121.everyTime)
			{
				monster.monsterSkillIndex30121.everyTimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
			}
			monster.monsterSkillIndex30121.redCdKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30121.redCdKeep >= monster.monsterSkillIndex30121.redCd)
			{
				monster.monsterSkillIndex30121.redCdKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill2Action());
			}
			monster.monsterSkillIndex30121.greenCdKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30121.greenCdKeep >= monster.monsterSkillIndex30121.greenCd)
			{
				monster.monsterSkillIndex30121.greenCdKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill3Action());
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
			int num = 1;
			for (int i = 0; i < monster.monsterSkillIndex30121.monsterNum; i++)
			{
				((MonoBehaviour)monster).StartCoroutine(SpawnMonsterMonster(Vector3.left * (float)num + Vector3.down + ((Component)monster).transform.position));
				num = -num;
			}
			yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 17f / 30f);
			actioning = false;
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
		}
	}

	private IEnumerator SpawnMonsterMonster(Vector3 vector3)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		zbMonsterOnceEffect effect = combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(301211);
		effect.InitData(combatData, 301211, vector3, "in", _selfControl: true);
		TrackEntry val = effect.PlayEffectAnim("in", loop: false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		effect.PlayEffectAnim("loop", loop: true);
		combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30121.monsterTid, vector3, ((Component)monster).transform.parent);
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		effect.Free();
	}

	private IEnumerator PlaySkill2Action()
	{
		actioning = true;
		monster.anim.spineAnimation.PlayTackAnimation(1, "skill2", loop: false);
		yield return (object)new WaitForSeconds(1.0666667f);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			Vector3 vector3 = new Vector3(-2f, monster.monsterSkillIndex30121.summonPos, 0f);
			combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(301212).InitData(combatData, 301212, vector3, "in2");
			yield return (object)new WaitForSeconds(1.3333334f);
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30121.monsterRedTid, vector3, ((Component)monster).transform.parent);
			actioning = false;
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
		}
	}

	private IEnumerator PlaySkill3Action()
	{
		actioning = true;
		monster.anim.spineAnimation.PlayTackAnimation(1, "skill2", loop: false);
		yield return (object)new WaitForSeconds(1.0666667f);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			Vector3 vector3 = new Vector3(2f, monster.monsterSkillIndex30121.summonPos, 0f);
			combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(301212).InitData(combatData, 301212, vector3, "in");
			yield return (object)new WaitForSeconds(1.3333334f);
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30121.monsterGreenTid, vector3, ((Component)monster).transform.parent);
			actioning = false;
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
		}
	}

	public override void ClearList()
	{
	}
}
