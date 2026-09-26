using System;

[Serializable]
public class zbServantCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int skillType { get; set; }

	public string name { get; set; }

	public int avatarId { get; set; }

	public int rarity { get; set; }

	public int sortId { get; set; }

	public int skillId { get; set; }

	public int mainAttrId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbServantCsvRecord Read(zbBinReader r)
	{
		return new zbServantCsvRecord
		{
			id = r.ReadInt(),
			skillType = r.ReadInt(),
			name = r.ReadString(),
			avatarId = r.ReadInt(),
			rarity = r.ReadInt(),
			sortId = r.ReadInt(),
			skillId = r.ReadInt(),
			mainAttrId = r.ReadInt()
		};
	}
}
