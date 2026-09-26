using System;

[Serializable]
public class zbTacticQualityCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public int frag { get; set; }

	public int tacticCoin { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbTacticQualityCsvRecord Read(zbBinReader r)
	{
		return new zbTacticQualityCsvRecord
		{
			quality = r.ReadInt(),
			frag = r.ReadInt(),
			tacticCoin = r.ReadInt()
		};
	}
}
