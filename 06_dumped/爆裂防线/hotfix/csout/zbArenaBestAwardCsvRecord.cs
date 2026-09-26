using System;

[Serializable]
public class zbArenaBestAwardCsvRecord : zbTableBaseRecord
{
	public int rank { get; set; }

	public float diamond { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbArenaBestAwardCsvRecord Read(zbBinReader r)
	{
		return new zbArenaBestAwardCsvRecord
		{
			rank = r.ReadInt(),
			diamond = r.ReadFloat()
		};
	}
}
