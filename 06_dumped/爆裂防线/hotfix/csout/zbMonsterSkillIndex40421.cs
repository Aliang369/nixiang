using System.Collections;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex40421 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex40421.addCdTime = monsterSkillRecord.param[0];
		monster.monsterIndex40421.monsterIndex40421 = true;
		monster.monsterIndex40421.monsterSkill = this;
	}

	public IEnumerator ActionTriggerAttackSkill()
	{
		monster.canMoveOrAttack = false;
		TrackEntry val = PlayTackAnimation(0, flag: false);
		if (monster.combatData.masterTeam.heroList.Count > 1)
		{
			int index = Rand.Range(1, monster.combatData.masterTeam.heroList.Count);
			monster.combatData.masterTeam.heroList[index].nextAddCdTime = monster.monsterIndex40421.addCdTime;
		}
		zbCombatSystem.Instance.combatFormula.CalcWallDamage(monster, monster.combatData.defense);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.AnimationEnd);
		}
		else
		{
			yield return (object)new WaitForSeconds(1f);
		}
		monster.SetCurHp(0L);
	}

	public override void ClearList()
	{
	}
}
