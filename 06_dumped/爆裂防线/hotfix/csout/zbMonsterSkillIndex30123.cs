using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMonsterSkillIndex30123 : zbMonsterSkillBase
{
	private int monsterUUID;

	public bool actioning;

	public List<int> checkMonsterUUID = new List<int>();

	private HashSet<int> ignoreList = new HashSet<int>();

	private List<zbMonsterEffect> effectList = new List<zbMonsterEffect>();

	private List<Bone> gunPointList = new List<Bone>();

	private List<Bone> xixuePointList = new List<Bone>();

	private float m_refreshTimer;

	private static readonly float m_refreshInterval = 1f;

	private Bone xixuePoint;

	private bool fasheXixue;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		int num = 0;
		monster.monsterSkillIndex30123.monsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30123.duration = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30123.addHpPre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30123.everyTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30123.monsterSkillNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30123.monsterSkillTid1 = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30123.monsterSkillTid2 = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30123.monsterSkillTid3 = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30123.zengshangPre = monsterSkillRecord.param[num];
		num++;
		fasheXixue = false;
		monster.monsterSkillIndex30123.everyTimeKeep = 0f;
		monster.monsterSkillIndex30123.usedTime = 0f;
		monster.monster30123Class = this;
		monsterUUID = monster.GetUUID();
		monster.monsterSkillIndex30123.immBuffs.Clear();
		monster.monsterSkillIndex30123.immBuffs.Add(2);
		monster.monsterSkillIndex30123.immBuffs.Add(3);
		monster.monsterSkillIndex30123.immBuffs.Add(4);
		xixuePoint = ((SkeletonRenderer)monster.anim.spineAnimation.skeletonAnimation).skeleton.FindBone("xixuePoint");
	}

	public IEnumerator Trigger30123(float usedTime = 0f)
	{
		fasheXixue = false;
		actioning = true;
		monster.canMoveOrAttack = false;
		monster.monsterSkillIndex30123.usedTime = usedTime;
		m_refreshTimer = 0f;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(2, "skin2toskin3", loop: false);
		monster.monsterSkillIndexBianShen = true;
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin3", loop: true);
		monster.anim.spineAnimation.PlayTackAnimation(1, "skill4", loop: true);
		ignoreList.Clear();
		combatData.monsters.GetRandomMonsters(monster.monsterSkillIndex30123.monsterNum, checkMonsterUUID, ignoreList);
		while (checkMonsterUUID.Count < monster.monsterSkillIndex30123.monsterNum)
		{
			checkMonsterUUID.Add(0);
		}
		effectList.Clear();
		gunPointList.Clear();
		xixuePointList.Clear();
		for (int i = 0; i < monster.monsterSkillIndex30123.monsterNum; i++)
		{
			zbMonsterEffect monsterSkillEffect = combatData.monsterEffectPoolMgr.GetMonsterSkillEffect(301213);
			monsterSkillEffect.InitData(combatData, monster.GetUUID(), 301213);
			if ((Object)(object)monsterSkillEffect != (Object)null)
			{
				monsterSkillEffect.PlayEffectAnim("loop", loop: false);
				effectList.Add(monsterSkillEffect);
				gunPointList.Add(((SkeletonRenderer)monsterSkillEffect.skeletonAnimation).skeleton.FindBone("shoot"));
				Bone obj = ((SkeletonRenderer)monsterSkillEffect.skeletonAnimation).skeleton.FindBone("xixuePoint");
				Vector3 worldPosition = SkeletonExtensions.GetWorldPosition(xixuePoint, ((Component)monster.anim.spineAnimation.skeletonAnimation).transform);
				Vector3 val2 = ((Component)monsterSkillEffect.skeletonAnimation).transform.InverseTransformPoint(worldPosition);
				SkeletonExtensions.SetLocalPosition(obj, val2);
			}
		}
		fasheXixue = true;
		foreach (int item in checkMonsterUUID)
		{
			ignoreList.Add(item);
		}
		yield return (object)new WaitForSeconds(monster.monsterSkillIndex30123.duration - monster.monsterSkillIndex30123.usedTime);
		Trigger30123_30122();
	}

	protected void Trigger30123_30122()
	{
		foreach (zbMonsterEffect effect in effectList)
		{
			effect.ReturnToPool();
		}
		for (int i = 0; i < checkMonsterUUID.Count; i++)
		{
			zbMonster monsterById = combatData.monsters.GetMonsterById(checkMonsterUUID[i]);
			if ((Object)(object)monsterById != (Object)null)
			{
				monsterById.canMoveOrAttack = true;
			}
		}
		monster.monsterSkillIndex3012 = 2;
		((MonoBehaviour)monster).StartCoroutine(monster.monster30122Class.Trigger30123_30122());
	}

	public override void Update()
	{
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		if (monster.monsterSkillIndex3012 != 3 || !fasheXixue)
		{
			return;
		}
		monster.monsterSkillIndex30123.usedTime += Time.deltaTime;
		for (int i = 0; i < checkMonsterUUID.Count; i++)
		{
			zbMonster zbMonster2 = combatData.monsters.GetMonsterById(checkMonsterUUID[i]);
			if ((Object)(object)zbMonster2 == (Object)null)
			{
				zbMonster2 = combatData.monsters.GetRandomMonster(ignoreList);
				if ((Object)(object)zbMonster2 != (Object)null)
				{
					checkMonsterUUID[i] = zbMonster2.GetUUID();
					ignoreList.Add(zbMonster2.GetUUID());
				}
			}
			if ((Object)(object)zbMonster2 != (Object)null)
			{
				zbUtil.SetActive(((Component)((Component)effectList[i]).transform).gameObject, active: true);
				Vector3 val = ((Component)effectList[i].skeletonAnimation).transform.InverseTransformPoint(zbMonster2.GetCurPosition());
				SkeletonExtensions.SetLocalPosition(gunPointList[i], val);
			}
			else
			{
				zbUtil.SetActive(((Component)((Component)effectList[i]).transform).gameObject, active: false);
				SkeletonExtensions.SetLocalPosition(gunPointList[i], new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY));
			}
		}
		monster.monsterSkillIndex30123.everyTimeKeep += Time.deltaTime;
		if (!actioning && monster.monsterSkillIndex30123.everyTimeKeep >= monster.monsterSkillIndex30123.everyTime)
		{
			monster.monsterSkillIndex30123.everyTimeKeep = 0f;
			PlaySkill1Action();
		}
		m_refreshTimer += Time.unscaledDeltaTime;
		if (m_refreshTimer >= m_refreshInterval)
		{
			m_refreshTimer = 0f;
			UpdatePerSecond();
		}
	}

	protected virtual void UpdatePerSecond()
	{
		for (int i = 0; i < checkMonsterUUID.Count; i++)
		{
			if ((Object)(object)combatData.monsters.GetMonsterById(checkMonsterUUID[i]) != (Object)null)
			{
				monster.ChangeHp(null, null, (long)(monster.monsterSkillIndex30123.addHpPre * (float)(long)monster.maxHp));
			}
		}
	}

	private void PlaySkill1Action()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		actioning = true;
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			int num = 1;
			for (int i = 0; i < monster.monsterSkillIndex30123.monsterSkillNum; i++)
			{
				((MonoBehaviour)monster).StartCoroutine(SpawnMonsterMonsterSkill1(Vector3.left * monster.monsterSkillIndex30122.summonSkill4Pos * (float)num + ((Component)monster).transform.position));
				num = -num;
			}
			actioning = false;
		}
	}

	private IEnumerator SpawnMonsterMonsterSkill1(Vector3 vector3)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(301111).InitData(combatData, 301111, vector3, "in");
		yield return (object)new WaitForSeconds(1f);
		combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30123.monsterSkillTid1, vector3, ((Component)monster).transform.parent);
		combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30123.monsterSkillTid2, vector3, ((Component)monster).transform.parent);
		combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30123.monsterSkillTid3, vector3, ((Component)monster).transform.parent);
	}

	public override void ClearList()
	{
	}
}
