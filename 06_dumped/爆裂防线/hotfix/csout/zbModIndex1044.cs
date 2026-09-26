public class zbModIndex1044 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1044 stModIndex2146 = new stModIndex1044();
		stModIndex2146.firstNumberWave = (int)modRecord.clientEff[0];
		stModIndex2146.damageRatio = modRecord.clientEff[1];
		team.modIndex1044.Add(stModIndex2146);
	}
}
