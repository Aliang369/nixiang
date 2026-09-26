using System;

[Serializable]
public class zbHeroCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public int sortId { get; set; }

	public int avatarId { get; set; }

	public int rarity { get; set; }

	public int skillId { get; set; }

	public int modNum { get; set; }

	public int mod1 { get; set; }

	public int mod2 { get; set; }

	public int mod3 { get; set; }

	public int mod4 { get; set; }

	public int mod5 { get; set; }

	public int mod6 { get; set; }

	public string shortWord1 { get; set; }

	public string shortWord2 { get; set; }

	public string shortWord3 { get; set; }

	public string shortWord4 { get; set; }

	public string shortWord5 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbHeroCsvRecord Read(zbBinReader r)
	{
		return new zbHeroCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			sortId = r.ReadInt(),
			avatarId = r.ReadInt(),
			rarity = r.ReadInt(),
			skillId = r.ReadInt(),
			modNum = r.ReadInt(),
			mod1 = r.ReadInt(),
			mod2 = r.ReadInt(),
			mod3 = r.ReadInt(),
			mod4 = r.ReadInt(),
			mod5 = r.ReadInt(),
			mod6 = r.ReadInt(),
			shortWord1 = r.ReadString(),
			shortWord2 = r.ReadString(),
			shortWord3 = r.ReadString(),
			shortWord4 = r.ReadString(),
			shortWord5 = r.ReadString()
		};
	}
}
