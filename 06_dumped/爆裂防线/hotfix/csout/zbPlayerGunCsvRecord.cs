using System;

[Serializable]
public class zbPlayerGunCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string desc2 { get; set; }

	public string icon { get; set; }

	public string battleBanner { get; set; }

	public string skillIcon { get; set; }

	public string frontBone { get; set; }

	public string fightBone { get; set; }

	public int quality { get; set; }

	public int propId { get; set; }

	public int convertPropId { get; set; }

	public int mod { get; set; }

	public int mod2 { get; set; }

	public int upgradeId { get; set; }

	public int open { get; set; }

	public int sort { get; set; }

	public string preShow { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPlayerGunCsvRecord Read(zbBinReader r)
	{
		return new zbPlayerGunCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			desc2 = r.ReadString(),
			icon = r.ReadString(),
			battleBanner = r.ReadString(),
			skillIcon = r.ReadString(),
			frontBone = r.ReadString(),
			fightBone = r.ReadString(),
			quality = r.ReadInt(),
			propId = r.ReadInt(),
			convertPropId = r.ReadInt(),
			mod = r.ReadInt(),
			mod2 = r.ReadInt(),
			upgradeId = r.ReadInt(),
			open = r.ReadInt(),
			sort = r.ReadInt(),
			preShow = r.ReadString()
		};
	}
}
