using System;
using Spine.Unity;
using UnityEngine;

public class zbMechaSkill10011 : zbMechaSkillBase
{
	[NonSerialized]
	public float bulletSpeed = 16f;

	protected override void OnInited()
	{
		base.OnInited();
		ignoreMonsters.Clear();
		ReturnToPool(lifetime);
		if (zbCombatSystem.Instance.combatFPS.IsLowPerformanceMode())
		{
			m_skeletonAnimation.UpdateTiming = (UpdateTiming)0;
		}
		else
		{
			m_skeletonAnimation.UpdateTiming = (UpdateTiming)1;
		}
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		float num = mechaSkillRecord.param[1] / 100f;
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
		bulletSpeed = mechaSkillRecord.param[2] / 100f;
	}

	protected override void Update()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		((Component)this).transform.Translate(m_direction * bulletSpeed * Time.deltaTime, (Space)0);
		if ((int)m_skeletonAnimation.UpdateTiming == 0 && Time.frameCount % zbCombatFPS.MANUAL_UPDATE_FRAME == 0)
		{
			m_skeletonAnimation.Update(Time.deltaTime * (float)zbCombatFPS.MANUAL_UPDATE_FRAME);
		}
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
