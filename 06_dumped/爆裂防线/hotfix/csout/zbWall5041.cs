using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

public class zbWall5041 : zbWallSkillBase
{
	private List<int> m_monstersInRange = new List<int>();

	private bool canFight;

	private bool enterFinish;

	private float m_damageInterval;

	private float m_damageIntervalPre;

	private float m_damageDuration;

	public int sortIndex;

	public stWallSkillIndex5041 wallSkillIndex5041Data;

	protected override void OnInited()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		m_sortType = ESortLayerType.Zhuangjiache;
		base.OnInited();
		enterFinish = false;
		canFight = false;
		m_monstersInRange.Clear();
		if (combatData.dataIndex == 2)
		{
			((Component)m_skeletonAnimation).transform.localScale = Vector3.one + 2f * Vector3.down;
		}
		else
		{
			((Component)m_skeletonAnimation).transform.localScale = Vector3.one;
		}
		ReturnToPool(9999f);
	}

	public override void SetWallAtr(float _damage, zbWallSkillCsvRecord _wallSkillRecord)
	{
		base.SetWallAtr(_damage, _wallSkillRecord);
		m_damageIntervalPre = wallSkillIndex5041Data.damageInterval;
		m_damageInterval = wallSkillIndex5041Data.damageInterval;
		m_damageDuration = 0f;
		((MonoBehaviour)this).StartCoroutine(ActionEnter());
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_monstersInRange.Clear();
	}

	protected override void Update()
	{
		base.Update();
		if (canFight)
		{
			m_damageIntervalPre += Time.deltaTime;
			m_damageDuration += Time.deltaTime;
			if (m_damageIntervalPre >= m_damageInterval)
			{
				m_damageIntervalPre -= m_damageInterval;
				UpdateMonsterCollision();
			}
			if (m_damageDuration >= (float)wallSkillIndex5041Data.duration)
			{
				m_damageDuration = 0f;
				((MonoBehaviour)this).StartCoroutine(ActionExit());
			}
		}
	}

	public IEnumerator ActionEnter()
	{
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "mogu_enter", false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.AnimationEnd);
		}
		m_skeletonAnimation.AnimationState.SetAnimation(0, "mogu_loop", true);
		enterFinish = true;
	}

	public IEnumerator ActionFight()
	{
		while (!enterFinish)
		{
			yield return null;
		}
		zbAudioMgr.Instance.PlayAudio("audio/combat/monster_die.ogg");
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "mogu_yanwu", false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.AnimationEnd);
		}
		m_skeletonAnimation.AnimationState.SetAnimation(0, "yanwu_loop", true);
	}

	public IEnumerator ActionExit()
	{
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "yanwu_exit", false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.AnimationEnd);
		}
		combatData.TriggerAction5041Index(sortIndex);
		ReturnToPool();
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			m_monstersInRange.Add(component.GetUUID());
		}
		if (!canFight)
		{
			wallSkillIndex5041Data.saveRecord[sortIndex] = -1;
			canFight = true;
			((MonoBehaviour)this).StartCoroutine(ActionFight());
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			m_monstersInRange.Remove(component.GetUUID());
		}
	}

	private void UpdateMonsterCollision()
	{
		if (!canFight)
		{
			return;
		}
		foreach (int item in m_monstersInRange)
		{
			ShootFormulaMonster(item);
		}
	}

	private void ShootFormulaMonster(int monsterUUID)
	{
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monsterById, base.damage, wallSkillRecord.atkType);
		}
	}
}
