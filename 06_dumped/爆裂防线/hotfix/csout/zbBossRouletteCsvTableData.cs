using System;

[Serializable]
public class zbBossRouletteCsvTableData
{
	public zbBossRouletteCsvRecord[] records;

	public static zbBossRouletteCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbBossRouletteCsvTableData zbBossRouletteCsvTableData2 = new zbBossRouletteCsvTableData();
		zbBossRouletteCsvRecord[] array = new zbBossRouletteCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbBossRouletteCsvRecord.Read(zbBinReader2);
		}
		zbBossRouletteCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbBossRouletteCsvTableData2;
	}
}
