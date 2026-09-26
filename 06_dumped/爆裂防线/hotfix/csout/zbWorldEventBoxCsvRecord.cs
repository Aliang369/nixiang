using System;

[Serializable]
public class zbWorldEventBoxCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public int quality { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWorldEventBoxCsvRecord Read(zbBinReader r)
	{
		return new zbWorldEventBoxCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			quality = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
