using System;

[Serializable]
public class zbMechaCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public int type { get; set; }

	public int quality { get; set; }

	public float scale { get; set; }

	public float scale1 { get; set; }

	public int sort { get; set; }

	public int[] skillList { get; set; }

	public int skill2 { get; set; }

	public int skill3 { get; set; }

	public int skill4 { get; set; }

	public int skill5 { get; set; }

	public int avatarId { get; set; }

	public int artSpeed { get; set; }

	public float damageRate { get; set; }

	public float levelDamageRate { get; set; }

	public int distanceStop { get; set; }

	public int enterExitSpeed { get; set; }

	public int distance { get; set; }

	public float bodyScale { get; set; }

	public int propId { get; set; }

	public int convertPropId { get; set; }

	public int[] upgradeCost { get; set; }

	public int[] skillCost { get; set; }

	public string preShow { get; set; }

	public string linkTo { get; set; }

	public string banner { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMechaCsvRecord Read(zbBinReader r)
	{
		return new zbMechaCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			type = r.ReadInt(),
			quality = r.ReadInt(),
			scale = r.ReadFloat(),
			scale1 = r.ReadFloat(),
			sort = r.ReadInt(),
			skillList = r.ReadIntArray(),
			skill2 = r.ReadInt(),
			skill3 = r.ReadInt(),
			skill4 = r.ReadInt(),
			skill5 = r.ReadInt(),
			avatarId = r.ReadInt(),
			artSpeed = r.ReadInt(),
			damageRate = r.ReadFloat(),
			levelDamageRate = r.ReadFloat(),
			distanceStop = r.ReadInt(),
			enterExitSpeed = r.ReadInt(),
			distance = r.ReadInt(),
			bodyScale = r.ReadFloat(),
			propId = r.ReadInt(),
			convertPropId = r.ReadInt(),
			upgradeCost = r.ReadIntArray(),
			skillCost = r.ReadIntArray(),
			preShow = r.ReadString(),
			linkTo = r.ReadString(),
			banner = r.ReadString()
		};
	}
}
