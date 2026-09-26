using System;

[Serializable]
public class zbRoomUpgradeUtilityCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int lv { get; set; }

	public int[][] costList { get; set; }

	public int peerageId { get; set; }

	public float[] typeValue { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbRoomUpgradeUtilityCsvRecord Read(zbBinReader r)
	{
		return new zbRoomUpgradeUtilityCsvRecord
		{
			id = r.ReadInt(),
			lv = r.ReadInt(),
			costList = r.ReadIntArray2(),
			peerageId = r.ReadInt(),
			typeValue = r.ReadFloatArray()
		};
	}
}
