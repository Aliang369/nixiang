using System;

[Serializable]
public class zbRestaurantTargetCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string desc { get; set; }

	public int avatarId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbRestaurantTargetCsvRecord Read(zbBinReader r)
	{
		return new zbRestaurantTargetCsvRecord
		{
			id = r.ReadInt(),
			desc = r.ReadString(),
			avatarId = r.ReadInt()
		};
	}
}
