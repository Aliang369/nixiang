using System;
using System.Collections;
using Spine;
using UnityEngine;

public class zbMechaDian01 : zbMechaBase
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
		zbAudioMgr.Instance.PlayAudio("audio/mecha/103_enter.ogg");
		yield return null;
		((Component)this).transform.position = new Vector3(combatData.nodeRootPositonX, combatData.nodeRootPositonY, ((Component)this).transform.position.z);
		yield return (object)new WaitForSeconds(1.1666666f);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 1.1666666f);
		}
	}

	public override IEnumerator ActionSkill1()
	{
		Vector2[] sixDirections = (Vector2[])(object)new Vector2[6]
		{
			new Vector2(0.5f, 0.866f),
			new Vector2(1f, 0f),
			new Vector2(0.5f, -0.866f),
			new Vector2(-0.5f, -0.866f),
			new Vector2(-1f, 0f),
			new Vector2(-0.5f, 0.866f)
		};
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		zbMechaSkillHandleBase skill = skills[0];
		current = anim.PlayAnimation("skill1_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		int count = (int)skill.mechaSkillRecord.param[0];
		for (int i = 0; i < count; i++)
		{
			zbAudioMgr.Instance.PlayAudio("audio/mecha/103_1.ogg");
			current = anim.PlayAnimation("skill1_2", loop: false);
			for (int j = 0; j < 6; j++)
			{
				zbMechaSkill10011 obj = (zbMechaSkill10011)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10011.prefab", this, goRiseEffect_2.transform.position, Vector2.up, 0, 0, skill.SkillId);
				obj.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
				obj.SetDirection(((Vector2)(ref sixDirections[j])).normalized);
				zbMechaSkill10011 obj2 = (zbMechaSkill10011)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10011.prefab", this, goRiseEffect_3.transform.position, Vector2.up, 0, 0, skill.SkillId);
				obj2.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
				obj2.SetDirection(((Vector2)(ref sixDirections[j])).normalized);
			}
			if (current != null)
			{
				yield return (object)new WaitForSeconds(current.AnimationEnd);
			}
		}
		current = anim.PlayAnimation("skill1_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill2()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		zbMechaSkillHandleBase skill = skills[1];
		current = anim.PlayAnimation("skill2_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/103_2.ogg", loop: true);
		for (int i = 0; (float)i < skill.mechaSkillRecord.param[0]; i++)
		{
			current = anim.PlayAnimation("skill2_2", loop: false);
			zbMonster randomMonster = combatData.monsters.GetRandomMonster();
			if ((Object)(object)randomMonster != (Object)null)
			{
				((zbMechaSkill10012)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10012.prefab", this, ((Component)randomMonster).transform.position, Vector2.up, randomMonster.GetUUID(), 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
			}
			yield return (object)new WaitForSeconds(skill.mechaSkillRecord.param[2]);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		current = anim.PlayAnimation("skill2_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill3()
	{
		zbMechaSkillHandleBase skill = skills[2];
		current = anim.PlayAnimation("skill3_1", loop: false);
		yield return (object)new WaitForSeconds(1f / 3f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/103_1.ogg");
		((Component)this).transform.position = new Vector3(combatData.nodeRootPositonX, combatData.nodeRootPositonY + combatData.defense.GetAlertPositionY() + combatData.defense.GetAlertOffsetY() + 1f, ((Component)this).transform.position.z);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 1f / 3f);
		}
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/103_3.ogg", loop: true);
		current = anim.PlayAnimation("skill3_2", loop: true);
		for (int i = 0; (float)i < skill.mechaSkillRecord.param[0]; i++)
		{
			int num = 0;
			if (i == 0)
			{
				zbMonster closestMonster = combatData.monsters.GetClosestMonster(10f);
				num = (((Object)(object)closestMonster != (Object)null) ? closestMonster.GetUUID() : 0);
			}
			((zbMechaSkill10013)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10013.prefab", this, goRiseEffect_1.transform.position, Vector2.up, num, num, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		}
		((zbMechaSkill10013_2)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10013_2.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		yield return (object)new WaitForSeconds(skill.mechaSkillRecord.param[2]);
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		current = anim.PlayAnimation("skill3_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill4()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		zbMechaSkillHandleBase skill = skills[3];
		current = anim.PlayAnimation("skill4_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		for (int i = 0; i < (int)skill.mechaSkillRecord.param[0]; i++)
		{
			zbAudioMgr.Instance.PlayAudio("audio/mecha/10042_1.ogg");
			current = anim.PlayAnimation("skill4_2", loop: false);
			((zbMechaSkill10014)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10014.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
			anim.skeletonAnimation.timeScale = current.AnimationEnd / skill.mechaSkillRecord.param[3];
			yield return (object)new WaitForSeconds(skill.mechaSkillRecord.param[3]);
		}
		anim.skeletonAnimation.timeScale = 1f;
		current = anim.PlayAnimation("skill4_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill5()
	{
		zbAudioMgr.Instance.PlayAudio("audio/mecha/103_5.ogg");
		zbMechaSkillHandleBase skill = skills[4];
		current = anim.PlayAnimation("skill5", loop: false);
		yield return (object)new WaitForSeconds(14f / 15f);
		((zbMechaSkill10015)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10015.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 14f / 15f);
		}
	}

	public override IEnumerator ActionExit()
	{
		zbAudioMgr.Instance.PlayAudio("audio/mecha/103_1.ogg");
		current = anim.PlayAnimation("exit", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		((Component)this).transform.position = new Vector3(100f, 100f, ((Component)this).transform.position.z);
		base.mechaActiving = false;
	}
}
