using System;

[Serializable]
public class zbArenaMonsterCsvTableData
{
	public zbArenaMonsterCsvRecord[] records;

	public static zbArenaMonsterCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbArenaMonsterCsvTableData zbArenaMonsterCsvTableData2 = new zbArenaMonsterCsvTableData();
		zbArenaMonsterCsvRecord[] array = new zbArenaMonsterCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbArenaMonsterCsvRecord.Read(zbBinReader2);
		}
		zbArenaMonsterCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbArenaMonsterCsvTableData2;
	}
}
