using System;

[Serializable]
public class zbBoxCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string icon { get; set; }

	public int quality { get; set; }

	public int boxCost { get; set; }

	public int box { get; set; }

	public int key { get; set; }

	public int unfold { get; set; }

	public int batchLimit { get; set; }

	public int batchStep { get; set; }

	public int boxShow { get; set; }

	public string preShow { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbBoxCsvRecord Read(zbBinReader r)
	{
		return new zbBoxCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			icon = r.ReadString(),
			quality = r.ReadInt(),
			boxCost = r.ReadInt(),
			box = r.ReadInt(),
			key = r.ReadInt(),
			unfold = r.ReadInt(),
			batchLimit = r.ReadInt(),
			batchStep = r.ReadInt(),
			boxShow = r.ReadInt(),
			preShow = r.ReadString()
		};
	}
}
