public class zbModIndex1045 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		team.ExtraProperties[ETeamExtra.WallDefenseFanji] += modRecord.clientEff[0];
	}
}
