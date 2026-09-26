using System;

[Serializable]
public class zbSkillEffectCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string spinePath { get; set; }

	public string sequenceName { get; set; }

	public int front { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSkillEffectCsvRecord Read(zbBinReader r)
	{
		return new zbSkillEffectCsvRecord
		{
			id = r.ReadInt(),
			spinePath = r.ReadString(),
			sequenceName = r.ReadString(),
			front = r.ReadInt()
		};
	}
}
