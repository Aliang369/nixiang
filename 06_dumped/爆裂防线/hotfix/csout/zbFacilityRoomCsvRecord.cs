using System;

[Serializable]
public class zbFacilityRoomCsvRecord : zbTableBaseRecord
{
	public int roomType { get; set; }

	public string spine { get; set; }

	public int explorerCost { get; set; }

	public int explorerExpire { get; set; }

	public string rem { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbFacilityRoomCsvRecord Read(zbBinReader r)
	{
		return new zbFacilityRoomCsvRecord
		{
			roomType = r.ReadInt(),
			spine = r.ReadString(),
			explorerCost = r.ReadInt(),
			explorerExpire = r.ReadInt(),
			rem = r.ReadString()
		};
	}
}
