using System;

[Serializable]
public class zbModHandbookCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public string desc { get; set; }

	public int type { get; set; }

	public int unlock { get; set; }

	public int[] award { get; set; }

	public int[] requireMod { get; set; }

	public int[] conflictMod { get; set; }

	public int[][] requireHero { get; set; }

	public string icon { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbModHandbookCsvRecord Read(zbBinReader r)
	{
		return new zbModHandbookCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			desc = r.ReadString(),
			type = r.ReadInt(),
			unlock = r.ReadInt(),
			award = r.ReadIntArray(),
			requireMod = r.ReadIntArray(),
			conflictMod = r.ReadIntArray(),
			requireHero = r.ReadIntArray2(),
			icon = r.ReadString()
		};
	}
}
