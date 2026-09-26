using System;

[Serializable]
public class zbBossTaskTargetCsvRecord : zbTableBaseRecord
{
	public int target { get; set; }

	public float damageAdd { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbBossTaskTargetCsvRecord Read(zbBinReader r)
	{
		return new zbBossTaskTargetCsvRecord
		{
			target = r.ReadInt(),
			damageAdd = r.ReadFloat()
		};
	}
}
