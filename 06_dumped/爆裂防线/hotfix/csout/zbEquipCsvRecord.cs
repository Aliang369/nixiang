using System;

[Serializable]
public class zbEquipCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string icon { get; set; }

	public int quality { get; set; }

	public int[] teamAtk { get; set; }

	public int subId1 { get; set; }

	public int exSubId1 { get; set; }

	public int subId2 { get; set; }

	public int exSubId2 { get; set; }

	public int subId3 { get; set; }

	public int exSubId3 { get; set; }

	public int subId4 { get; set; }

	public int exSubId4 { get; set; }

	public int subId5 { get; set; }

	public int exSubId5 { get; set; }

	public int upId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbEquipCsvRecord Read(zbBinReader r)
	{
		return new zbEquipCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			icon = r.ReadString(),
			quality = r.ReadInt(),
			teamAtk = r.ReadIntArray(),
			subId1 = r.ReadInt(),
			exSubId1 = r.ReadInt(),
			subId2 = r.ReadInt(),
			exSubId2 = r.ReadInt(),
			subId3 = r.ReadInt(),
			exSubId3 = r.ReadInt(),
			subId4 = r.ReadInt(),
			exSubId4 = r.ReadInt(),
			subId5 = r.ReadInt(),
			exSubId5 = r.ReadInt(),
			upId = r.ReadInt()
		};
	}
}
