using System;

[Serializable]
public class zbOperateCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string shortName { get; set; }

	public string icon { get; set; }

	public string banner { get; set; }

	public string icon2 { get; set; }

	public string littleIcon { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbOperateCsvRecord Read(zbBinReader r)
	{
		return new zbOperateCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			shortName = r.ReadString(),
			icon = r.ReadString(),
			banner = r.ReadString(),
			icon2 = r.ReadString(),
			littleIcon = r.ReadString()
		};
	}
}
