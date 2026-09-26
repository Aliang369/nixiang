using System;

[Serializable]
public class zbNpcJobCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public int[] stren { get; set; }

	public int[] agil { get; set; }

	public int[] intel { get; set; }

	public int[] recommendRoomList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbNpcJobCsvRecord Read(zbBinReader r)
	{
		return new zbNpcJobCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			stren = r.ReadIntArray(),
			agil = r.ReadIntArray(),
			intel = r.ReadIntArray(),
			recommendRoomList = r.ReadIntArray()
		};
	}
}
