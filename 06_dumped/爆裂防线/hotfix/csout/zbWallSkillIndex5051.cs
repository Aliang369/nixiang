public class zbWallSkillIndex5051 : zbWallSkillHandleBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		wallSkillDataBase = new stWallSkillIndex5051();
		stWallSkillIndex5051 stWallSkillIndex5082 = (stWallSkillIndex5051)wallSkillDataBase;
		stWallSkillIndex5082.damageAddPrecent.Value += wallSkillRecord.param[0];
		stWallSkillIndex5082.triggerNumber.Value = (int)wallSkillRecord.param[1];
		stWallSkillIndex5082.damageRadius.Value = wallSkillRecord.param[2] / 100f;
		stWallSkillIndex5082.damagePrecent.Value = wallSkillRecord.param[3];
		stWallSkillIndex5082.wallSkillIndex5051 = this;
		stWallSkillIndex5082.saveRecord[0] = stWallSkillIndex5082.triggerNumber;
		combatData.wallSkillIndex5051DataList.Add(stWallSkillIndex5082);
	}
}
