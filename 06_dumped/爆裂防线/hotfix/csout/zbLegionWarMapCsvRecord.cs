using System;

[Serializable]
public class zbLegionWarMapCsvRecord : zbTableBaseRecord
{
	public int pointId { get; set; }

	public int mirrorId { get; set; }

	public string pointName { get; set; }

	public int pointType { get; set; }

	public int pointForce { get; set; }

	public int[] connection { get; set; }

	public int actionCost { get; set; }

	public int[][] awardList { get; set; }

	public int standCrowded { get; set; }

	public float standOccupyPct { get; set; }

	public float standScore { get; set; }

	public string spineName { get; set; }

	public int sceneId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLegionWarMapCsvRecord Read(zbBinReader r)
	{
		return new zbLegionWarMapCsvRecord
		{
			pointId = r.ReadInt(),
			mirrorId = r.ReadInt(),
			pointName = r.ReadString(),
			pointType = r.ReadInt(),
			pointForce = r.ReadInt(),
			connection = r.ReadIntArray(),
			actionCost = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			standCrowded = r.ReadInt(),
			standOccupyPct = r.ReadFloat(),
			standScore = r.ReadFloat(),
			spineName = r.ReadString(),
			sceneId = r.ReadInt()
		};
	}
}
