using System;

[Serializable]
public class zbVipShopChannelCsvRecord : zbTableBaseRecord
{
	public int cid { get; set; }

	public int goodsId { get; set; }

	public string cgoodsId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbVipShopChannelCsvRecord Read(zbBinReader r)
	{
		return new zbVipShopChannelCsvRecord
		{
			cid = r.ReadInt(),
			goodsId = r.ReadInt(),
			cgoodsId = r.ReadString()
		};
	}
}
