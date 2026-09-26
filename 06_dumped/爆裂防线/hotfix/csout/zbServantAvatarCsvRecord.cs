using System;

[Serializable]
public class zbServantAvatarCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string servantIcon { get; set; }

	public string frontBone { get; set; }

	public string fightBone { get; set; }

	public int fightOffset { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbServantAvatarCsvRecord Read(zbBinReader r)
	{
		return new zbServantAvatarCsvRecord
		{
			id = r.ReadInt(),
			servantIcon = r.ReadString(),
			frontBone = r.ReadString(),
			fightBone = r.ReadString(),
			fightOffset = r.ReadInt()
		};
	}
}
