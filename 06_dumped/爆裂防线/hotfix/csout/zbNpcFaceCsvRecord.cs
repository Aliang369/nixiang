using System;

[Serializable]
public class zbNpcFaceCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string icon { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbNpcFaceCsvRecord Read(zbBinReader r)
	{
		return new zbNpcFaceCsvRecord
		{
			id = r.ReadInt(),
			icon = r.ReadString()
		};
	}
}
