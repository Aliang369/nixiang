using System;

[Serializable]
public class zbFreeGiftCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public int[] award { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbFreeGiftCsvRecord Read(zbBinReader r)
	{
		return new zbFreeGiftCsvRecord
		{
			type = r.ReadInt(),
			award = r.ReadIntArray()
		};
	}
}
