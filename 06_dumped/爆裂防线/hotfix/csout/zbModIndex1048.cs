public class zbModIndex1048 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1048 item = new stModIndex1048
		{
			firstBelowHp = modRecord.clientEff[0],
			immunityDuration = modRecord.clientEff[1]
		};
		team.modIndex1048.Add(item);
	}
}
