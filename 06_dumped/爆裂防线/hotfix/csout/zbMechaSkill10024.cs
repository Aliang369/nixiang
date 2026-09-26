using System.Collections;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbMechaSkill10024 : zbMechaSkillBase
{
	private zbMechaHuo01 mechaHuo01;

	private float zoomPreSec;

	protected override void OnInited()
	{
		base.OnInited();
		m_sortType = ESortLayerType.BurnLayer;
		zbSortMgr.Instance.InitSortOrder(m_sortType, ((Component)this).gameObject);
		((MonoBehaviour)this).StartCoroutine(ActionZoom());
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		mechaHuo01 = (zbMechaHuo01)hero;
		float num = mechaHuo01.skills[0].mechaSkillRecord.param[1] / 100f;
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
		zoomPreSec = (_mechaSkillRecord.param[1] / 100f - 3.5f) / _mechaSkillRecord.param[0];
		skillAttrList[EAttrSkillType.BurnDamage] = base.damage * (float)hero.team.battleTeamInfo.RoleBaseInfo.TeamAttack;
	}

	public IEnumerator ActionZoom()
	{
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "enter", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		m_skeletonAnimation.AnimationState.SetAnimation(0, "loop", true);
		mechaHuo01.skill1CanZoom = true;
		while (true)
		{
			yield return null;
			float num = ((Component)this).transform.localScale.x + zoomPreSec * Time.deltaTime;
			((Component)this).transform.localScale = new Vector3(num, num, 1f);
			if (num >= mechaSkillRecord.param[1] / 100f)
			{
				break;
			}
			mechaHuo01.skill1ZoomRange = num;
		}
		ReturnToPool();
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, component, base.damage, mechaSkillRecord.atkType);
		}
	}
}
