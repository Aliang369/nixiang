using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMonsterSkillIndex30133 : zbMonsterSkillBase
{
	private int monsterUUID;

	private bool actioning;

	public bool rockPingzi;

	private int pingziIndex;

	private zbMonsterEffect wudiEffect;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin3", loop: true);
		int num = 0;
		monster.monsterSkillIndex30133.energyAttackNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.dazeTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.hitDamagePre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.energyAttackCd = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.everyTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.purpleTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.purpleSkillId = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.blueTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.blueTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.greenTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.bluePrecent = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.wudiTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.addAttackDamage = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30133.addMoveSpeed = monsterSkillRecord.param[num];
		num++;
		monster.monster30133Class = this;
		monster.monsterSkillIndex30133.energyAttackNumKeep = 0;
		monster.monsterSkillIndex30133.everyTimeKeep = 0f;
		monster.monsterSkillIndex30133.rockTimeKeep = 0f;
		monster.monsterSkillIndex30133.wudiTimeKeep = 0f;
		monster.monsterSkillIndex30133.energyAttackCdKeep = monster.monsterSkillIndex30133.energyAttackCd;
		rockPingzi = false;
		monsterUUID = monster.GetUUID();
	}

	public IEnumerator Trigger30133()
	{
		actioning = true;
		monster.canMoveOrAttack = false;
		monster.monsterSkillIndex30133.wudiTimeKeep = monster.monsterSkillIndex30133.wudiTime;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(2, "skin2toskin3", loop: false);
		monster.monsterSkillIndexBianShen = true;
		((MonoBehaviour)monster).StartCoroutine(Trigger30133Wudi());
		monster.monsterSkillIndexBianShen = false;
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin3", loop: true);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public IEnumerator Trigger30133Wudi()
	{
		wudiEffect = combatData.monsterEffectPoolMgr.GetMonsterSkillEffect(301313);
		wudiEffect.InitData(combatData, monsterUUID, 301313);
		wudiEffect.PlayEffectAnim("in", loop: false);
		wudiEffect.QueueEffectAnim("loop", loop: true);
		yield return (object)new WaitForSeconds(monster.monsterSkillIndex30133.wudiTime);
		wudiEffect.ReturnToPool();
	}

	public override void Update()
	{
		if (monster.monsterSkillIndex3013 != 3)
		{
			return;
		}
		monster.monsterSkillIndex30133.wudiTimeKeep -= Time.deltaTime;
		monster.monsterSkillIndex30133.energyAttackCdKeep += Time.deltaTime;
		if (monster.monster30133Class.rockPingzi && monster.monsterSkillIndex30133.energyAttackNumKeep >= monster.monsterSkillIndex30133.energyAttackNum)
		{
			monster.monsterSkillIndex30133.energyAttackCdKeep = 0f;
			((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
		}
		else
		{
			if (IsBuffStop() || actioning)
			{
				return;
			}
			if (!rockPingzi)
			{
				monster.monsterSkillIndex30133.everyTimeKeep += Time.deltaTime;
				if (!actioning && monster.monsterSkillIndex30133.everyTimeKeep >= monster.monsterSkillIndex30133.everyTime)
				{
					monster.anim.spineAnimation.PlayTackAnimation(1, "skill2", loop: true);
					rockPingzi = true;
					pingziIndex = Rand.Range(0, 3);
					monster.monsterSkillIndex30133.energyAttackNumKeep = 0;
					monster.monsterSkillIndex30133.everyTimeKeep = 0f;
					monster.monsterSkillIndex30133.rockTimeKeep = 0f;
					monster.canMoveOrAttack = false;
				}
				return;
			}
			monster.monsterSkillIndex30133.rockTimeKeep += Time.deltaTime;
			if (pingziIndex == 0)
			{
				if (!actioning && monster.monsterSkillIndex30133.rockTimeKeep >= monster.monsterSkillIndex30133.purpleTime)
				{
					rockPingzi = false;
					monster.monsterSkillIndex30133.everyTimeKeep = 0f;
					((MonoBehaviour)monster).StartCoroutine(PlaySkill2Action());
				}
			}
			else if (pingziIndex == 1)
			{
				if (!actioning && monster.monsterSkillIndex30133.rockTimeKeep >= monster.monsterSkillIndex30133.blueTime)
				{
					rockPingzi = false;
					monster.monsterSkillIndex30133.everyTimeKeep = 0f;
					((MonoBehaviour)monster).StartCoroutine(PlaySkill3Action());
				}
			}
			else if (pingziIndex == 2 && !actioning && monster.monsterSkillIndex30133.rockTimeKeep >= monster.monsterSkillIndex30133.greenTime)
			{
				rockPingzi = false;
				monster.monsterSkillIndex30133.everyTimeKeep = 0f;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill4Action());
			}
		}
	}

	private IEnumerator PlaySkill1Action()
	{
		rockPingzi = false;
		actioning = true;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(1, "skill3", loop: false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		monster.anim.spineAnimation.PlayTackAnimation(1, "skill3_1", loop: true);
		yield return (object)new WaitForSeconds(monster.monsterSkillIndex30133.dazeTime);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	private IEnumerator PlaySkill2Action()
	{
		rockPingzi = false;
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill2_2", loop: false);
		yield return (object)new WaitForSeconds(17f / 30f);
		((MonoBehaviour)monster).StartCoroutine(PlaySkill2Action_2());
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 17f / 30f);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	private IEnumerator PlaySkill2Action_2()
	{
		Vector2Int val = combatData.monsters.monsterGrid.FindBestClusterCell();
		Vector3 vector3 = new Vector3((float)((Vector2Int)(ref val)).x, (float)((Vector2Int)(ref val)).y);
		zbMonsterOnceEffect effect = combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(301312);
		effect.InitData(combatData, 301312, ((Component)monster).transform.position, "animation", _selfControl: true);
		Bone obj = ((SkeletonRenderer)effect.skeletonAnimation).skeleton.FindBone("shoot");
		Vector3 val2 = ((Component)effect.skeletonAnimation).transform.InverseTransformPoint(vector3);
		SkeletonExtensions.SetLocalPosition(obj, val2);
		TrackEntry shootTrack = effect.PlayEffectAnim("animation", loop: false);
		yield return (object)new WaitForSeconds(2f / 3f);
		zbHero zbHero2 = monster.combatData.masterTeam.heroList[0];
		zbSkillSystem.Instance.CreateSubSkill(monster.monsterSkillIndex30133.purpleSkillId, zbHero2, null, zbHero2.heroAttr.skillAttrList, vector3, Vector2.up);
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 2f / 3f);
		effect.ReturnToPool();
	}

	private IEnumerator PlaySkill3Action()
	{
		rockPingzi = false;
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill2_3", loop: false);
		yield return (object)new WaitForSeconds(17f / 30f);
		combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30133.blueTid, ((Component)monster).transform.position, ((Component)monster).transform.parent);
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 17f / 30f);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	private IEnumerator PlaySkill4Action()
	{
		rockPingzi = false;
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill2_4", loop: false);
		yield return (object)new WaitForSeconds(2f / 3f);
		monster.ChangeHp(null, null, (long)(monster.monsterSkillIndex30133.bluePrecent * (float)(long)monster.maxHp));
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 2f / 3f);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void ClearList()
	{
	}
}
