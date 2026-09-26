using System;

[Serializable]
public class zbModCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string name_zh { get; set; }

	public string desc { get; set; }

	public string desc_zh { get; set; }

	public string icon { get; set; }

	public int quality { get; set; }

	public int modIndex { get; set; }

	public float[] clientEff { get; set; }

	public int serverAttr { get; set; }

	public int attrType { get; set; }

	public int[][] attrList { get; set; }

	public float[] valueList { get; set; }

	public int skillLvUp { get; set; }

	public int[] skillType { get; set; }

	public int pvpMark { get; set; }

	public int handbookId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbModCsvRecord Read(zbBinReader r)
	{
		return new zbModCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			name_zh = r.ReadString(),
			desc = r.ReadString(),
			desc_zh = r.ReadString(),
			icon = r.ReadString(),
			quality = r.ReadInt(),
			modIndex = r.ReadInt(),
			clientEff = r.ReadFloatArray(),
			serverAttr = r.ReadInt(),
			attrType = r.ReadInt(),
			attrList = r.ReadIntArray2(),
			valueList = r.ReadFloatArray(),
			skillLvUp = r.ReadInt(),
			skillType = r.ReadIntArray(),
			pvpMark = r.ReadInt(),
			handbookId = r.ReadInt()
		};
	}
}
