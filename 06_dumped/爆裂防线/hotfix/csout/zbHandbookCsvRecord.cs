using System;

[Serializable]
public class zbHandbookCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public int type { get; set; }

	public int avatarId { get; set; }

	public int index { get; set; }

	public float bodyScale { get; set; }

	public int itemId { get; set; }

	public int[] adviceList { get; set; }

	public int elementType { get; set; }

	public string desc2 { get; set; }

	public int isShow { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbHandbookCsvRecord Read(zbBinReader r)
	{
		return new zbHandbookCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			type = r.ReadInt(),
			avatarId = r.ReadInt(),
			index = r.ReadInt(),
			bodyScale = r.ReadFloat(),
			itemId = r.ReadInt(),
			adviceList = r.ReadIntArray(),
			elementType = r.ReadInt(),
			desc2 = r.ReadString(),
			isShow = r.ReadInt()
		};
	}
}
