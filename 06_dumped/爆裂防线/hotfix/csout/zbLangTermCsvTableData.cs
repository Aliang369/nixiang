using System;

[Serializable]
public class zbLangTermCsvTableData
{
	public zbLangTermCsvRecord[] records;

	public static zbLangTermCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLangTermCsvTableData zbLangTermCsvTableData2 = new zbLangTermCsvTableData();
		zbLangTermCsvRecord[] array = new zbLangTermCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbLangTermCsvRecord.Read(zbBinReader2);
		}
		zbLangTermCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbLangTermCsvTableData2;
	}
}
