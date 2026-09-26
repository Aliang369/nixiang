using System;

[Serializable]
public class zbArenaGiftCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int layer { get; set; }

	public int[] award { get; set; }

	public int moneyType { get; set; }

	public int price { get; set; }

	public int refresh { get; set; }

	public int limitNum { get; set; }

	public int effect { get; set; }

	public string bonus { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbArenaGiftCsvRecord Read(zbBinReader r)
	{
		return new zbArenaGiftCsvRecord
		{
			id = r.ReadInt(),
			layer = r.ReadInt(),
			award = r.ReadIntArray(),
			moneyType = r.ReadInt(),
			price = r.ReadInt(),
			refresh = r.ReadInt(),
			limitNum = r.ReadInt(),
			effect = r.ReadInt(),
			bonus = r.ReadString()
		};
	}
}
