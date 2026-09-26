using System.Collections;
using System.Linq;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex20101 : zbMonsterSkillBase
{
	private int monsterUUID;

	public override void RefreshProperty()
	{
		stMonsterSkillIndex20101 stMonsterSkillIndex40463 = new stMonsterSkillIndex20101();
		stMonsterSkillIndex40463.distance = monsterSkillRecord.param[0] / 100f;
		base.RefreshProperty();
		loopStart = 1;
		loopNumber = 2;
		int num = 0;
		while (true)
		{
			int num2 = num * loopNumber + loopStart;
			if (num2 >= monsterSkillRecord.param.Count())
			{
				break;
			}
			stMonsterSkillIndex40463.monsterNumList.Add((int)monsterSkillRecord.param[num2]);
			stMonsterSkillIndex40463.monsterTidList.Add((int)monsterSkillRecord.param[num2 + 1]);
			num++;
		}
		monsterUUID = monster.GetUUID();
		if (!monster.isRestoreData)
		{
			((MonoBehaviour)monster).StartCoroutine(ActionCoroutine(stMonsterSkillIndex40463));
		}
	}

	public IEnumerator ActionCoroutine(stMonsterSkillIndex20101 monsterIndex20101)
	{
		TrackEntry val = PlayTackAnimation(1, flag: false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.AnimationEnd);
		}
		else
		{
			yield return (object)new WaitForSeconds(1f / 3f);
		}
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			yield break;
		}
		int num = 0;
		foreach (int monsterNum in monsterIndex20101.monsterNumList)
		{
			num += monsterNum;
		}
		if (num == 1 && (long)monster.curHp <= 0)
		{
			combatData.monsters.SpawnMonsterAtPosition(monsterIndex20101.monsterTidList[0], ((Component)monster).transform.position, ((Component)monster).transform.parent);
			yield break;
		}
		int num2 = 0;
		int num3 = 0;
		foreach (int monsterNum2 in monsterIndex20101.monsterNumList)
		{
			for (int i = 0; i < monsterNum2; i++)
			{
				num2 = ((num2 >= 8) ? 7 : num2);
				combatData.monsters.SpawnMonsterAtPosition(monsterIndex20101.monsterTidList[num3], zbMonsterSkillBase.vector3s[num2] * monsterIndex20101.distance + ((Component)monster).transform.position, ((Component)monster).transform.parent);
				num2++;
			}
			num3++;
		}
	}

	public override void ClearList()
	{
	}
}
