using System;

[Serializable]
public class zbSkillTypeCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public int unlockLevel { get; set; }

	public string bulletPrefab { get; set; }

	public string icon1 { get; set; }

	public string icon2 { get; set; }

	public string icon3 { get; set; }

	public string icon4 { get; set; }

	public int modId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSkillTypeCsvRecord Read(zbBinReader r)
	{
		return new zbSkillTypeCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			unlockLevel = r.ReadInt(),
			bulletPrefab = r.ReadString(),
			icon1 = r.ReadString(),
			icon2 = r.ReadString(),
			icon3 = r.ReadString(),
			icon4 = r.ReadString(),
			modId = r.ReadInt()
		};
	}
}
