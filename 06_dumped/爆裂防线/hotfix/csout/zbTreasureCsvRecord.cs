using System;

[Serializable]
public class zbTreasureCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string icon { get; set; }

	public int quality { get; set; }

	public int propId { get; set; }

	public float[] attr { get; set; }

	public string version { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbTreasureCsvRecord Read(zbBinReader r)
	{
		return new zbTreasureCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			icon = r.ReadString(),
			quality = r.ReadInt(),
			propId = r.ReadInt(),
			attr = r.ReadFloatArray(),
			version = r.ReadString()
		};
	}
}
