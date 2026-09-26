using System;

[Serializable]
public class zbWorldBossHurtAwardCsvTableData
{
	public zbWorldBossHurtAwardCsvRecord[] records;

	public static zbWorldBossHurtAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWorldBossHurtAwardCsvTableData zbWorldBossHurtAwardCsvTableData2 = new zbWorldBossHurtAwardCsvTableData();
		zbWorldBossHurtAwardCsvRecord[] array = new zbWorldBossHurtAwardCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbWorldBossHurtAwardCsvRecord.Read(zbBinReader2);
		}
		zbWorldBossHurtAwardCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbWorldBossHurtAwardCsvTableData2;
	}
}
