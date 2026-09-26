using System;

[Serializable]
public class zbDefenseTypeCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbDefenseTypeCsvRecord Read(zbBinReader r)
	{
		return new zbDefenseTypeCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString()
		};
	}
}
