using System;

[Serializable]
public class zbMoneyExchangeCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int count { get; set; }

	public int costId { get; set; }

	public int cost { get; set; }

	public int batch { get; set; }

	public int half { get; set; }

	public int dailyLimit { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMoneyExchangeCsvRecord Read(zbBinReader r)
	{
		return new zbMoneyExchangeCsvRecord
		{
			id = r.ReadInt(),
			count = r.ReadInt(),
			costId = r.ReadInt(),
			cost = r.ReadInt(),
			batch = r.ReadInt(),
			half = r.ReadInt(),
			dailyLimit = r.ReadInt()
		};
	}
}
