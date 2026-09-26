public class zbWallSkillIndex5031 : zbWallSkillHandleBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		wallSkillDataBase = new stWallSkillIndex5031();
		stWallSkillIndex5031 stWallSkillIndex5082 = (stWallSkillIndex5031)wallSkillDataBase;
		stWallSkillIndex5082.damageAddPrecent.Value += wallSkillRecord.param[0];
		stWallSkillIndex5082.shootNumber.Value = (int)wallSkillRecord.param[1];
		stWallSkillIndex5082.alertRange.Value = wallSkillRecord.param[2] / 100f;
		stWallSkillIndex5082.duration.Value = wallSkillRecord.param[3];
		stWallSkillIndex5082.damageInterval.Value = wallSkillRecord.param[4];
		stWallSkillIndex5082.cd.Value = wallSkillRecord.param[5];
		stWallSkillIndex5082.atkValRate.Value = wallSkillRecord.param[6];
		stWallSkillIndex5082.wallSkillIndex5031 = this;
		stWallSkillIndex5082.saveRecord[0] = stWallSkillIndex5082.shootNumber;
		combatData.wallSkillIndex5031DataList.Add(stWallSkillIndex5082);
	}
}
