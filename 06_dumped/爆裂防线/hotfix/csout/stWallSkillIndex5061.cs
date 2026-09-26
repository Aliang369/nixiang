using System.Collections.Generic;

public class stWallSkillIndex5061 : zbWallSkillDataBase
{
	public zbSafeInt addImmunity = new zbSafeInt(0, "stWallSkillIndex5061.addImmunity");

	public zbSafeFloat knockback = new zbSafeFloat(0f, "stWallSkillIndex5061.knockback");

	public zbSafeFloat damagePrecent = new zbSafeFloat(0f, "stWallSkillIndex5061.damagePrecent");

	public zbSafeInt towMonsterType = new zbSafeInt(0, "stWallSkillIndex5061.towMonsterType");

	public int triggerNumber;

	public zbWallSkillIndex5061 wallSkillIndex5061;

	public override void Restore(List<int> list)
	{
		if (list.Count > 0)
		{
			saveRecord[0] = list[0];
			triggerNumber = saveRecord[0];
		}
	}
}
