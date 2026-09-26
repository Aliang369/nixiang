using System;

[Serializable]
public class zbPushSetCsvTableData
{
	public zbPushSetCsvRecord[] records;

	public static zbPushSetCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPushSetCsvTableData zbPushSetCsvTableData2 = new zbPushSetCsvTableData();
		zbPushSetCsvRecord[] array = new zbPushSetCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbPushSetCsvRecord.Read(zbBinReader2);
		}
		zbPushSetCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbPushSetCsvTableData2;
	}
}
