using System;

[Serializable]
public class zbGeneCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string icon { get; set; }

	public int quality { get; set; }

	public int geneShape { get; set; }

	public float[] attr { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbGeneCsvRecord Read(zbBinReader r)
	{
		return new zbGeneCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			icon = r.ReadString(),
			quality = r.ReadInt(),
			geneShape = r.ReadInt(),
			attr = r.ReadFloatArray()
		};
	}
}
