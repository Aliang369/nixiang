using System.Collections;
using Spine;
using UnityEngine;

public class zbMechaSkill10032 : zbMechaSkillBase
{
	private Collider2D collider10032;

	private bool playExitBool;

	protected override void OnInited()
	{
		m_sortType = ESortLayerType.Zhuangjiache;
		base.OnInited();
		ignoreMonsters.Clear();
		collider10032 = ((Component)this).GetComponent<Collider2D>();
		((Behaviour)collider10032).enabled = false;
		playExitBool = false;
		((MonoBehaviour)this).StartCoroutine(ShootRepeater());
	}

	public override void SetMechaAtr(float _damage, zbMechaSkillCsvRecord _mechaSkillRecord)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.SetMechaAtr(_damage, _mechaSkillRecord);
		float num = mechaSkillRecord.param[2] / 100f;
		((Component)this).transform.localScale = new Vector3(num, num, 1f);
	}

	public IEnumerator ShootRepeater()
	{
		zbAudioMgr.Instance.PlayAudio("audio/combat/ganbingdan_fire.ogg");
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "enter", false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		((Behaviour)collider10032).enabled = true;
		ReturnToPool(mechaSkillRecord.param[1]);
		val = m_skeletonAnimation.AnimationState.SetAnimation(0, "loop", true);
		yield return (object)new WaitForSeconds(val.AnimationEnd - 0.5f);
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		zbMonster component = ((Component)collider).GetComponent<zbMonster>();
		if ((Object)(object)component != (Object)null)
		{
			if (!playExitBool)
			{
				playExitBool = true;
				PlayExit();
			}
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, component, base.damage, mechaSkillRecord.atkType);
		}
	}

	protected void PlayExit()
	{
		TrackEntry val = m_skeletonAnimation.AnimationState.SetAnimation(0, "exit", false);
		ReturnToPool(val.AnimationEnd);
	}
}
