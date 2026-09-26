using System;

[Serializable]
public class zbWorldLairKillAwardCsvTableData
{
	public zbWorldLairKillAwardCsvRecord[] records;

	public static zbWorldLairKillAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWorldLairKillAwardCsvTableData zbWorldLairKillAwardCsvTableData2 = new zbWorldLairKillAwardCsvTableData();
		zbWorldLairKillAwardCsvRecord[] array = new zbWorldLairKillAwardCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbWorldLairKillAwardCsvRecord.Read(zbBinReader2);
		}
		zbWorldLairKillAwardCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbWorldLairKillAwardCsvTableData2;
	}
}
