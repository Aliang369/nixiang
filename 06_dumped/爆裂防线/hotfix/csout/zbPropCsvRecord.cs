using System;

[Serializable]
public class zbPropCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string icon { get; set; }

	public int getWay { get; set; }

	public int quality { get; set; }

	public int sellMoney { get; set; }

	public int sellPrice { get; set; }

	public int sellGroupNum { get; set; }

	public int expire { get; set; }

	public int[] operateId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPropCsvRecord Read(zbBinReader r)
	{
		return new zbPropCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			icon = r.ReadString(),
			getWay = r.ReadInt(),
			quality = r.ReadInt(),
			sellMoney = r.ReadInt(),
			sellPrice = r.ReadInt(),
			sellGroupNum = r.ReadInt(),
			expire = r.ReadInt(),
			operateId = r.ReadIntArray()
		};
	}
}
