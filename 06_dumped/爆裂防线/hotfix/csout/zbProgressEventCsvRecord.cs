using System;

[Serializable]
public class zbProgressEventCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string eventStr { get; set; }

	public int firstType { get; set; }

	public string firstSubType1 { get; set; }

	public int firstSubType2 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbProgressEventCsvRecord Read(zbBinReader r)
	{
		return new zbProgressEventCsvRecord
		{
			id = r.ReadInt(),
			eventStr = r.ReadString(),
			firstType = r.ReadInt(),
			firstSubType1 = r.ReadString(),
			firstSubType2 = r.ReadInt()
		};
	}
}
