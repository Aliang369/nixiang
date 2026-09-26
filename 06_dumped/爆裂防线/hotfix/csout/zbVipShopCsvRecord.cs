using System;

[Serializable]
public class zbVipShopCsvRecord : zbTableBaseRecord
{
	public int goodsId { get; set; }

	public int referId { get; set; }

	public string goodsName { get; set; }

	public string goodsDesc { get; set; }

	public int goodsType { get; set; }

	public int goodsSubType { get; set; }

	public float price { get; set; }

	public int voucherPrice { get; set; }

	public int voucher { get; set; }

	public int[][] awardList { get; set; }

	public string icon { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbVipShopCsvRecord Read(zbBinReader r)
	{
		return new zbVipShopCsvRecord
		{
			goodsId = r.ReadInt(),
			referId = r.ReadInt(),
			goodsName = r.ReadString(),
			goodsDesc = r.ReadString(),
			goodsType = r.ReadInt(),
			goodsSubType = r.ReadInt(),
			price = r.ReadFloat(),
			voucherPrice = r.ReadInt(),
			voucher = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			icon = r.ReadString()
		};
	}
}
