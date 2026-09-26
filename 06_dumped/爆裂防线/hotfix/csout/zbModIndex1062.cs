public class zbModIndex1062 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1062 stModIndex2146 = new stModIndex1062();
		stModIndex2146.roll0to1 = modRecord.clientEff[0];
		stModIndex2146.skillType = modRecord.skillType[0];
		team.modIndex1062.Add(stModIndex2146);
	}
}
