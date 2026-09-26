using System;

[Serializable]
public class zbDailyTaskTargetCsvTableData
{
	public zbDailyTaskTargetCsvRecord[] records;

	public static zbDailyTaskTargetCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbDailyTaskTargetCsvTableData zbDailyTaskTargetCsvTableData2 = new zbDailyTaskTargetCsvTableData();
		zbDailyTaskTargetCsvRecord[] array = new zbDailyTaskTargetCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbDailyTaskTargetCsvRecord.Read(zbBinReader2);
		}
		zbDailyTaskTargetCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbDailyTaskTargetCsvTableData2;
	}
}
