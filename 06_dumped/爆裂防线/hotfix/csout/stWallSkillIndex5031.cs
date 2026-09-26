using System.Collections.Generic;
using UnityEngine;

public class stWallSkillIndex5031 : zbWallSkillDataBase
{
	protected static readonly List<Vector3> heroVector3s;

	protected static readonly List<Vector3> heroVector3s1;

	protected static readonly List<Vector3> heroVector3s2;

	public zbSafeFloat damageAddPrecent = new zbSafeFloat(0f, "stWallSkillIndex5031.damageAddPrecent");

	public zbSafeInt shootNumber = new zbSafeInt(0, "stWallSkillIndex5031.shootNumber");

	public zbSafeFloat alertRange = new zbSafeFloat(0f, "stWallSkillIndex5031.alertRange");

	public zbSafeFloat duration = new zbSafeFloat(0f, "stWallSkillIndex5031.duration");

	public zbSafeFloat damageInterval = new zbSafeFloat(0f, "stWallSkillIndex5031.damageInterval");

	public zbSafeFloat cd = new zbSafeFloat(0f, "stWallSkillIndex5031.cd");

	public zbSafeFloat atkValRate = new zbSafeFloat(0f, "stWallSkillIndex5031.atkValRate");

	public zbWallSkillIndex5031 wallSkillIndex5031;

	public override void TriggerAction(zbCombatData combatData)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> list = ((combatData.battleSideTeamInfo.HelperTeamInfo.SkillList.Count <= 0) ? heroVector3s : ((!wallSkillIndex5031.isP1) ? heroVector3s2 : heroVector3s1));
		for (int i = 0; i < 2; i++)
		{
			zbWall5031 zbWall5082 = (zbWall5031)zbSkillSystem.Instance.CreateSkill("prefab/combat/wall/wall_5031.prefab", combatData.defense, Vector2.op_Implicit(Vector2.zero), Vector2.up);
			zbWall5082.wallSkillIndex5031Data = this;
			zbWall5082.SetWallAtr((float)atkValRate * (float)(int)wallSkillIndex5031.team.teamAttack, wallSkillIndex5031.wallSkillRecord);
			Vector3 val;
			if (zbCombatSystem.Instance.isPVPBattleType)
			{
				Transform transform = ((Component)zbWall5082).transform;
				val = list[i];
				float num = ((Vector3)(ref val))[0] + combatData.nodeRootPositonX;
				val = list[i];
				transform.position = Vector2.op_Implicit(new Vector2(num, ((Vector3)(ref val))[1] + combatData.nodeRootPositonY - 1.8f));
			}
			else
			{
				Transform transform2 = ((Component)zbWall5082).transform;
				val = list[i];
				float num2 = ((Vector3)(ref val))[0] + combatData.nodeRootPositonX;
				val = list[i];
				transform2.position = Vector2.op_Implicit(new Vector2(num2, ((Vector3)(ref val))[1] + combatData.nodeRootPositonY));
			}
		}
	}

	public override void Restore(List<int> list)
	{
		if (list.Count > 0)
		{
			saveRecord[0] = list[0];
			shootNumber.Value = saveRecord[0];
		}
	}

	public void DesShootNumber()
	{
		--shootNumber;
		saveRecord[0] = shootNumber;
	}

	static stWallSkillIndex5031()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		heroVector3s = new List<Vector3>
		{
			new Vector3(-3f, -2.5f),
			new Vector3(3f, -2.5f)
		};
		heroVector3s1 = new List<Vector3>
		{
			new Vector3(-3f, -2.5f),
			new Vector3(-0.5f, -2.5f)
		};
		heroVector3s2 = new List<Vector3>
		{
			new Vector3(0.5f, -2.5f),
			new Vector3(3f, -2.5f)
		};
	}
}
