using System.Collections.Generic;
using UnityEngine;

public class stWallSkillIndex5021 : zbWallSkillDataBase
{
	public zbSafeFloat defenseDamageDecPre = new zbSafeFloat(0f, "stWallSkillIndex5021.defenseDamageDecPre");

	public zbSafeFloat hpGreaterPresent = new zbSafeFloat(0f, "stWallSkillIndex5021.hpGreaterPresent");

	public int triggerNumber;

	public zbWallSkillIndex5021 wallSkillIndex5021;

	public override void TriggerAction(zbCombatData combatData)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if ((float)hpGreaterPresent > 0f)
		{
			((zbWall5021)zbSkillSystem.Instance.CreateSkill("prefab/combat/wall/wall_5021.prefab", combatData.defense, Vector2.op_Implicit(Vector2.zero), Vector2.up)).SetWallAtr(0f, wallSkillIndex5021.wallSkillRecord);
			combatData.defense.AddShieldHp((int)((float)(long)combatData.defense.maxShieldHp * (float)hpGreaterPresent));
		}
	}

	public override void Restore(List<int> list)
	{
		if (list.Count > 0)
		{
			saveRecord[0] = list[0];
			triggerNumber = saveRecord[0];
		}
	}
}
