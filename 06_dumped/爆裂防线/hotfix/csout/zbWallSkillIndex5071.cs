public class zbWallSkillIndex5071 : zbWallSkillHandleBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		wallSkillDataBase = new stWallSkillIndex5071();
		stWallSkillIndex5071 stWallSkillIndex5082 = (stWallSkillIndex5071)wallSkillDataBase;
		stWallSkillIndex5082.damageBossAddPrecent.Value = wallSkillRecord.param[0];
		stWallSkillIndex5082.subSkillNumber.Value = (int)wallSkillRecord.param[1];
		stWallSkillIndex5082.fireInterval.Value = wallSkillRecord.param[2];
		stWallSkillIndex5082.damageInterval.Value = wallSkillRecord.param[3];
		stWallSkillIndex5082.moveSpeed.Value = wallSkillRecord.param[4] / 100f;
		stWallSkillIndex5082.damagePrecent.Value = wallSkillRecord.param[5];
		stWallSkillIndex5082.wallSkillIndex5071 = this;
		stWallSkillIndex5082.triggerNumber = 1;
		stWallSkillIndex5082.saveRecord[0] = stWallSkillIndex5082.triggerNumber;
		combatData.wallSkillIndex5071DataList.Add(stWallSkillIndex5082);
	}
}
