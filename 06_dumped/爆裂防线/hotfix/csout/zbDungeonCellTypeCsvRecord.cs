using System;

[Serializable]
public class zbDungeonCellTypeCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public int funType { get; set; }

	public int length { get; set; }

	public int leftDoor { get; set; }

	public int rightDoor { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbDungeonCellTypeCsvRecord Read(zbBinReader r)
	{
		return new zbDungeonCellTypeCsvRecord
		{
			type = r.ReadInt(),
			funType = r.ReadInt(),
			length = r.ReadInt(),
			leftDoor = r.ReadInt(),
			rightDoor = r.ReadInt()
		};
	}
}
