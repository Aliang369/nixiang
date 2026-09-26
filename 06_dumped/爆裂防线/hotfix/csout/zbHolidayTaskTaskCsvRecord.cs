using System;

[Serializable]
public class zbHolidayTaskTaskCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int day { get; set; }

	public int exp { get; set; }

	public int show { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbHolidayTaskTaskCsvRecord Read(zbBinReader r)
	{
		return new zbHolidayTaskTaskCsvRecord
		{
			id = r.ReadInt(),
			day = r.ReadInt(),
			exp = r.ReadInt(),
			show = r.ReadInt()
		};
	}
}
