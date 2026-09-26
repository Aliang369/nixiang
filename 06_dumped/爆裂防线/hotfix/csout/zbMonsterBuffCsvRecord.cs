using System;

[Serializable]
public class zbMonsterBuffCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string desc { get; set; }

	public string desc_zh { get; set; }

	public int buffIndex { get; set; }

	public string spinePath { get; set; }

	public int spinePoint { get; set; }

	public float param { get; set; }

	public int layer { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMonsterBuffCsvRecord Read(zbBinReader r)
	{
		return new zbMonsterBuffCsvRecord
		{
			id = r.ReadInt(),
			desc = r.ReadString(),
			desc_zh = r.ReadString(),
			buffIndex = r.ReadInt(),
			spinePath = r.ReadString(),
			spinePoint = r.ReadInt(),
			param = r.ReadFloat(),
			layer = r.ReadInt()
		};
	}
}
