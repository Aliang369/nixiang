using System.Linq;
using UnityEngine;

public class zbMonsterSkillIndex40091 : zbMonsterSkillBase
{
	private stMonsterSkillIndex40091 monsterIndex40091 = new stMonsterSkillIndex40091();

	private int monsterUUID;

	private float dieTimeKeep;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		loopStart = 2;
		loopNumber = 2;
		monsterIndex40091.dieTime = monsterSkillRecord.param[0];
		monsterIndex40091.distance = monsterSkillRecord.param[1] / 100f;
		int num = 0;
		while (true)
		{
			int num2 = num * loopNumber + loopStart;
			if (num2 >= monsterSkillRecord.param.Count())
			{
				break;
			}
			monsterIndex40091.monsterNumList.Add((int)monsterSkillRecord.param[num2]);
			monsterIndex40091.monsterTidList.Add((int)monsterSkillRecord.param[num2 + 1]);
			num++;
		}
		dieTimeKeep = 0f;
		monsterUUID = monster.GetUUID();
	}

	public override void Update()
	{
		dieTimeKeep += Time.deltaTime;
		if (dieTimeKeep >= monsterIndex40091.dieTime)
		{
			CaclCallMonster();
			dieTimeKeep = 0f;
		}
	}

	public void CaclCallMonster()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (int monsterNum in monsterIndex40091.monsterNumList)
		{
			num += monsterNum;
		}
		if (num == 1)
		{
			combatData.monsters.SpawnMonsterAtPosition(monsterIndex40091.monsterTidList[0], ((Component)monster).transform.position, ((Component)monster).transform.parent);
		}
		else
		{
			int num2 = 0;
			int num3 = 0;
			foreach (int monsterNum2 in monsterIndex40091.monsterNumList)
			{
				for (int i = 0; i < monsterNum2; i++)
				{
					num2 = ((num2 >= 8) ? 7 : num2);
					combatData.monsters.SpawnMonsterAtPosition(monsterIndex40091.monsterTidList[num3], zbMonsterSkillBase.vector3s[num2] * 0.5f + ((Component)monster).transform.position, ((Component)monster).transform.parent);
					num2++;
				}
				num3++;
			}
		}
		monster.SetCurHp(0L);
	}

	public override void ClearList()
	{
	}
}
