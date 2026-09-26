using System;

[Serializable]
public class zbModHandbookTargetCsvRecord : zbTableBaseRecord
{
	public int target { get; set; }

	public int[] award { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbModHandbookTargetCsvRecord Read(zbBinReader r)
	{
		return new zbModHandbookTargetCsvRecord
		{
			target = r.ReadInt(),
			award = r.ReadIntArray()
		};
	}
}
