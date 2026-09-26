using System;

[Serializable]
public class zbPeakRobotCsvRecord : zbTableBaseRecord
{
	public int rank { get; set; }

	public int robotId { get; set; }

	public int mst1 { get; set; }

	public int mst2 { get; set; }

	public int mst3 { get; set; }

	public int mst4 { get; set; }

	public int mst5 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPeakRobotCsvRecord Read(zbBinReader r)
	{
		return new zbPeakRobotCsvRecord
		{
			rank = r.ReadInt(),
			robotId = r.ReadInt(),
			mst1 = r.ReadInt(),
			mst2 = r.ReadInt(),
			mst3 = r.ReadInt(),
			mst4 = r.ReadInt(),
			mst5 = r.ReadInt()
		};
	}
}
