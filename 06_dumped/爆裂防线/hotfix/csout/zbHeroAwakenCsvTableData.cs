using System;

[Serializable]
public class zbHeroAwakenCsvTableData
{
	public zbHeroAwakenCsvRecord[] records;

	public static zbHeroAwakenCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHeroAwakenCsvTableData zbHeroAwakenCsvTableData2 = new zbHeroAwakenCsvTableData();
		zbHeroAwakenCsvRecord[] array = new zbHeroAwakenCsvRecord[zbBinReader2.RecordCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = zbHeroAwakenCsvRecord.Read(zbBinReader2);
		}
		zbHeroAwakenCsvTableData2.records = array;
		zbBinReader2.ValidateEnd();
		return zbHeroAwakenCsvTableData2;
	}
}
