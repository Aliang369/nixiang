using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

public class zbMechaBing01 : zbMechaBase
{
	private TrackEntry current;

	[NonSerialized]
	public float skill1ZoomRange;

	private Vector3 skill2Vector;

	private float[] xs = new float[5] { -2.88f, -1.44f, 0f, 1.44f, 2.88f };

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
		zbAudioMgr.Instance.PlayAudio("audio/mecha/103_enter.ogg");
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
		current = anim.PlayAnimation("skill1", loop: false);
		yield return (object)new WaitForSeconds(1.4666667f);
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		int targetMonsterUUID = 0;
		zbMonster randomMonster;
		for (int i = 0; i < 5; i++)
		{
			zbAudioMgr.Instance.PlayAudio("audio/combat/ganbingdan_fire.ogg");
			randomMonster = combatData.monsters.GetRandomMonster();
			if ((Object)(object)randomMonster != (Object)null)
			{
				targetMonsterUUID = randomMonster.GetUUID();
			}
			((zbMechaSkill10031)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10031.prefab", this, goRiseEffect_1.transform.position, Vector2.up, targetMonsterUUID, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
			yield return (object)new WaitForSeconds(1f / 3f);
		}
		yield return (object)new WaitForSeconds(1f / 3f);
		randomMonster = combatData.monsters.GetRandomMonster();
		zbAudioMgr.Instance.PlayAudio("audio/combat/ganbingdan_fire.ogg");
		if ((Object)(object)randomMonster != (Object)null)
		{
			targetMonsterUUID = randomMonster.GetUUID();
		}
		((zbMechaSkill10031)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10031.prefab", this, goRiseEffect_1.transform.position, Vector2.up, targetMonsterUUID, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 3.4666667f);
		}
	}

	public void GetRandomIndexList(int count, int maxCount, HashSet<int> uniqueNumbers)
	{
		uniqueNumbers.Clear();
		if (count > 0)
		{
			while (uniqueNumbers.Count < count && uniqueNumbers.Count < maxCount)
			{
				uniqueNumbers.Add(Rand.Range(0, maxCount));
			}
		}
	}

	public override IEnumerator ActionSkill2()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		current = anim.PlayAnimation("skill2", loop: false);
		yield return (object)new WaitForSeconds(1f);
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		zbMechaSkillHandleBase skill = skills[1];
		float distance = skill.mechaSkillRecord.param[2] / 100f;
		float y = 1.7f + combatData.defense.GetAlertPositionY() + combatData.defense.GetAlertOffsetY();
		int maxCount = (int)((9f - y) / distance) * 5;
		HashSet<int> hashSet = new HashSet<int>();
		int count = (int)skill.mechaSkillRecord.param[0];
		GetRandomIndexList(count, maxCount, hashSet);
		foreach (int item in hashSet)
		{
			int num2 = item / 5;
			int num3 = item % 5 + 1;
			skill2Vector.x = xs[num3 - 1];
			skill2Vector.y = y + (float)num2 * distance;
			((zbMechaSkill10032)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10032.prefab", this, skill2Vector + new Vector3(combatData.nodeRootPositonX, combatData.nodeRootPositonY), Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
			yield return (object)new WaitForSeconds(0.1f);
		}
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 1f - (float)count * 3f / 30f);
		}
	}

	public override IEnumerator ActionSkill3()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		zbMechaSkillHandleBase zbMechaSkillHandleBase2 = skills[2];
		current = anim.PlayAnimation("skill3", loop: false);
		((zbMechaSkill10033)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10033.prefab", this, Vector2.op_Implicit(new Vector2(combatData.nodeRootPositonX, combatData.nodeRootPositonY)), Vector2.up, 0, 0, zbMechaSkillHandleBase2.SkillId)).SetMechaAtr(zbMechaSkillHandleBase2.damageRate * base.Damage, zbMechaSkillHandleBase2.mechaSkillRecord);
		yield return (object)new WaitForSeconds(1.2f);
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 1.2f);
		}
	}

	public override IEnumerator ActionSkill4()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		zbMechaSkillHandleBase skill = skills[3];
		current = anim.PlayAnimation("skill4", loop: false);
		yield return (object)new WaitForSeconds(17f / 30f);
		((Component)this).transform.position = new Vector3(combatData.nodeRootPositonX, combatData.nodeRootPositonY + combatData.defense.GetAlertPositionY() + combatData.defense.GetAlertOffsetY() + 1f, ((Component)this).transform.position.z);
		yield return (object)new WaitForSeconds(0.26666665f);
		zbAudioMgr.Instance.PlayAudio("audio/ui/rise_normal.ogg");
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		yield return (object)new WaitForSeconds(5f / 6f);
		((zbMechaSkill10034)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10034.prefab", this, ((Component)this).transform.position, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 1.6666666f);
		}
	}

	public override IEnumerator ActionSkill5()
	{
		AudioSource audio_xuli = zbAudioMgr.Instance.PlayAudio("audio/mecha/102_xuli.ogg", loop: true);
		zbMechaSkillHandleBase skill = skills[4];
		current = anim.PlayAnimation("skill5", loop: false);
		yield return (object)new WaitForSeconds(23f / 30f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/10032.ogg");
		zbAudioMgr.Instance.StopAudio(audio_xuli);
		yield return (object)new WaitForSeconds(0.8000001f);
		((zbMechaSkill10035)zbSkillSystem.Instance.CreateSkill("prefab/combat/mecha/mecha_10035.prefab", this, goRiseEffect_2.transform.position, Vector2.up, 0, 0, skill.SkillId)).SetMechaAtr(skill.damageRate * base.Damage, skill.mechaSkillRecord);
		yield return (object)new WaitForSeconds(current.AnimationEnd - 1.5666667f);
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
