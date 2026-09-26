using System;

[Serializable]
public class zbTacticCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public string icon { get; set; }

	public int quality { get; set; }

	public int modId { get; set; }

	public int[][] modChange { get; set; }

	public int attrType { get; set; }

	public int[] attr { get; set; }

	public float val { get; set; }

	public int ratioId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbTacticCsvRecord Read(zbBinReader r)
	{
		return new zbTacticCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			icon = r.ReadString(),
			quality = r.ReadInt(),
			modId = r.ReadInt(),
			modChange = r.ReadIntArray2(),
			attrType = r.ReadInt(),
			attr = r.ReadIntArray(),
			val = r.ReadFloat(),
			ratioId = r.ReadInt()
		};
	}
}
