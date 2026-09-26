using System;

[Serializable]
public class zbMonsterEffectCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string spinePath { get; set; }

	public string sequenceName { get; set; }

	public int front { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMonsterEffectCsvRecord Read(zbBinReader r)
	{
		return new zbMonsterEffectCsvRecord
		{
			id = r.ReadInt(),
			spinePath = r.ReadString(),
			sequenceName = r.ReadString(),
			front = r.ReadInt()
		};
	}
}
