public class zbModIndex1077 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1077 item = new stModIndex1077
		{
			srcBuffId = (int)modRecord.clientEff[0],
			tarBuffId = (int)modRecord.clientEff[1],
			extraTime = modRecord.clientEff[2],
			skillType = modRecord.skillType[0]
		};
		team.modIndex1077.Add(item);
	}
}
