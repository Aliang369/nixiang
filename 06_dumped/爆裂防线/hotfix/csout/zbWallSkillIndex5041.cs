public class zbWallSkillIndex5041 : zbWallSkillHandleBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		wallSkillDataBase = new stWallSkillIndex5041();
		stWallSkillIndex5041 stWallSkillIndex5082 = (stWallSkillIndex5041)wallSkillDataBase;
		stWallSkillIndex5082.addHpByKillUnCall.Value = (int)wallSkillRecord.param[0];
		stWallSkillIndex5082.triggerNumber = (int)wallSkillRecord.param[1];
		stWallSkillIndex5082.duration.Value = wallSkillRecord.param[2];
		stWallSkillIndex5082.damageInterval.Value = wallSkillRecord.param[3];
		stWallSkillIndex5082.damagePrecent.Value = wallSkillRecord.param[4];
		stWallSkillIndex5082.wallSkillIndex5041 = this;
		combatData.wallSkillIndex5041DataList.Add(stWallSkillIndex5082);
		if (stWallSkillIndex5082.triggerNumber > 0)
		{
			stWallSkillIndex5082.saveRecord[0] = 0;
			stWallSkillIndex5082.saveRecord[1] = 1;
			stWallSkillIndex5082.saveRecord[2] = 2;
			stWallSkillIndex5082.saveRecord[3] = 3;
			stWallSkillIndex5082.saveRecord[4] = 4;
		}
		else
		{
			stWallSkillIndex5082.saveRecord[0] = -1;
			stWallSkillIndex5082.saveRecord[1] = -1;
			stWallSkillIndex5082.saveRecord[2] = -1;
			stWallSkillIndex5082.saveRecord[3] = -1;
			stWallSkillIndex5082.saveRecord[4] = -1;
		}
	}
}
