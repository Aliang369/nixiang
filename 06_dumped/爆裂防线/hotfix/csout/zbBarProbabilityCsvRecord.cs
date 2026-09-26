using System;

[Serializable]
public class zbBarProbabilityCsvRecord : zbTableBaseRecord
{
	public int barType { get; set; }

	public int quality { get; set; }

	public string desc { get; set; }

	public float probability { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbBarProbabilityCsvRecord Read(zbBinReader r)
	{
		return new zbBarProbabilityCsvRecord
		{
			barType = r.ReadInt(),
			quality = r.ReadInt(),
			desc = r.ReadString(),
			probability = r.ReadFloat()
		};
	}
}
