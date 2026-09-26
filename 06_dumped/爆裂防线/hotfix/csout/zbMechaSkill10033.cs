using System;
using System.Collections;
using Spine;
using UnityEngine;

public class zbMechaSkill10033 : zbMechaSkillBase
{
	private Collider2D collider10033;

	private zbBoundsCircle2D boundsCircle2D;

	[NonSerialized]
	public GameObject goRiseEffect_1;

	protected override void OnInited()
	{
		base.OnInited();
		goRiseEffect_1 = ((Component)((Component)this).transform.Find("goRiseEffect_1")).gameObject;
		boundsCircle2D.center.x = 0f;
		boundsCircle2D.center.y = 0f;
		ignoreMonsters.Clear();
		collider10033 = ((Component)this).GetComponent<Collider2D>();
		((Behaviour)collider10033).enabled = false;
		((MonoBehaviour)this).StartCoroutine(ShootRepeater());
		((MonoBehaviour)this).StartCoroutine(PlayAudio());
	}

	public IEnumerator ShootRepeater()
	{
		TrackEntry current = m_skeletonAnimation.AnimationState.SetAnimation(0, "enter", false);
		yield return (object)new WaitForSeconds(2.7f);
		((Behaviour)collider10033).enabled = true;
		m_sortType = ESortLayerType.Background;
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		yield return (object)new WaitForSeconds(current.AnimationEnd - 0.5f);
		m_skeletonAnimation.AnimationState.SetAnimation(0, "loop", true);
		yield return (object)new WaitForSeconds(mechaSkillRecord.param[0]);
		m_skeletonAnimation.AnimationState.SetAnimation(0, "exit", false);
		ReturnToPool(current.AnimationEnd);
	}

	public IEnumerator PlayAudio()
	{
		yield return (object)new WaitForSeconds(1.2f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		UpdateWallCollision();
		yield return (object)new WaitForSeconds(0.39999998f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		UpdateWallCollision();
		yield return (object)new WaitForSeconds(0.26666665f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		UpdateWallCollision();
		yield return (object)new WaitForSeconds(0.30000007f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		UpdateWallCollision();
		yield return (object)new WaitForSeconds(0.6666665f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		UpdateWallCollision();
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			component.AddSlowDownPercent(mechaSkillRecord.param[1]);
			if ((double)mechaSkillRecord.param[2] > 0.01)
			{
				component.addPercentMass *= mechaSkillRecord.param[2] / 2f;
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			component.DecSlowDownPercent(mechaSkillRecord.param[1]);
			if ((double)mechaSkillRecord.param[2] > 0.01)
			{
				component.addPercentMass /= mechaSkillRecord.param[2] / 2f;
			}
		}
	}

	protected void UpdateWallCollision()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		boundsCircle2D.radius = goRiseEffect_1.transform.position.y;
		combatData.monsters.monsterGrid.QueryList(in boundsCircle2D, ignoreMonsters, overlapResultList);
		foreach (zbMonster overlapResult in overlapResultList)
		{
			ignoreMonsters.Add(overlapResult.GetUUID());
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, overlapResult, base.damage, mechaSkillRecord.atkType);
		}
	}
}
