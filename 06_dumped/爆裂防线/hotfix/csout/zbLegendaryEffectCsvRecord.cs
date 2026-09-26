using System;

[Serializable]
public class zbLegendaryEffectCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int mstSkillId { get; set; }

	public int[] mstId { get; set; }

	public string spinePath { get; set; }

	public int position { get; set; }

	public string desc { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLegendaryEffectCsvRecord Read(zbBinReader r)
	{
		return new zbLegendaryEffectCsvRecord
		{
			id = r.ReadInt(),
			mstSkillId = r.ReadInt(),
			mstId = r.ReadIntArray(),
			spinePath = r.ReadString(),
			position = r.ReadInt(),
			desc = r.ReadString()
		};
	}
}
