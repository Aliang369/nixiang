public class zbWallSkillIndex5021 : zbWallSkillHandleBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		wallSkillDataBase = new stWallSkillIndex5021();
		stWallSkillIndex5021 stWallSkillIndex5082 = (stWallSkillIndex5021)wallSkillDataBase;
		stWallSkillIndex5082.defenseDamageDecPre.Value = wallSkillRecord.param[0];
		stWallSkillIndex5082.hpGreaterPresent.Value = wallSkillRecord.param[1];
		stWallSkillIndex5082.triggerNumber = 1;
		stWallSkillIndex5082.saveRecord[0] = stWallSkillIndex5082.triggerNumber;
		stWallSkillIndex5082.wallSkillIndex5021 = this;
		combatData.wallSkillIndex5021DataList.Add(stWallSkillIndex5082);
	}
}
