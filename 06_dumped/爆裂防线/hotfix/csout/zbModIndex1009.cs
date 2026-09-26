public class zbModIndex1009 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.subSkillAddDamage[(int)modRecord.clientEff[0]] *= 1f + modRecord.clientEff[1];
		stSubSkillHitToSubBuff item = new stSubSkillHitToSubBuff
		{
			subSkillId = (int)modRecord.clientEff[0],
			roll0to1 = modRecord.clientEff[2],
			subBuffId = (int)modRecord.clientEff[3],
			extraTime = modRecord.clientEff[4]
		};
		team.subSkillHitToSubBuff.Add(item);
	}
}
