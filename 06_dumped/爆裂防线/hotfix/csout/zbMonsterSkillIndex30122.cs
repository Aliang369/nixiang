using System.Collections;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30122 : zbMonsterSkillBase
{
	private int monsterUUID;

	public bool actioning;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin1", loop: true);
		int num = 0;
		monster.monsterSkillIndex30122.everyTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.monsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.monsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.summonPos = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30122.monsterRedTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.redCd = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.monsterGreenTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.greenCd = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.skill4Time = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.summonSkill4Pos = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.monsterSkill4Num = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.monsterSkill4Tid1 = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.monsterSkill4Tid2 = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.monsterDieTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.hpPre30123_1 = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.hpPre30123_2 = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.hpPre30123_3 = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30122.summonPos += monster.combatData.defense.GetAlertPositionY();
		monster.monster30122Class = this;
		monster.monsterSkillIndex30122.everyTimeKeep = 0f;
		monster.monsterSkillIndex30122.redCdKeep = 0f;
		monster.monsterSkillIndex30122.greenCdKeep = 0f;
		monster.monsterSkillIndex30122.skill4TimeKeep = 0f;
		monster.monsterSkillIndex30122.trigger30123Num = 0;
		monsterUUID = monster.GetUUID();
	}

	public IEnumerator Trigger30122()
	{
		actioning = true;
		monster.canMoveOrAttack = false;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(2, "skin1toskin2", loop: false);
		monster.monsterSkillIndexBianShen = true;
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		monster.monsterSkillIndexBianShen = false;
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin2", loop: true);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public IEnumerator Trigger30123_30122()
	{
		monster.anim.spineAnimation.StopAnimation(1);
		actioning = true;
		monster.canMoveOrAttack = false;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(2, "skin3toskin2", loop: false);
		monster.monsterSkillIndexBianShen = true;
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		monster.monsterSkillIndexBianShen = false;
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin2", loop: true);
		actioning = false;
		monster.canMoveOrAttack = true;
	}

	public override void Update()
	{
		if (monster.monsterSkillIndex3012 == 2 && monster.monsterSkillIndex30122.trigger30123Num < 3 && !actioning)
		{
			float num = 0f;
			if (monster.monsterSkillIndex30122.trigger30123Num == 0)
			{
				num = monster.monsterSkillIndex30122.hpPre30123_1;
			}
			else if (monster.monsterSkillIndex30122.trigger30123Num == 1)
			{
				num = monster.monsterSkillIndex30122.hpPre30123_2;
			}
			else if (monster.monsterSkillIndex30122.trigger30123Num == 2)
			{
				num = monster.monsterSkillIndex30122.hpPre30123_3;
			}
			if ((float)(long)monster.curHp / (float)(long)monster.maxHp < num)
			{
				monster.monsterSkillIndex3012 = 3;
				stMonsterSkillIndex30122 monsterSkillIndex = monster.monsterSkillIndex30122;
				int trigger30123Num = monsterSkillIndex.trigger30123Num + 1;
				monsterSkillIndex.trigger30123Num = trigger30123Num;
				((MonoBehaviour)monster).StartCoroutine(monster.monster30123Class.Trigger30123());
				return;
			}
		}
		if (!IsBuffStop() && monster.monsterSkillIndex3012 == 2 && !actioning)
		{
			monster.monsterSkillIndex30122.everyTimeKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30122.everyTimeKeep >= monster.monsterSkillIndex30122.everyTime)
			{
				monster.monsterSkillIndex30122.everyTimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
			}
			monster.monsterSkillIndex30122.redCdKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30122.redCdKeep >= monster.monsterSkillIndex30122.redCd)
			{
				monster.monsterSkillIndex30122.redCdKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill2Action());
			}
			monster.monsterSkillIndex30122.greenCdKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30122.greenCdKeep >= monster.monsterSkillIndex30122.greenCd)
			{
				monster.monsterSkillIndex30122.greenCdKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill3Action());
			}
			monster.monsterSkillIndex30122.skill4TimeKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30122.skill4TimeKeep >= monster.monsterSkillIndex30122.skill4Time)
			{
				monster.monsterSkillIndex30122.skill4TimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill4Action());
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
			for (int i = 0; i < monster.monsterSkillIndex30122.monsterNum; i++)
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
		combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(301211).InitData(combatData, 301211, vector3, "in");
		yield return (object)new WaitForSeconds(1.1666666f);
		combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30122.monsterTid, vector3, ((Component)monster).transform.parent);
	}

	private IEnumerator PlaySkill2Action()
	{
		actioning = true;
		monster.anim.spineAnimation.PlayTackAnimation(1, "skill2", loop: false);
		yield return (object)new WaitForSeconds(1.0666667f);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			Vector3 vector3 = new Vector3(-2f, monster.monsterSkillIndex30122.summonPos, 0f);
			combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(301212).InitData(combatData, 301212, vector3, "in2");
			yield return (object)new WaitForSeconds(1.3333334f);
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30122.monsterRedTid, vector3, ((Component)monster).transform.parent);
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
			Vector3 vector3 = new Vector3(2f, monster.monsterSkillIndex30122.summonPos, 0f);
			combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(301212).InitData(combatData, 301212, vector3, "in");
			yield return (object)new WaitForSeconds(1.3333334f);
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30122.monsterGreenTid, vector3, ((Component)monster).transform.parent);
			actioning = false;
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
		}
	}

	private IEnumerator PlaySkill4Action()
	{
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill3", loop: false);
		yield return (object)new WaitForSeconds(1.3333334f);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			int num = 1;
			for (int i = 0; i < monster.monsterSkillIndex30122.monsterSkill4Num; i++)
			{
				((MonoBehaviour)monster).StartCoroutine(SpawnMonsterMonsterSkill4(Vector3.left * monster.monsterSkillIndex30122.summonSkill4Pos * (float)num + ((Component)monster).transform.position));
				num = -num;
			}
			yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 2f / 3f);
			actioning = false;
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
		}
	}

	private IEnumerator SpawnMonsterMonsterSkill4(Vector3 vector3)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(301111).InitData(combatData, 301111, vector3, "in");
		yield return (object)new WaitForSeconds(1f);
		combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30122.monsterSkill4Tid1, vector3, ((Component)monster).transform.parent);
		combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30122.monsterSkill4Tid2, vector3, ((Component)monster).transform.parent);
	}

	public override void ClearList()
	{
	}
}
