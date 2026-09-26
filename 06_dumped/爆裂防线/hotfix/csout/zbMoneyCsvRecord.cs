using System;

[Serializable]
public class zbMoneyCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public string icon { get; set; }

	public int quality { get; set; }

	public int[] quickBuy { get; set; }

	public int show { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMoneyCsvRecord Read(zbBinReader r)
	{
		return new zbMoneyCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			icon = r.ReadString(),
			quality = r.ReadInt(),
			quickBuy = r.ReadIntArray(),
			show = r.ReadInt()
		};
	}
}
