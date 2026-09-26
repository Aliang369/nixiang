using System;

[Serializable]
public class zbModBattleRandCsvTableData
{
	public zbModBattleRandCsvRecord[] records;

	public static zbModBattleRandCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbModBattleRandCsvTableData zbModBattleRandCsvTableData2 = new zbModBattleRandCsvTableData();
		zbModBattleRandCsvRecord[] array = new zbModBattleRandCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbModBattleRandCsvRecord.Read(zbBinReader2);
		}
		zbModBattleRandCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbModBattleRandCsvTableData2;
	}
}
