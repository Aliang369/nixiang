public class zbModIndex1012 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMainKillToSubSkill item = new stMainKillToSubSkill
		{
			subCount = (int)modRecord.clientEff[0],
			roll0to1 = modRecord.clientEff[1],
			subSkillId = (int)modRecord.clientEff[2],
			skillType = modRecord.skillType[0]
		};
		team.mainKillToSubSkill.Add(item);
	}
}
