using System;

[Serializable]
public class zbRestaurantBuyCsvRecord : zbTableBaseRecord
{
	public int count { get; set; }

	public int off { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbRestaurantBuyCsvRecord Read(zbBinReader r)
	{
		return new zbRestaurantBuyCsvRecord
		{
			count = r.ReadInt(),
			off = r.ReadInt()
		};
	}
}
