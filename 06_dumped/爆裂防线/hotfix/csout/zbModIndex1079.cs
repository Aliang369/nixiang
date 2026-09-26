public class zbModIndex1079 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1079 item = new stModIndex1079
		{
			addExtraTime = modRecord.clientEff[0],
			maxExtraTime = modRecord.clientEff[1],
			skillType = modRecord.skillType[0]
		};
		team.modIndex1079.Add(item);
	}
}
