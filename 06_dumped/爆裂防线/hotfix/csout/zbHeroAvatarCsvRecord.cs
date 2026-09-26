using System;

[Serializable]
public class zbHeroAvatarCsvRecord : zbTableBaseRecord
{
	public int avatarId { get; set; }

	public int hudOffsetY { get; set; }

	public int sex { get; set; }

	public string heroIcon { get; set; }

	public string heroItemIcon { get; set; }

	public string frontBone { get; set; }

	public string fightBone { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbHeroAvatarCsvRecord Read(zbBinReader r)
	{
		return new zbHeroAvatarCsvRecord
		{
			avatarId = r.ReadInt(),
			hudOffsetY = r.ReadInt(),
			sex = r.ReadInt(),
			heroIcon = r.ReadString(),
			heroItemIcon = r.ReadString(),
			frontBone = r.ReadString(),
			fightBone = r.ReadString()
		};
	}
}
