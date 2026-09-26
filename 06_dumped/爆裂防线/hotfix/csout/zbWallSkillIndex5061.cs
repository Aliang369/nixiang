public class zbWallSkillIndex5061 : zbWallSkillHandleBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		wallSkillDataBase = new stWallSkillIndex5061();
		stWallSkillIndex5061 stWallSkillIndex5082 = (stWallSkillIndex5061)wallSkillDataBase;
		stWallSkillIndex5082.addImmunity.Value = (int)wallSkillRecord.param[0];
		stWallSkillIndex5082.knockback.Value = wallSkillRecord.param[1] / 100f;
		stWallSkillIndex5082.damagePrecent.Value = wallSkillRecord.param[2];
		stWallSkillIndex5082.towMonsterType.Value = (int)wallSkillRecord.param[3];
		stWallSkillIndex5082.wallSkillIndex5061 = this;
		stWallSkillIndex5082.triggerNumber = 1;
		stWallSkillIndex5082.saveRecord[0] = stWallSkillIndex5082.triggerNumber;
		combatData.wallSkillIndex5061DataList.Add(stWallSkillIndex5082);
	}
}
