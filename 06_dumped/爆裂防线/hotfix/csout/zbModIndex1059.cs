public class zbModIndex1059 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1059 item = new stModIndex1059
		{
			skillAttackType = (int)modRecord.clientEff[0],
			buffId = (int)modRecord.clientEff[1],
			extraTime = modRecord.clientEff[0]
		};
		team.modIndex1059.Add(item);
	}
}
