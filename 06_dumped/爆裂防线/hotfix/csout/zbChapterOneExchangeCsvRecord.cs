using System;

[Serializable]
public class zbChapterOneExchangeCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int[] award { get; set; }

	public int moneyType { get; set; }

	public int price { get; set; }

	public int limitNum { get; set; }

	public int showType { get; set; }

	public int[] showList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterOneExchangeCsvRecord Read(zbBinReader r)
	{
		return new zbChapterOneExchangeCsvRecord
		{
			id = r.ReadInt(),
			award = r.ReadIntArray(),
			moneyType = r.ReadInt(),
			price = r.ReadInt(),
			limitNum = r.ReadInt(),
			showType = r.ReadInt(),
			showList = r.ReadIntArray()
		};
	}
}
