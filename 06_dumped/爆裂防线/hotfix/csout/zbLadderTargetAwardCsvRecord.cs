using System;

[Serializable]
public class zbLadderTargetAwardCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int[][] month1 { get; set; }

	public int[][] month2 { get; set; }

	public int[][] month3 { get; set; }

	public int[][] month4 { get; set; }

	public int[][] month5 { get; set; }

	public int[][] month6 { get; set; }

	public int[][] month7 { get; set; }

	public int[][] month8 { get; set; }

	public int[][] month9 { get; set; }

	public int[][] month10 { get; set; }

	public int[][] month11 { get; set; }

	public int[][] month12 { get; set; }

	public int reward { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLadderTargetAwardCsvRecord Read(zbBinReader r)
	{
		return new zbLadderTargetAwardCsvRecord
		{
			id = r.ReadInt(),
			month1 = r.ReadIntArray2(),
			month2 = r.ReadIntArray2(),
			month3 = r.ReadIntArray2(),
			month4 = r.ReadIntArray2(),
			month5 = r.ReadIntArray2(),
			month6 = r.ReadIntArray2(),
			month7 = r.ReadIntArray2(),
			month8 = r.ReadIntArray2(),
			month9 = r.ReadIntArray2(),
			month10 = r.ReadIntArray2(),
			month11 = r.ReadIntArray2(),
			month12 = r.ReadIntArray2(),
			reward = r.ReadInt()
		};
	}
}
