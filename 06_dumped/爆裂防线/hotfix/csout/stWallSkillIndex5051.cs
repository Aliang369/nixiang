using System.Collections.Generic;

public class stWallSkillIndex5051 : zbWallSkillDataBase
{
	public zbSafeFloat damageAddPrecent = new zbSafeFloat(0f, "stWallSkillIndex5051.damageAddPrecent");

	public zbSafeInt triggerNumber = new zbSafeInt(0, "stWallSkillIndex5051.triggerNumber");

	public zbSafeFloat damagePrecent = new zbSafeFloat(0f, "stWallSkillIndex5051.damagePrecent");

	public zbSafeFloat damageRadius = new zbSafeFloat(0f, "stWallSkillIndex5051.damageRadius");

	public zbWallSkillIndex5051 wallSkillIndex5051;

	public override void Restore(List<int> list)
	{
		if (list.Count > 0)
		{
			saveRecord[0] = list[0];
			triggerNumber.Value = saveRecord[0];
		}
	}

	public void DesShootNumber()
	{
		--triggerNumber;
		saveRecord[0] = triggerNumber;
	}
}
