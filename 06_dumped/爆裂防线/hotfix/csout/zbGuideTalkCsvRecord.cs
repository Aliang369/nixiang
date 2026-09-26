using System;

[Serializable]
public class zbGuideTalkCsvRecord : zbTableBaseRecord
{
	public int talkId { get; set; }

	public int turn { get; set; }

	public string desc { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbGuideTalkCsvRecord Read(zbBinReader r)
	{
		return new zbGuideTalkCsvRecord
		{
			talkId = r.ReadInt(),
			turn = r.ReadInt(),
			desc = r.ReadString()
		};
	}
}
