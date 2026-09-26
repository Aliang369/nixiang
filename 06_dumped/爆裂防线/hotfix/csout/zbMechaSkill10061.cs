using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbMechaSkill10061 : zbMechaSkillBase
{
	protected override void OnInited()
	{
		base.OnInited();
		ignoreMonsters.Clear();
		skillAttrList[EAttrSkillType.SlowDown] = 0.9f;
		ReturnToPool(lifetime);
		((MonoBehaviour)this).StartCoroutine(ActionSkill1());
	}

	private void UpdateMonsterCollision()
	{
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(this, monster.Value, base.damage, mechaSkillRecord.atkType);
		}
	}

	public IEnumerator ActionSkill1()
	{
		TrackEntry current = m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		yield return (object)new WaitForSeconds(1.3333334f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/106_daoguang_1.ogg");
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			zbCombatSystem.Instance.buffSystem.CreateBuff(this, skillAttrList, monster.Value.GetUUID(), 1, mechaSkillRecord.param[0]);
		}
		yield return (object)new WaitForSeconds(1f / 3f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/106_daoguang_1.ogg");
		yield return (object)new WaitForSeconds(1f / 3f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/106_daoguang_1.ogg");
		yield return (object)new WaitForSeconds(1f / 3f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/106_daoguang_1.ogg");
		yield return (object)new WaitForSeconds(1f / 3f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/106_daoguang_1.ogg");
		yield return (object)new WaitForSeconds(1.2333333f);
		zbAudioMgr.Instance.PlayAudio("audio/mecha/106_daoguang.ogg");
		UpdateMonsterCollision();
		if (current != null)
		{
			yield return (object)new WaitForSeconds(current.AnimationEnd - 3.9f);
		}
		ReturnToPool();
	}
}
