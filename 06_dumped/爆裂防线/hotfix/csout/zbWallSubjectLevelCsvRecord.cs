using System;

[Serializable]
public class zbWallSubjectLevelCsvRecord : zbTableBaseRecord
{
	public int subjectId { get; set; }

	public int level { get; set; }

	public int chapter { get; set; }

	public int mstLv { get; set; }

	public int[][] award { get; set; }

	public int sceneId { get; set; }

	public int mst1 { get; set; }

	public int mst2 { get; set; }

	public int mst3 { get; set; }

	public int mst4 { get; set; }

	public int mst5 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWallSubjectLevelCsvRecord Read(zbBinReader r)
	{
		return new zbWallSubjectLevelCsvRecord
		{
			subjectId = r.ReadInt(),
			level = r.ReadInt(),
			chapter = r.ReadInt(),
			mstLv = r.ReadInt(),
			award = r.ReadIntArray2(),
			sceneId = r.ReadInt(),
			mst1 = r.ReadInt(),
			mst2 = r.ReadInt(),
			mst3 = r.ReadInt(),
			mst4 = r.ReadInt(),
			mst5 = r.ReadInt()
		};
	}
}
