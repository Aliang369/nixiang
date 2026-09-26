using System;

[Serializable]
public class zbWorldDistanceCsvRecord : zbTableBaseRecord
{
	public int distance { get; set; }

	public int time { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWorldDistanceCsvRecord Read(zbBinReader r)
	{
		return new zbWorldDistanceCsvRecord
		{
			distance = r.ReadInt(),
			time = r.ReadInt()
		};
	}
}
