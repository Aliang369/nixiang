using System;

[Serializable]
public class zbViptdescCsvTableData
{
	public zbViptdescCsvRecord[] records;

	public static zbViptdescCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbViptdescCsvTableData zbViptdescCsvTableData2 = new zbViptdescCsvTableData();
		zbViptdescCsvRecord[] array = new zbViptdescCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbViptdescCsvRecord.Read(zbBinReader2);
		}
		zbViptdescCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbViptdescCsvTableData2;
	}
}
