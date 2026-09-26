using System;

[Serializable]
public class zbVipGoodsPriceCsvRecord : zbTableBaseRecord
{
	public int goodsId { get; set; }

	public float price { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbVipGoodsPriceCsvRecord Read(zbBinReader r)
	{
		return new zbVipGoodsPriceCsvRecord
		{
			goodsId = r.ReadInt(),
			price = r.ReadFloat()
		};
	}
}
