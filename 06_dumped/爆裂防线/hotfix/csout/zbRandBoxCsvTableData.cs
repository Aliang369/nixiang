using System;

[Serializable]
public class zbRandBoxCsvTableData
{
	public zbRandBoxCsvRecord[] records;

	public static zbRandBoxCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbRandBoxCsvTableData zbRandBoxCsvTableData2 = new zbRandBoxCsvTableData();
		zbRandBoxCsvRecord[] array = new zbRandBoxCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbRandBoxCsvRecord.Read(zbBinReader2);
		}
		zbRandBoxCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbRandBoxCsvTableData2;
	}
}
