public class zbModIndex1063 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1063 stModIndex2146 = new stModIndex1063();
		stModIndex2146.roll0to1 = modRecord.clientEff[0];
		stModIndex2146.subBuffId = (int)modRecord.clientEff[1];
		stModIndex2146.extraTime = modRecord.clientEff[2];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex1063.Add(stModIndex2146);
	}
}
