using System;
using System.Collections;
using Spine;
using UnityEngine;

public class zbMechaHuo01 : zbMechaBase
{
	private TrackEntry current;

	[NonSerialized]
	public bool skill1CanZoom;

	[NonSerialized]
	public float skill1ZoomRange;

	[NonSerialized]
	public zbMechaHuo01Sub mechaHuo01Sub;

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team)
	{
		base.InitData(_combatData, _team);
		mechaHuo01Sub = ((Component)((Component)this).transform.Find("skill2Collider")).GetComponent<zbMechaHuo01Sub>();
	}

	public override void ActionBegin()
	{
		skill1CanZoom = false;
		mechaHuo01Sub.m_monstersInRange.Clear();
		((MonoBehaviour)this).StartCoroutine(Action());
	}

	public override IEnumerator ActionEnter()
	{
		current = anim.PlayAnimation("enter", loop: false);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10020.ogg");
		yield return null;
		((Component)this).transform.position = new Vector3(combatData.nodeRootPositonX, combatData.nodeRootPositonY, ((Component)this).transform.position.z);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill1()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		current = anim.PlayAnimation("skill1_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		AudioSource audio = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_ranshao.ogg", loop: true);
		((zbMechaSkill10021)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10021.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skills[0].SkillId)).SetMechaAtr(skills[0].damageRate * base.Damage, skills[0].mechaSkillRecord);
		current = anim.PlayAnimation("skill1_2", loop: true);
		yield return (object)new WaitForSeconds(skills[0].mechaSkillRecord.param[2]);
		zbAudioMgr.Instance.StopAudio(audio);
		current = anim.PlayAnimation("skill1_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill2()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		current = anim.PlayAnimation("skill2_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		AudioSource audio = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_ranshao.ogg", loop: true);
		zbMechaSkill10022 subSkill = (zbMechaSkill10022)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10022.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skills[1].SkillId);
		subSkill.SetMechaAtr(skills[1].damageRate * base.Damage, skills[1].mechaSkillRecord);
		current = anim.PlayAnimation("skill2_2", loop: true);
		yield return (object)new WaitForSeconds(skills[1].mechaSkillRecord.param[1]);
		zbAudioMgr.Instance.StopAudio(audio);
		current = anim.PlayAnimation("skill2_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		subSkill.ReturnToPool();
	}

	public override IEnumerator ActionSkill3()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		current = anim.PlayAnimation("skill3_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		for (int i = 0; (float)i < skills[2].mechaSkillRecord.param[0]; i++)
		{
			current = anim.PlayAnimation("skill3_2", loop: false);
			yield return null;
			zbMonster randomMonster = combatData.monsters.GetRandomMonster();
			if ((Object)(object)randomMonster != (Object)null)
			{
				zbAudioMgr.Instance.PlayAudio("audio/mecha/10022.ogg");
				((zbMechaSkill10023)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10023.prefab", this, goRiseEffect_2.transform.position, Vector2.up, randomMonster.GetUUID(), 0, skills[2].SkillId)).SetMechaAtr(skills[2].damageRate * base.Damage, skills[2].mechaSkillRecord);
			}
			else
			{
				zbAudioMgr.Instance.PlayAudio("audio/mecha/10022.ogg");
				((zbMechaSkill10023)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10023.prefab", this, goRiseEffect_2.transform.position, Vector2.up, 0, 0, skills[2].SkillId)).SetMechaAtr(skills[2].damageRate * base.Damage, skills[2].mechaSkillRecord);
			}
			if (current != null)
			{
				yield return (object)new WaitForSeconds(current.AnimationEnd);
			}
		}
		current = anim.PlayAnimation("skill3_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill4()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		((zbMechaSkill10024)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10024.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skills[3].SkillId)).SetMechaAtr(skills[3].damageRate * base.Damage, skills[3].mechaSkillRecord);
		current = anim.PlayAnimation("skill4_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		AudioSource audio = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_ranshao.ogg", loop: true);
		current = anim.PlayAnimation("skill4_2", loop: true);
		yield return (object)new WaitForSeconds(skills[3].mechaSkillRecord.param[0]);
		zbAudioMgr.Instance.StopAudio(audio);
		current = anim.PlayAnimation("skill4_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill5()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		current = anim.PlayAnimation("skill5_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		Vector2 val = default(Vector2);
		for (int i = 0; (float)i < skills[4].mechaSkillRecord.param[0]; i++)
		{
			current = anim.PlayAnimation("skill5_2", loop: false);
			zbAudioMgr.Instance.PlayAudio("audio/combat/kongtouhongzha_hit.ogg");
			zbMonster randomMonster = combatData.monsters.GetRandomMonster();
			if ((Object)(object)randomMonster != (Object)null)
			{
				((zbMechaSkill10025)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10025.prefab", this, ((Component)randomMonster).transform.position, Vector2.up, 0, 0, skills[4].SkillId)).SetMechaAtr(skills[4].damageRate * base.Damage, skills[4].mechaSkillRecord);
			}
			else
			{
				((Vector2)(ref val))._002Ector(Rand.Range(-3f, 3f), Rand.Range(-5f, 6f));
				((zbMechaSkill10025)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10025.prefab", this, Vector2.op_Implicit(val), Vector2.up, 0, 0, skills[4].SkillId)).SetMechaAtr(skills[4].damageRate * base.Damage, skills[4].mechaSkillRecord);
			}
			if (current != null)
			{
				yield return (object)new WaitForSeconds(current.AnimationEnd);
			}
		}
		current = anim.PlayAnimation("skill5_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
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
