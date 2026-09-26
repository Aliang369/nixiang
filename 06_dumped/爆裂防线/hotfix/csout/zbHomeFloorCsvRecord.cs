using System;

[Serializable]
public class zbHomeFloorCsvRecord : zbTableBaseRecord
{
	public int floor { get; set; }

	public int block1 { get; set; }

	public int block2 { get; set; }

	public int block3 { get; set; }

	public int block4 { get; set; }

	public int block5 { get; set; }

	public int block6 { get; set; }

	public int peerageId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbHomeFloorCsvRecord Read(zbBinReader r)
	{
		return new zbHomeFloorCsvRecord
		{
			floor = r.ReadInt(),
			block1 = r.ReadInt(),
			block2 = r.ReadInt(),
			block3 = r.ReadInt(),
			block4 = r.ReadInt(),
			block5 = r.ReadInt(),
			block6 = r.ReadInt(),
			peerageId = r.ReadInt()
		};
	}
}
