using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class zbMechaNengliang01 : zbMechaBase
{
	private TrackEntry current;

	[NonSerialized]
	public float skill1ZoomRange;

	[NonSerialized]
	public Bone[] shootList = (Bone[])(object)new Bone[5];

	[NonSerialized]
	public GameObject shootGameObject_1;

	[NonSerialized]
	public GameObject shootGameObject_2;

	[NonSerialized]
	public GameObject shootGameObject_3;

	[NonSerialized]
	public GameObject shootGameObject_4;

	[NonSerialized]
	public GameObject shootGameObject_5;

	[NonSerialized]
	public GameObject goRiseEffect_6;

	[NonSerialized]
	public GameObject goRiseEffect_7;

	public override void InitData(zbCombatData _combatData, zbCombatTeam _team)
	{
		base.InitData(_combatData, _team);
		for (int i = 0; i < 5; i++)
		{
			if (i == 0)
			{
				shootList[i] = ((SkeletonRenderer)anim.skeletonAnimation).skeleton.FindBone("shoot");
			}
			else
			{
				shootList[i] = ((SkeletonRenderer)anim.skeletonAnimation).skeleton.FindBone("shoot_" + (i + 1));
			}
		}
		shootGameObject_1 = ((Component)((Component)this).transform.Find("heroAvatar").Find("shoot_1")).gameObject;
		shootGameObject_2 = ((Component)((Component)this).transform.Find("heroAvatar").Find("shoot_2")).gameObject;
		shootGameObject_3 = ((Component)((Component)this).transform.Find("heroAvatar").Find("shoot_3")).gameObject;
		shootGameObject_4 = ((Component)((Component)this).transform.Find("heroAvatar").Find("shoot_4")).gameObject;
		shootGameObject_5 = ((Component)((Component)this).transform.Find("heroAvatar").Find("shoot_5")).gameObject;
		goRiseEffect_6 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_6")).gameObject;
		goRiseEffect_7 = ((Component)((Component)this).transform.Find("heroAvatar").Find("gunpoint_7")).gameObject;
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
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill1()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		zbMechaSkillHandleBase skill = skills[0];
		current = anim.PlayAnimation("skill1_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		AudioSource audio = zbAudioMgr.Instance.PlayAudio("audio/mecha/105_jiguang.ogg", loop: true);
		zbMechaSkill10051 subSkill = (zbMechaSkill10051)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10051.prefab", this, goRiseEffect_1.transform.position, Vector2.up, 0, 0, skill.SkillId);
		subSkill.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		current = anim.PlayAnimation("skill1_2", loop: true);
		anim.skeletonAnimation.timeScale = current.AnimationEnd / skill.mechaSkillRecord.param[2] * 2f;
		yield return (object)new WaitForSeconds(skill.mechaSkillRecord.param[2]);
		anim.skeletonAnimation.timeScale = 1f;
		zbAudioMgr.Instance.StopAudio(audio);
		subSkill.ReturnToPool();
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
		current = anim.PlayAnimation("skill2", loop: false);
		yield return (object)new WaitForSeconds(2.0333333f);
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		AudioSource audio = zbAudioMgr.Instance.PlayAudio("audio/combat/gaonengshexian.ogg", loop: true);
		anim.skeletonAnimation.timeScale = 10f / skill.mechaSkillRecord.param[1];
		zbMechaSkill10052 subSkill_1 = (zbMechaSkill10052)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10052.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skill.SkillId);
		subSkill_1.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		subSkill_1.goRiseEffect = shootGameObject_1;
		subSkill_1.gunpoint = shootList[0];
		zbMechaSkill10052 subSkill_2 = (zbMechaSkill10052)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10052.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skill.SkillId);
		subSkill_2.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		subSkill_2.goRiseEffect = shootGameObject_2;
		subSkill_2.gunpoint = shootList[1];
		zbMechaSkill10052 subSkill_3 = (zbMechaSkill10052)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10052.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skill.SkillId);
		subSkill_3.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		subSkill_3.goRiseEffect = shootGameObject_3;
		subSkill_3.gunpoint = shootList[2];
		zbMechaSkill10052 subSkill_4 = (zbMechaSkill10052)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10052.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skill.SkillId);
		subSkill_4.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		subSkill_4.goRiseEffect = shootGameObject_4;
		subSkill_4.gunpoint = shootList[3];
		yield return (object)new WaitForSeconds(skill.mechaSkillRecord.param[1]);
		anim.skeletonAnimation.timeScale = 1f;
		subSkill_1.ReturnToPool();
		subSkill_2.ReturnToPool();
		subSkill_3.ReturnToPool();
		subSkill_4.ReturnToPool();
		zbAudioMgr.Instance.StopAudio(audio);
		yield return (object)new WaitForSeconds(0.5f);
	}

	public override IEnumerator ActionSkill3()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		zbMechaSkillHandleBase zbMechaSkillHandleBase2 = skills[2];
		current = anim.PlayAnimation("skill3", loop: false);
		((zbMechaSkill10053)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10053.prefab", this, Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY)), Vector2.up, 0, 0, zbMechaSkillHandleBase2.SkillId)).SetMechaAtr(zbMechaSkillHandleBase2.damageRate * base.Damage, zbMechaSkillHandleBase2.mechaSkillRecord);
		yield return (object)new WaitForSeconds(1.8333334f);
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 1.8333334f);
		}
	}

	public override IEnumerator ActionSkill4()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		zbMechaSkillHandleBase skill = skills[3];
		current = anim.PlayAnimation("skill4_1", loop: false);
		yield return (object)new WaitForSeconds(1f);
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		((Component)this).transform.position = new Vector3(combatData.nodeRootPositonX, combatData.nodeRootPositonY + combatData.defense.GetAlertPositionY() + combatData.defense.GetAlertOffsetY() + 1f, ((Component)this).transform.position.z);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/105_chuansong.ogg");
		yield return (object)new WaitForSeconds(0.6666666f);
		zbMechaSkill10054 subSkill_1 = (zbMechaSkill10054)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10054.prefab", this, goRiseEffect_3.transform.position, Vector2.up, 0, 0, skill.SkillId);
		subSkill_1.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		subSkill_1.Set10054Param(0, shootList[0], shootGameObject_1);
		zbMechaSkill10054 subSkill_2 = (zbMechaSkill10054)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10054.prefab", this, goRiseEffect_4.transform.position, Vector2.up, 0, 0, skill.SkillId);
		subSkill_2.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		subSkill_2.Set10054Param(1, shootList[1], shootGameObject_2);
		zbMechaSkill10054 subSkill_3 = (zbMechaSkill10054)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10054.prefab", this, goRiseEffect_6.transform.position, Vector2.up, 0, 0, skill.SkillId);
		subSkill_3.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		subSkill_3.Set10054Param(2, shootList[2], shootGameObject_3);
		zbMechaSkill10054 subSkill_4 = (zbMechaSkill10054)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10054.prefab", this, goRiseEffect_7.transform.position, Vector2.up, 0, 0, skill.SkillId);
		subSkill_4.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		subSkill_4.Set10054Param(3, shootList[3], shootGameObject_4);
		zbMechaSkill10054 subSkill_5 = (zbMechaSkill10054)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10054.prefab", this, goRiseEffect_5.transform.position, Vector2.up, 0, 0, skill.SkillId);
		subSkill_5.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		subSkill_5.Set10054Param(4, shootList[4], shootGameObject_5);
		yield return (object)new WaitForSeconds(13f / 15f);
		subSkill_1.CanFight(isFight: true);
		subSkill_2.CanFight(isFight: true);
		subSkill_3.CanFight(isFight: true);
		subSkill_4.CanFight(isFight: true);
		subSkill_5.CanFight(isFight: true);
		AudioSource audio1 = zbAudioMgr.Instance.PlayAudio("audio/mecha/105_jiguang.ogg", loop: true);
		AudioSource audio2 = zbAudioMgr.Instance.PlayAudio("audio/combat/gaonengshexian.ogg", loop: true);
		current = anim.PlayAnimation("skill4_2", loop: true);
		yield return (object)new WaitForSeconds(skill.mechaSkillRecord.param[1]);
		subSkill_1.ReturnToPool();
		subSkill_2.ReturnToPool();
		subSkill_3.ReturnToPool();
		subSkill_4.ReturnToPool();
		subSkill_5.ReturnToPool();
		zbAudioMgr.Instance.StopAudio(audio1);
		zbAudioMgr.Instance.StopAudio(audio2);
		current = anim.PlayAnimation("skill4_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
	}

	public override IEnumerator ActionSkill5()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/105_xuli.ogg", loop: true);
		zbMechaSkillHandleBase skill = skills[4];
		current = anim.PlayAnimation("skill5_1", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
		}
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		AudioSource audio = zbAudioMgr.Instance.PlayAudio("audio/mecha/105_skill5.ogg", loop: true);
		zbMechaSkill10055 subSkill = (zbMechaSkill10055)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10055.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skill.SkillId);
		subSkill.SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		current = anim.PlayAnimation("skill5_2", loop: true);
		yield return (object)new WaitForSeconds(skill.mechaSkillRecord.param[1]);
		zbAudioMgr.Instance.StopAudio(audio);
		subSkill.ReturnToPool();
		current = anim.PlayAnimation("skill5_3", loop: false);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd);
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
