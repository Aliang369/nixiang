public class zbModIndex1024 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1024 item = new stModIndex1024
		{
			srcBuffId = (int)modRecord.clientEff[0],
			damagePercent = modRecord.clientEff[1],
			skillType = modRecord.skillType[0]
		};
		team.modIndex1024.Add(item);
	}
}
