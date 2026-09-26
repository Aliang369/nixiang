using System;

[Serializable]
public class zbRestaurantPieceCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int unlockSlot { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbRestaurantPieceCsvRecord Read(zbBinReader r)
	{
		return new zbRestaurantPieceCsvRecord
		{
			id = r.ReadInt(),
			unlockSlot = r.ReadInt()
		};
	}
}
