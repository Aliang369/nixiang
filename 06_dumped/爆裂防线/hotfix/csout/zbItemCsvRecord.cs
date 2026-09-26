using System;

[Serializable]
public class zbItemCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string icon { get; set; }

	public int quality { get; set; }

	public int[][] itemCompose { get; set; }

	public int sellMoney { get; set; }

	public int sellPrice { get; set; }

	public int sellGroupNum { get; set; }

	public int goodsId { get; set; }

	public int[] quickBuy { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbItemCsvRecord Read(zbBinReader r)
	{
		return new zbItemCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			icon = r.ReadString(),
			quality = r.ReadInt(),
			itemCompose = r.ReadIntArray2(),
			sellMoney = r.ReadInt(),
			sellPrice = r.ReadInt(),
			sellGroupNum = r.ReadInt(),
			goodsId = r.ReadInt(),
			quickBuy = r.ReadIntArray()
		};
	}
}
