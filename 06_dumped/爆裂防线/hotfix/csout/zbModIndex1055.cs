public class zbModIndex1055 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stModIndex1055 item = new stModIndex1055
		{
			defenseHpLess = modRecord.clientEff[0],
			extraDamageRatio = modRecord.clientEff[1]
		};
		team.modIndex1055.Add(item);
	}
}
