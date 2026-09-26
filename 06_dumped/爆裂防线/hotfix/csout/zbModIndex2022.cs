public class zbModIndex2022 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex2022 item = new stModIndex2022
		{
			roll0to1 = modRecord.clientEff[0],
			subBuffId = (int)modRecord.clientEff[1],
			extraTime = modRecord.clientEff[2],
			skillType = modRecord.skillType[0]
		};
		team.modIndex2022.Add(item);
	}
}
