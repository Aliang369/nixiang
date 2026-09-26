using System.Collections.Generic;

public class stWallSkillIndex5041 : zbWallSkillDataBase
{
	public zbSafeInt addHpByKillUnCall = new zbSafeInt(0, "stWallSkillIndex5041.addHpByKillUnCall");

	public int triggerNumber;

	public zbSafeFloat duration = new zbSafeFloat(0f, "stWallSkillIndex5041.duration");

	public float damageRange;

	public zbSafeFloat damageInterval = new zbSafeFloat(0f, "stWallSkillIndex5041.damageInterval");

	public zbSafeFloat damagePrecent = new zbSafeFloat(0f, "stWallSkillIndex5041.damagePrecent");

	public zbWallSkillIndex5041 wallSkillIndex5041;

	public override void Restore(List<int> list)
	{
		saveRecord[0] = -1;
		saveRecord[1] = -1;
		saveRecord[2] = -1;
		saveRecord[3] = -1;
		saveRecord[4] = -1;
		int num = 0;
		foreach (int item in list)
		{
			saveRecord[num] = item;
			num++;
		}
	}
}
