using System;

[Serializable]
public class zbModHandbookTypeCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public string name { get; set; }

	public string icon { get; set; }

	public int unlock { get; set; }

	public int[] award { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbModHandbookTypeCsvRecord Read(zbBinReader r)
	{
		return new zbModHandbookTypeCsvRecord
		{
			type = r.ReadInt(),
			name = r.ReadString(),
			icon = r.ReadString(),
			unlock = r.ReadInt(),
			award = r.ReadIntArray()
		};
	}
}
