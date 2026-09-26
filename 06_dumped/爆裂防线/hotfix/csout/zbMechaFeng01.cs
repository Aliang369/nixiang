using System;
using System.Collections;
using Spine;
using UnityEngine;

public class zbMechaFeng01 : zbMechaBase
{
	private TrackEntry current;

	[NonSerialized]
	public float skill1ZoomRange;

	[NonSerialized]
	private GameObject goRiseEffect_6;

	private GameObject goRiseEffect_7;

	private GameObject goRiseEffect_8;

	private GameObject goRiseEffect_9;

	private GameObject goRiseEffect_10;

	private GameObject goRiseEffect_11;

	private GameObject goRiseEffect_12;

	private GameObject[,] goRiseEffectList = new GameObject[4, 3];

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team)
	{
		base.InitData(_combatData, _team);
		goRiseEffect_6 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_6")).gameObject;
		goRiseEffect_7 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_7")).gameObject;
		goRiseEffect_8 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_8")).gameObject;
		goRiseEffect_9 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_9")).gameObject;
		goRiseEffect_10 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_10")).gameObject;
		goRiseEffect_11 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_11")).gameObject;
		goRiseEffect_12 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_12")).gameObject;
		goRiseEffectList[0, 0] = goRiseEffect_1;
		goRiseEffectList[0, 1] = goRiseEffect_2;
		goRiseEffectList[0, 2] = goRiseEffect_3;
		goRiseEffectList[1, 0] = goRiseEffect_4;
		goRiseEffectList[1, 1] = goRiseEffect_5;
		goRiseEffectList[1, 2] = goRiseEffect_6;
		goRiseEffectList[2, 0] = goRiseEffect_7;
		goRiseEffectList[2, 1] = goRiseEffect_8;
		goRiseEffectList[2, 2] = goRiseEffect_9;
		goRiseEffectList[3, 0] = goRiseEffect_10;
		goRiseEffectList[3, 1] = goRiseEffect_11;
		goRiseEffectList[3, 2] = goRiseEffect_12;
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
		((Component)this).transform.position = new Vector3(combatData.nodeRootPositonX, combatData.nodeRootPositonY + combatData.defense.GetAlertPositionY() + combatData.defense.GetAlertOffsetY() + 1f, ((Component)this).transform.position.z);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill1()
	{
		zbMechaSkillHandleBase zbMechaSkillHandleBase2 = skills[0];
		current = anim.PlayAnimation("skill1", loop: false);
		((zbMechaSkill10061)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10061.prefab", this, Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY)), Vector2.up, 0, 0, zbMechaSkillHandleBase2.SkillId)).SetMechaAtr(zbMechaSkillHandleBase2.damageRate * base.Damage, zbMechaSkillHandleBase2.mechaSkillRecord);
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
		int count = (int)(skill.mechaSkillRecord.param[0] / 2f);
		for (int i = 0; i < count; i++)
		{
			current = anim.PlayAnimation("skill2_2", loop: false);
			zbAudioMgr.Instance.PlayAudio("audio/combat/yasuoqiren.ogg");
			((zbMechaSkill10062)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10062.prefab", this, goRiseEffect_1.transform.position, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
			yield return (object)new WaitForSeconds(5f / 6f);
			zbAudioMgr.Instance.PlayAudio("audio/combat/yasuoqiren.ogg");
			((zbMechaSkill10062)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10062.prefab", this, goRiseEffect_1.transform.position, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
			if (current != null)
			{
				yield return (object)new WaitForSeconds(current.AnimationEnd - 5f / 6f);
			}
		}
		current = anim.PlayAnimation("skill2_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill3()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		zbMechaSkillHandleBase skill = skills[2];
		current = anim.PlayAnimation("skill3_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		Vector3 vector1 = zbUtil.GetTargetNormalized(goRiseEffect_2.transform.position, Vector2.op_Implicit(new Vector2(3.6f, 8.4f)));
		Vector3 vector2 = zbUtil.GetTargetNormalized(goRiseEffect_3.transform.position, Vector2.op_Implicit(new Vector2(-3.6f, 8.4f)));
		int count = (int)skill.mechaSkillRecord.param[0];
		for (int i = 0; i < count; i++)
		{
			current = anim.PlayAnimation("skill3_2", loop: false);
			yield return (object)new WaitForSeconds(11f / 30f);
			zbAudioMgr.Instance.PlayAudio("audio/combat/yasuoqiren.ogg");
			((zbMechaSkill10063)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10063.prefab", this, goRiseEffect_2.transform.position, Vector2.op_Implicit(vector1), 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
			((zbMechaSkill10063)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10063.prefab", this, goRiseEffect_3.transform.position, Vector2.op_Implicit(vector2), 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
			if (current != null)
			{
				yield return (object)new WaitForSeconds(current.AnimationEnd - 11f / 30f);
			}
		}
		if (!GetSkillByIndex(4).IsActive())
		{
			current = anim.PlayAnimation("skill3_3", loop: false);
			if (current != null)
			{
				yield return (object)new WaitForSeconds(current.AnimationEnd);
			}
		}
	}

	public override IEnumerator ActionSkill4()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		zbMechaSkillHandleBase skill = skills[3];
		current = anim.PlayAnimation("skill3toskill4", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		Vector3[,] vector = new Vector3[4, 3]
		{
			{
				Vector2.op_Implicit(Vector2.up),
				default(Vector3),
				default(Vector3)
			},
			{
				default(Vector3),
				default(Vector3),
				default(Vector3)
			},
			{
				default(Vector3),
				default(Vector3),
				default(Vector3)
			},
			{
				default(Vector3),
				default(Vector3),
				default(Vector3)
			}
		};
		Vector2 val = new Vector2(0.5f, 0.866f);
		vector[0, 1] = Vector2.op_Implicit(((Vector2)(ref val)).normalized);
		val = new Vector2(0.866f, 0.5f);
		vector[0, 2] = Vector2.op_Implicit(((Vector2)(ref val)).normalized);
		val = new Vector2(-0.866f, 0.5f);
		vector[1, 0] = Vector2.op_Implicit(((Vector2)(ref val)).normalized);
		val = new Vector2(-0.5f, 0.866f);
		vector[1, 1] = Vector2.op_Implicit(((Vector2)(ref val)).normalized);
		vector[1, 2] = Vector2.op_Implicit(Vector2.up);
		vector[2, 0] = Vector2.op_Implicit(Vector2.down);
		val = new Vector2(-0.5f, -0.866f);
		vector[2, 1] = Vector2.op_Implicit(((Vector2)(ref val)).normalized);
		val = new Vector2(-0.866f, -0.5f);
		vector[2, 2] = Vector2.op_Implicit(((Vector2)(ref val)).normalized);
		val = new Vector2(0.866f, -0.5f);
		vector[3, 0] = Vector2.op_Implicit(((Vector2)(ref val)).normalized);
		val = new Vector2(0.5f, -0.866f);
		vector[3, 1] = Vector2.op_Implicit(((Vector2)(ref val)).normalized);
		vector[3, 2] = Vector2.op_Implicit(Vector2.down);
		int count = (int)(skill.mechaSkillRecord.param[0] / 2f);
		for (int i = 0; i < count; i++)
		{
			current = anim.PlayAnimation("skill4_2", loop: false);
			zbAudioMgr.Instance.PlayAudio("audio/combat/yasuoqiren.ogg");
			for (int j = 0; j < 4; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					zbMechaSkill10064 obj = (zbMechaSkill10064)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10064.prefab", this, goRiseEffectList[j, k].transform.position, Vector2.op_Implicit(vector[j, k]), 0, 0, skill.SkillId);
					obj.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
					obj.sortIndex = j * 3 + k + 1;
				}
			}
			yield return (object)new WaitForSeconds(0.7f);
			zbAudioMgr.Instance.PlayAudio("audio/combat/yasuoqiren.ogg");
			for (int l = 0; l < 4; l++)
			{
				for (int m = 0; m < 3; m++)
				{
					((zbMechaSkill10064)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10064.prefab", this, goRiseEffectList[l, m].transform.position, Vector2.op_Implicit(vector[l, m]), 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
				}
			}
			if (current != null)
			{
				yield return (object)new WaitForSeconds(current.AnimationEnd - 0.7f);
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
		zbAudioMgr.Instance.PlayAudio("audio/mecha/103_5.ogg");
		zbMechaSkillHandleBase skill = skills[4];
		current = anim.PlayAnimation("skill5", loop: false);
		yield return (object)new WaitForSeconds(1.0333333f);
		((zbMechaSkill10065)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10065.prefab", this, goRiseEffect_1.transform.position, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 1.0333333f);
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
