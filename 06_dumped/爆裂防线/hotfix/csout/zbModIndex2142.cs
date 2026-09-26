public class zbModIndex2142 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex2142 item = new stModIndex2142
		{
			srcBuffId = (int)modRecord.clientEff[0],
			knockbackPercent = modRecord.clientEff[1],
			skillType = modRecord.skillType[0]
		};
		team.modIndex2142.Add(item);
	}
}
