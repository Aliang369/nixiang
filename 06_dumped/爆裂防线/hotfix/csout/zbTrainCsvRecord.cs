using System;

[Serializable]
public class zbTrainCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public int waitTime { get; set; }

	public int runTime { get; set; }

	public int goodsId { get; set; }

	public int skinId { get; set; }

	public int[][] costList { get; set; }

	public int[][] driverAwardList { get; set; }

	public int mst1 { get; set; }

	public int mst2 { get; set; }

	public int mst3 { get; set; }

	public int mst4 { get; set; }

	public int mst5 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbTrainCsvRecord Read(zbBinReader r)
	{
		return new zbTrainCsvRecord
		{
			quality = r.ReadInt(),
			waitTime = r.ReadInt(),
			runTime = r.ReadInt(),
			goodsId = r.ReadInt(),
			skinId = r.ReadInt(),
			costList = r.ReadIntArray2(),
			driverAwardList = r.ReadIntArray2(),
			mst1 = r.ReadInt(),
			mst2 = r.ReadInt(),
			mst3 = r.ReadInt(),
			mst4 = r.ReadInt(),
			mst5 = r.ReadInt()
		};
	}
}
