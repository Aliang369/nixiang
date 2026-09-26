public class zbModIndex1065 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1065 stModIndex2146 = new stModIndex1065();
		stModIndex2146.cdDecPreTime = modRecord.clientEff[0];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex1065.Add(stModIndex2146);
	}
}
