using Spine;
using UnityEngine;

public class zbMechaSkill10044_2 : zbMechaSkillBase
{
	protected override void OnInited()
	{
		base.OnInited();
		ignoreMonsters.Clear();
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		if (val != null)
		{
			ReturnToPool(val.AnimationEnd);
		}
		else
		{
			ReturnToPool(1f);
		}
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		float num = mechaSkillRecord.param[1] / 100f;
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
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
