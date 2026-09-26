using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMonsterSkillIndex30132 : zbMonsterSkillBase
{
	private int monsterUUID;

	private bool actioning;

	public bool rockPingzi;

	private int pingziIndex;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin2", loop: true);
		int num = 0;
		monster.monsterSkillIndex30132.energyAttackNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30132.dazeTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30132.hitDamagePre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30132.energyAttackCd = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30132.everyTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30132.purpleTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30132.purpleSkillId = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30132.blueTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30132.blueTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30132.hpPre30133 = monsterSkillRecord.param[num];
		num++;
		monster.monster30132Class = this;
		monster.monsterSkillIndex30132.energyAttackNumKeep = 0;
		monster.monsterSkillIndex30132.everyTimeKeep = 0f;
		monster.monsterSkillIndex30132.rockTimeKeep = 0f;
		monster.monsterSkillIndex30132.energyAttackCdKeep = monster.monsterSkillIndex30132.energyAttackCd;
		rockPingzi = false;
		monsterUUID = monster.GetUUID();
	}

	public IEnumerator Trigger30132()
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

	public override void Update()
	{
		if (monster.monsterSkillIndex3013 != 2)
		{
			return;
		}
		monster.monsterSkillIndex30132.energyAttackCdKeep += Time.deltaTime;
		if ((float)(long)monster.curHp / (float)(long)monster.maxHp < monster.monsterSkillIndex30132.hpPre30133 && !actioning)
		{
			monster.monsterSkillIndex3013 = 3;
			((MonoBehaviour)monster).StartCoroutine(monster.monster30133Class.Trigger30133());
		}
		else if (monster.monster30132Class.rockPingzi && monster.monsterSkillIndex30132.energyAttackNumKeep >= monster.monsterSkillIndex30132.energyAttackNum)
		{
			monster.monsterSkillIndex30132.energyAttackCdKeep = 0f;
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
				monster.monsterSkillIndex30132.everyTimeKeep += Time.deltaTime;
				if (!actioning && monster.monsterSkillIndex30132.everyTimeKeep >= monster.monsterSkillIndex30132.everyTime)
				{
					monster.anim.spineAnimation.PlayTackAnimation(1, "skill2", loop: true);
					rockPingzi = true;
					pingziIndex = Rand.Range(0, 2);
					monster.monsterSkillIndex30132.energyAttackNumKeep = 0;
					monster.monsterSkillIndex30132.everyTimeKeep = 0f;
					monster.monsterSkillIndex30132.rockTimeKeep = 0f;
					monster.canMoveOrAttack = false;
				}
				return;
			}
			monster.monsterSkillIndex30132.rockTimeKeep += Time.deltaTime;
			if (pingziIndex == 0)
			{
				if (!actioning && monster.monsterSkillIndex30132.rockTimeKeep >= monster.monsterSkillIndex30132.purpleTime)
				{
					rockPingzi = false;
					monster.monsterSkillIndex30132.everyTimeKeep = 0f;
					((MonoBehaviour)monster).StartCoroutine(PlaySkill2Action());
				}
			}
			else if (!actioning && monster.monsterSkillIndex30132.rockTimeKeep >= monster.monsterSkillIndex30132.blueTime)
			{
				rockPingzi = false;
				monster.monsterSkillIndex30132.everyTimeKeep = 0f;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill3Action());
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
		yield return (object)new WaitForSeconds(monster.monsterSkillIndex30132.dazeTime);
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
		zbSkillSystem.Instance.CreateSubSkill(monster.monsterSkillIndex30132.purpleSkillId, zbHero2, null, zbHero2.heroAttr.skillAttrList, vector3, Vector2.up);
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 2f / 3f);
		effect.ReturnToPool();
	}

	private IEnumerator PlaySkill3Action()
	{
		rockPingzi = false;
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill2_3", loop: false);
		yield return (object)new WaitForSeconds(17f / 30f);
		combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30132.blueTid, ((Component)monster).transform.position, ((Component)monster).transform.parent);
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 17f / 30f);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void ClearList()
	{
	}
}
