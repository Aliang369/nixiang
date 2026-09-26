using System;

[Serializable]
public class zbSignTargetCsvRecord : zbTableBaseRecord
{
	public int index { get; set; }

	public int day { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSignTargetCsvRecord Read(zbBinReader r)
	{
		return new zbSignTargetCsvRecord
		{
			index = r.ReadInt(),
			day = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
