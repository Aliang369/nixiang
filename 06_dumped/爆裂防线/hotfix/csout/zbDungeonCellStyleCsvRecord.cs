using System;

[Serializable]
public class zbDungeonCellStyleCsvRecord : zbTableBaseRecord
{
	public int styleId { get; set; }

	public string bg { get; set; }

	public string ambient { get; set; }

	public string leftdoor { get; set; }

	public string rightdoor { get; set; }

	public string middledoor { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbDungeonCellStyleCsvRecord Read(zbBinReader r)
	{
		return new zbDungeonCellStyleCsvRecord
		{
			styleId = r.ReadInt(),
			bg = r.ReadString(),
			ambient = r.ReadString(),
			leftdoor = r.ReadString(),
			rightdoor = r.ReadString(),
			middledoor = r.ReadString()
		};
	}
}
