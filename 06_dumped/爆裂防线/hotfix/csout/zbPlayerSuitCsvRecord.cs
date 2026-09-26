using System;

[Serializable]
public class zbPlayerSuitCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string icon { get; set; }

	public int avatarId { get; set; }

	public int quality { get; set; }

	public int grade { get; set; }

	public int propId { get; set; }

	public int headId { get; set; }

	public int convertPropId { get; set; }

	public int upgradeId { get; set; }

	public int open { get; set; }

	public int sort { get; set; }

	public string preShow { get; set; }

	public string skillName { get; set; }

	public string skillDesc1 { get; set; }

	public string skillDesc2 { get; set; }

	public string skillDesc3 { get; set; }

	public string skillDesc4 { get; set; }

	public string skillDesc5 { get; set; }

	public string skillIcon { get; set; }

	public int skillType { get; set; }

	public int mod1 { get; set; }

	public int mod2 { get; set; }

	public int mod3 { get; set; }

	public int mod4 { get; set; }

	public int mod5 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	public string tempParam3 { get; set; }

	public string tempParam4 { get; set; }

	public string tempParam5 { get; set; }

	public string tempParam6 { get; set; }

	public string tempParam7 { get; set; }

	internal static zbPlayerSuitCsvRecord Read(zbBinReader r)
	{
		return new zbPlayerSuitCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			icon = r.ReadString(),
			avatarId = r.ReadInt(),
			quality = r.ReadInt(),
			grade = r.ReadInt(),
			propId = r.ReadInt(),
			headId = r.ReadInt(),
			convertPropId = r.ReadInt(),
			upgradeId = r.ReadInt(),
			open = r.ReadInt(),
			sort = r.ReadInt(),
			preShow = r.ReadString(),
			skillName = r.ReadString(),
			skillDesc1 = r.ReadString(),
			skillDesc2 = r.ReadString(),
			skillDesc3 = r.ReadString(),
			skillDesc4 = r.ReadString(),
			skillDesc5 = r.ReadString(),
			skillIcon = r.ReadString(),
			skillType = r.ReadInt(),
			mod1 = r.ReadInt(),
			mod2 = r.ReadInt(),
			mod3 = r.ReadInt(),
			mod4 = r.ReadInt(),
			mod5 = r.ReadInt()
		};
	}
}
