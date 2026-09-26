using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMechaWuli01 : zbMechaBase
{
	private TrackEntry current;

	[NonSerialized]
	public float skill1ZoomRange;

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team)
	{
		base.InitData(_combatData, _team);
	}

	public override void ActionBegin()
	{
		((MonoBehaviour)this).StartCoroutine(Action());
	}

	public override IEnumerator ActionEnter()
	{
		current = anim.PlayAnimation("enter", loop: false);
		yield return null;
		((Component)this).transform.position = new Vector3(combatData.nodeRootPositonX, combatData.nodeRootPositonY + combatData.defense.GetAlertPositionY() + combatData.defense.GetAlertOffsetY() + 1f, ((Component)this).transform.position.z);
		yield return (object)new WaitForSeconds(1f / 3f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/104_enter.ogg");
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 1f / 3f);
		}
	}

	public override IEnumerator ActionSkill1()
	{
		zbAudioMgr.Instance.PlayAudio("audio/combat/yasuoqiren.ogg");
		zbMechaSkillHandleBase skill = skills[0];
		current = anim.PlayAnimation("skill1_1", loop: false);
		yield return (object)new WaitForSeconds(1f / 3f);
		((zbMechaSkill10041)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10041.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 1f / 3f);
		}
		current = anim.PlayAnimation("skill1_2", loop: false);
		float duration = current.AnimationEnd;
		float interval = skill.mechaSkillRecord.param[3];
		float elapsed = 0f;
		float tick = 0f;
		while (elapsed < duration)
		{
			float deltaTime = Time.deltaTime;
			elapsed += deltaTime;
			tick += deltaTime;
			if (tick >= interval)
			{
				zbAudioMgr.Instance.PlayAudio("audio/mecha/10041.ogg");
				Vector3 targetNormalized = zbUtil.GetTargetNormalized(goRiseEffect_1.transform.position, goRiseEffect_2.transform.position);
				for (int i = 0; (float)i < skill.mechaSkillRecord.param[4] + 1f; i++)
				{
					int number = i;
					((MonoBehaviour)this).StartCoroutine(ShootRepeater(Vector2.op_Implicit(targetNormalized), number, skill));
				}
				tick -= interval;
			}
			yield return null;
		}
		current = anim.PlayAnimation("skill1_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public IEnumerator ShootRepeater(Vector2 directionToTarget, int number, zbMechaSkillHandleBase skill)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		yield return (object)new WaitForSeconds((float)number * 0.05f);
		for (int i = 0; (float)i < skill.mechaSkillRecord.param[5] + 1f; i++)
		{
			int num = (int)Math.Ceiling((float)i / 2f);
			float num2 = spreadAngle * (float)num;
			if (i % 2 == 1)
			{
				num2 = (0f - spreadAngle) * (float)num;
			}
			Vector2 direction = Vector2.op_Implicit(Quaternion.Euler(0f, 0f, num2) * Vector2.op_Implicit(directionToTarget));
			zbMechaSkill10041_2 obj = (zbMechaSkill10041_2)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10041_2.prefab", this, goRiseEffect_1.transform.position, Vector2.up, 0, 0, skill.SkillId);
			obj.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
			obj.SetDirection(direction);
		}
	}

	public override IEnumerator ActionSkill2()
	{
		zbMechaSkillHandleBase skill = skills[1];
		current = anim.PlayAnimation("skill2", loop: false);
		((zbMechaSkill10042)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10042.prefab", this, Vector2.op_Implicit(Vector2.up * -5.5f + new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY)), Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		yield return (object)new WaitForSeconds(1.3333334f);
		zbAudioMgr.Instance.PlayAudio("audio/ui/chuchang.mp3");
		combatData.defense.AddShieldHp((int)(skill.damageRate * base.Damage));
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 5f / 6f);
		}
	}

	public override IEnumerator ActionSkill3()
	{
		zbMechaSkillHandleBase skill = skills[2];
		current = anim.PlayAnimation("skill3_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		AudioSource m_audio = zbAudioMgr.Instance.PlayAudio("audio/mecha/10043.ogg", loop: true);
		current = anim.PlayAnimation("skill3_2", loop: true);
		((zbMechaSkill10043)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10043.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		float moveSpeed = skill.mechaSkillRecord.param[2] / 100f;
		float offsetY = skill.mechaSkillRecord.param[3] / 100f;
		while (true)
		{
			((Component)this).transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, (Space)0);
			if (((Component)((Component)this).transform).transform.position.y >= combatData.defense.GetAlertPositionY() + offsetY)
			{
				break;
			}
			yield return null;
		}
		zbAudioMgr.Instance.StopAudio(m_audio);
		current = anim.PlayAnimation("skill3_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill4()
	{
		zbMechaSkillHandleBase skill = skills[3];
		current = anim.PlayAnimation("skill4_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		for (int i = 0; (float)i < skill.mechaSkillRecord.param[0]; i++)
		{
			zbAudioMgr.Instance.PlayAudio("audio/mecha/10042_1.ogg");
			current = anim.PlayAnimation("skill4_2", loop: false);
			yield return null;
			zbMonster randomMonster = combatData.monsters.GetRandomMonster();
			if ((Object)(object)randomMonster != (Object)null)
			{
				((zbMechaSkill10044)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10044.prefab", this, goRiseEffect_2.transform.position, Vector2.up, randomMonster.GetUUID(), 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
			}
			else
			{
				((zbMechaSkill10044)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10044.prefab", this, goRiseEffect_2.transform.position, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
			}
			if (current != null)
			{
				yield return (object)new WaitForSeconds(current.AnimationEnd);
			}
		}
		current = anim.PlayAnimation("skill4_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill5()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/104_skill5xuli.ogg", loop: true);
		zbMechaSkillHandleBase skill = skills[4];
		current = anim.PlayAnimation("skill5", loop: false);
		Vector3 val = ((Component)anim.skeletonAnimation).transform.InverseTransformPoint(Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY)));
		SkeletonExtensions.SetLocalPosition(boneShoot, val);
		yield return (object)new WaitForSeconds(3.3333333f);
		((zbMechaSkill10045)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10045.prefab", this, Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY)), Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		((zbMechaSkill10045_2)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10045_2.prefab", this, Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY)), Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		yield return (object)new WaitForSeconds(1.1f);
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		yield return (object)new WaitForSeconds(current.AnimationEnd - 4.4333334f);
		anim.PlayAnimation("idle", loop: true);
		if (skill.mechaSkillRecord.param[3] > current.AnimationEnd)
		{
			yield return (object)new WaitForSeconds(skill.mechaSkillRecord.param[3] - current.AnimationEnd);
		}
	}

	public override IEnumerator ActionExit()
	{
		zbAudioMgr.Instance.PlayAudio("audio/ui/rise_normal.ogg");
		current = anim.PlayAnimation("exit", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		((Component)this).transform.position = new Vector3(100f, 100f, ((Component)this).transform.position.z);
		base.mechaActiving = false;
	}
}
