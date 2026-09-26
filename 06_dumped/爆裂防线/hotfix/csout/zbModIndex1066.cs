public class zbModIndex1066 : zbModBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		int key = modRecord.skillType[0];
		float num = modRecord.clientEff[0];
		if (team.mainToDefaultAddDamage.TryGetValue(key, out var value))
		{
			team.mainToDefaultAddDamage[key] = value + num;
		}
		else
		{
			team.mainToDefaultAddDamage.Add(key, num);
		}
	}
}
