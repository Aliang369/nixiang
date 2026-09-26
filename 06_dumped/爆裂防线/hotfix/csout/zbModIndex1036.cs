public class zbModIndex1036 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1036 stModIndex2146 = new stModIndex1036();
		stModIndex2146.roll0to1 = modRecord.clientEff[0];
		stModIndex2146.subSkillId = (int)modRecord.clientEff[1];
		team.modIndex1036.Add(stModIndex2146);
	}
}
