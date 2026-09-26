public class zbModIndex1016 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMainSkillBuffExtraBuff item = new stMainSkillBuffExtraBuff
		{
			srcBuffId = (int)modRecord.clientEff[0],
			tarBuffId = (int)modRecord.clientEff[1],
			extraTime = modRecord.clientEff[2],
			skillType = modRecord.skillType[0]
		};
		team.mainSkillBuffExtraBuff.Add(item);
	}
}
