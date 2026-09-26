using System;

[Serializable]
public class zbWallSubjectRelicCsvRecord : zbTableBaseRecord
{
	public int subjectId { get; set; }

	public int id { get; set; }

	public string name { get; set; }

	public string icon { get; set; }

	public int classId { get; set; }

	public int mod { get; set; }

	public int[] convertAward { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWallSubjectRelicCsvRecord Read(zbBinReader r)
	{
		return new zbWallSubjectRelicCsvRecord
		{
			subjectId = r.ReadInt(),
			id = r.ReadInt(),
			name = r.ReadString(),
			icon = r.ReadString(),
			classId = r.ReadInt(),
			mod = r.ReadInt(),
			convertAward = r.ReadIntArray()
		};
	}
}
