using System;

[Serializable]
public class zbGetWayCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string icon { get; set; }

	public int index { get; set; }

	public int funcType { get; set; }

	public string link { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbGetWayCsvRecord Read(zbBinReader r)
	{
		return new zbGetWayCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			icon = r.ReadString(),
			index = r.ReadInt(),
			funcType = r.ReadInt(),
			link = r.ReadString()
		};
	}
}
