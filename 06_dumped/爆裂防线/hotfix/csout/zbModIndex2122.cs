public class zbModIndex2122 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.DianqiuMod2122Roll] = modRecord.clientEff[0];
		team.ExtraProperties[ETeamExtra.DianqiuMod2122Cont] = modRecord.clientEff[1];
	}
}
