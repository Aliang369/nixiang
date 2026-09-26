using System.Collections.Generic;

public class stWallSkillIndex5071 : zbWallSkillDataBase
{
	public zbSafeFloat damageBossAddPrecent = new zbSafeFloat(0f, "stWallSkillIndex5071.damageBossAddPrecent");

	public zbSafeInt subSkillNumber = new zbSafeInt(0, "stWallSkillIndex5071.subSkillNumber");

	public zbSafeFloat fireInterval = new zbSafeFloat(0f, "stWallSkillIndex5071.fireInterval");

	public zbSafeFloat damageInterval = new zbSafeFloat(0f, "stWallSkillIndex5071.damageInterval");

	public zbSafeFloat moveSpeed = new zbSafeFloat(0f, "stWallSkillIndex5071.moveSpeed");

	public zbSafeFloat damagePrecent = new zbSafeFloat(0f, "stWallSkillIndex5071.damagePrecent");

	public int triggerNumber;

	public float lastCallTime;

	public zbWallSkillIndex5071 wallSkillIndex5071;

	public override void Restore(List<int> list)
	{
		if (list.Count > 0)
		{
			saveRecord[0] = list[0];
			triggerNumber = saveRecord[0];
		}
	}
}
